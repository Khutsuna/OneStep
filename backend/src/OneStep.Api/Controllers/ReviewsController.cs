using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Reviews;

namespace OneStep.Api.Controllers;

/// <summary>Product ratings and reviews.</summary>
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviews;

    public ReviewsController(IReviewService reviews)
    {
        _reviews = reviews;
    }

    /// <summary>A product's reviews, the rating summary, and whether the current user may write one.</summary>
    /// <response code="404">No product has this id.</response>
    [HttpGet("api/products/{productId:int}/reviews")]
    [AllowAnonymous]
    [ProducesResponseType<ProductReviewsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ProductReviewsDto> GetForProduct(
        int productId,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5)
    {
        return await _reviews.GetForProductAsync(productId, page, pageSize, cancellationToken);
    }

    /// <summary>Writes a review. Only for customers who received the product, once per product.</summary>
    /// <response code="403">The customer hasn't received this product.</response>
    /// <response code="409">The customer already reviewed this product.</response>
    [HttpPost("api/products/{productId:int}/reviews")]
    [Authorize(Policy = Policies.CustomerOnly)]
    [ProducesResponseType<ReviewDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewDto>> Create(int productId, ReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await _reviews.CreateAsync(productId, request, cancellationToken);
        return CreatedAtAction(nameof(GetForProduct), new { productId }, review);
    }

    /// <summary>Deletes a review: your own, or any review if you're staff (moderation).</summary>
    /// <response code="403">Not your review, and you're not staff.</response>
    [HttpDelete("api/reviews/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _reviews.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
