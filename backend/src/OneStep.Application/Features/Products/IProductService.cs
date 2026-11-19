using OneStep.Application.Common;

namespace OneStep.Application.Features.Products;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> SearchAsync(ProductSearchRequest request, CancellationToken cancellationToken);
    Task<ProductDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ProductFiltersDto> GetFiltersAsync(int? categoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductListItemDto>> GetSimilarAsync(int id, int count, CancellationToken cancellationToken);
    Task<IReadOnlyList<LowStockProductDto>> GetLowStockAsync(CancellationToken cancellationToken);
    Task<ProductDetailsDto> CreateAsync(ProductRequest request, CancellationToken cancellationToken);
    Task<ProductDetailsDto> UpdateAsync(int id, ProductRequest request, CancellationToken cancellationToken);
    Task UpdateStockAsync(int id, StockUpdateRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
