using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Entities;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Reviews;

/// <summary>
/// Ratings and reviews. Only customers who received the product (a Delivered order) can review it,
/// once per product ("verified purchase"). Authors can delete their review; staff can delete any (moderation).
/// </summary>
public class ReviewService : IReviewService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ReviewService(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private int UserId => _currentUser.UserId ?? throw new UnauthorizedException("Please log in.");
    private bool IsStaff => _currentUser.Role is "Admin" or "StoreManager";

    // =====================================================================
    //  [You] Write a review. Replace this method with the code from the chat.
    // =====================================================================
    public async Task<ReviewDto> CreateAsync(int productId, ReviewRequest request, CancellationToken cancellationToken)
    {
        // 1. The product must exist.
        await EnsureProductExistsAsync(productId, cancellationToken);

        // 2. The business rule: the customer received the product and hasn't reviewed it yet.
        var (received, reviewed) = await GetReviewStatusAsync(productId, UserId, cancellationToken);
        var reason = ReviewRules.WhyCannotReview(received, reviewed);
        if (reason is not null)
        {
            // Already reviewed = 409 Conflict. Never received = 403 Forbidden.
            if (reviewed)
            {
                throw new ConflictException(reason);
            }
            throw new ForbiddenException(reason);
        }

        // 3. Save it. Empty text becomes null.
        var review = new Review
        {
            ProductId = productId,
            UserId = UserId,
            Rating = (byte)request.Rating!.Value,
            Title = Clean(request.Title),
            Comment = Clean(request.Comment)
        };
        _db.Reviews.Add(review);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Two submits at the same moment: the database's unique index (ProductId, UserId) stops the second.
            throw new ConflictException(ReviewRules.AlreadyReviewedMessage);
        }

        // 4. Return it with the author's short name ("Nino B.").
        return await GetDtoAsync(review.Id, cancellationToken);
    }

    public async Task<ProductReviewsDto> GetForProductAsync(int productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        await EnsureProductExistsAsync(productId, cancellationToken);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var reviews = _db.Reviews.AsNoTracking().Where(r => r.ProductId == productId);

        // How many reviews of each star rating (for the bars next to the average).
        var counts = await reviews
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var ratingCounts = Enumerable.Range(1, 5).Reverse()
            .Select(stars => counts.FirstOrDefault(c => c.Rating == stars)?.Count ?? 0)
            .ToList();
        var total = ratingCounts.Sum();
        var average = total == 0
            ? 0m
            : Math.Round((decimal)counts.Sum(c => c.Rating * c.Count) / total, 2, MidpointRounding.AwayFromZero);

        var currentUserId = _currentUser.UserId;
        var items = await reviews
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id, r.ProductId, r.Rating, r.Title, r.Comment, r.CreatedAt, r.UserId,
                r.User.FirstName, r.User.LastName
            })
            .ToListAsync(cancellationToken);

        var dtos = items
            .Select(r => new ReviewDto(r.Id, r.ProductId, r.Rating, r.Title, r.Comment,
                ReviewRules.DisplayName(r.FirstName, r.LastName), r.CreatedAt, r.UserId == currentUserId))
            .ToList();

        // Can the person looking at the page write a review?
        var canReview = false;
        string? reason = null;
        if (currentUserId is null)
        {
            reason = "Log in to write a review.";
        }
        else if (_currentUser.Role == "Customer")
        {
            var (received, reviewed) = await GetReviewStatusAsync(productId, currentUserId.Value, cancellationToken);
            reason = ReviewRules.WhyCannotReview(received, reviewed);
            canReview = reason is null;
        }

        return new ProductReviewsDto(
            average, total, ratingCounts,
            new PagedResult<ReviewDto>(dtos, page, pageSize, total),
            canReview, reason);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Review {id} was not found.");

        if (review.UserId != UserId && !IsStaff)
        {
            throw new ForbiddenException("You can only delete your own reviews.");
        }

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync(cancellationToken);
    }

    // ---------- helpers for your code ----------

    /// <summary>Has this customer received the product, and have they reviewed it already?</summary>
    private async Task<(bool Received, bool Reviewed)> GetReviewStatusAsync(
        int productId, int userId, CancellationToken cancellationToken)
    {
        var received = await _db.OrderItems.AnyAsync(i =>
            i.ProductId == productId &&
            i.Order.UserId == userId &&
            i.Order.Status == OrderStatus.Delivered, cancellationToken);

        var reviewed = await _db.Reviews.AnyAsync(r =>
            r.ProductId == productId && r.UserId == userId, cancellationToken);

        return (received, reviewed);
    }

    private async Task EnsureProductExistsAsync(int productId, CancellationToken cancellationToken)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == productId, cancellationToken))
        {
            throw new NotFoundException($"Product {productId} was not found.");
        }
    }

    /// <summary>Loads one review as a DTO (with the author's short name).</summary>
    private async Task<ReviewDto> GetDtoAsync(int id, CancellationToken cancellationToken)
    {
        var r = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new { r.Id, r.ProductId, r.Rating, r.Title, r.Comment, r.CreatedAt, r.UserId, r.User.FirstName, r.User.LastName })
            .FirstAsync(cancellationToken);

        return new ReviewDto(r.Id, r.ProductId, r.Rating, r.Title, r.Comment,
            ReviewRules.DisplayName(r.FirstName, r.LastName), r.CreatedAt, r.UserId == _currentUser.UserId);
    }

    /// <summary>Empty text becomes null, other text is trimmed.</summary>
    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
