using OneStep.Application.Features.Products;

namespace OneStep.Application.Features.Wishlist;

public interface IWishlistService
{
    Task<IReadOnlyList<ProductListItemDto>> GetAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<int>> GetProductIdsAsync(CancellationToken cancellationToken);
    Task AddAsync(int productId, CancellationToken cancellationToken);
    Task RemoveAsync(int productId, CancellationToken cancellationToken);
}
