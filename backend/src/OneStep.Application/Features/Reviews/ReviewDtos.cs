using System.ComponentModel.DataAnnotations;
using OneStep.Application.Common;

namespace OneStep.Application.Features.Reviews;

/// <summary>One review. The author is shown as "Nino B." so full names stay private.</summary>
public record ReviewDto(
    int Id,
    int ProductId,
    int Rating,
    string? Title,
    string? Comment,
    string AuthorName,
    DateTime CreatedAt,
    bool IsMine);

/// <summary>The reviews section of a product page.</summary>
public record ProductReviewsDto(
    decimal AverageRating,
    int ReviewCount,
    IReadOnlyList<int> RatingCounts,       // how many 5-star, 4-star, 3-star, 2-star and 1-star reviews
    PagedResult<ReviewDto> Reviews,
    bool CanReview,                        // show the "write a review" form?
    string? CannotReviewReason);           // if not, why (shown to the customer)

public class ReviewRequest
{
    [Required]
    [Range(1, 5)]
    public int? Rating { get; set; }

    [StringLength(150)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }
}
