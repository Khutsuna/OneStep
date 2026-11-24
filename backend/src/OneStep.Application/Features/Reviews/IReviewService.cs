namespace OneStep.Application.Features.Reviews;

public interface IReviewService
{
    Task<ProductReviewsDto> GetForProductAsync(int productId, int page, int pageSize, CancellationToken cancellationToken);
    Task<ReviewDto> CreateAsync(int productId, ReviewRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
