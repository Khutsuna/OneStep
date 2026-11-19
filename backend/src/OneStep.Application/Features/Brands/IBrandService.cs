namespace OneStep.Application.Features.Brands;

public interface IBrandService
{
    Task<IReadOnlyList<BrandDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<BrandDto> CreateAsync(BrandRequest request, CancellationToken cancellationToken);
    Task<BrandDto> UpdateAsync(int id, BrandRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
