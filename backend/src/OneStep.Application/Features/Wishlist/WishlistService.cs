using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common.Exceptions;
using OneStep.Application.Features.Products;
using OneStep.Domain.Entities;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Wishlist;

/// <summary>
/// The logged-in customer's wishlist. Adding and removing are idempotent:
/// adding a product twice, or removing one that isn't there, is not an error.
/// That makes the heart button safe to click quickly.
/// </summary>
public class WishlistService : IWishlistService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IProductService _products;

    public WishlistService(IAppDbContext db, ICurrentUser currentUser, IProductService products)
    {
        _db = db;
        _currentUser = currentUser;
        _products = products;
    }

    private int UserId => _currentUser.UserId ?? throw new UnauthorizedException("Please log in.");

    public async Task<IReadOnlyList<ProductListItemDto>> GetAsync(CancellationToken cancellationToken)
    {
        var ids = await GetProductIdsAsync(cancellationToken);
        // Reuses the product list logic, so wishlist cards show the same current prices as the shop.
        return await _products.GetByIdsAsync(ids, cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetProductIdsAsync(CancellationToken cancellationToken)
    {
        return await _db.WishlistItems
            .AsNoTracking()
            .Where(w => w.UserId == UserId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => w.ProductId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(int productId, CancellationToken cancellationToken)
    {
        var productExists = await _db.Products
            .AnyAsync(p => p.Id == productId && p.Status == ProductStatus.Active, cancellationToken);
        if (!productExists)
        {
            throw new NotFoundException($"Product {productId} was not found.");
        }

        var alreadySaved = await _db.WishlistItems
            .AnyAsync(w => w.UserId == UserId && w.ProductId == productId, cancellationToken);
        if (alreadySaved)
        {
            return;
        }

        _db.WishlistItems.Add(new WishlistItem
        {
            UserId = UserId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Two quick clicks: the other request already saved it (primary key UserId + ProductId). Fine.
        }
    }

    public async Task RemoveAsync(int productId, CancellationToken cancellationToken)
    {
        var item = await _db.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == UserId && w.ProductId == productId, cancellationToken);
        if (item is null)
        {
            return;
        }

        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
