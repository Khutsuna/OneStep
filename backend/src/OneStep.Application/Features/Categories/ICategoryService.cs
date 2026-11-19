namespace OneStep.Application.Features.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryTreeDto>> GetTreeAsync(CancellationToken cancellationToken);
    Task<CategoryTreeDto> CreateAsync(CategoryRequest request, CancellationToken cancellationToken);
    Task<CategoryTreeDto> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);

    /// <summary>The category plus all of its subcategories, at any depth.</summary>
    Task<IReadOnlyList<int>> GetSelfAndDescendantIdsAsync(int categoryId, CancellationToken cancellationToken);
}
