using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Entities;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Categories;

public class CategoryService : ICategoryService
{
    private readonly IAppDbContext _db;

    public CategoryService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CategoryTreeDto>> GetTreeAsync(CancellationToken cancellationToken)
    {
        // There are only a few dozen categories, so load them all and build the tree in memory.
        var categories = await _db.Categories.AsNoTracking().ToListAsync(cancellationToken);

        var productCounts = await _db.Products
            .Where(p => p.Status == ProductStatus.Active)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

        return BuildChildren(null, categories, productCounts);
    }

    public async Task<CategoryTreeDto> CreateAsync(CategoryRequest request, CancellationToken cancellationToken)
    {
        await ValidateParentAsync(request.ParentId, categoryId: null, cancellationToken);
        await EnsureNameIsFreeAsync(request.Name, request.ParentId, categoryId: null, cancellationToken);

        var category = new Category
        {
            Name = request.Name.Trim(),
            ParentId = request.ParentId,
            Description = request.Description?.Trim(),
            SortOrder = request.SortOrder,
            Slug = await SlugHelper.CreateUniqueAsync(request.Name,
                slug => _db.Categories.AnyAsync(c => c.Slug == slug, cancellationToken))
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(category, productCount: 0);
    }

    public async Task<CategoryTreeDto> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Category {id} was not found.");

        await ValidateParentAsync(request.ParentId, id, cancellationToken);
        await EnsureNameIsFreeAsync(request.Name, request.ParentId, id, cancellationToken);

        if (!string.Equals(category.Name, request.Name.Trim(), StringComparison.Ordinal))
        {
            category.Slug = await SlugHelper.CreateUniqueAsync(request.Name,
                slug => _db.Categories.AnyAsync(c => c.Slug == slug && c.Id != id, cancellationToken));
        }

        category.Name = request.Name.Trim();
        category.ParentId = request.ParentId;
        category.Description = request.Description?.Trim();
        category.SortOrder = request.SortOrder;
        await _db.SaveChangesAsync(cancellationToken);

        var productCount = await _db.Products.CountAsync(p => p.CategoryId == id, cancellationToken);
        return ToDto(category, productCount);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Category {id} was not found.");

        if (await _db.Categories.AnyAsync(c => c.ParentId == id, cancellationToken))
        {
            throw new ConflictException("This category still has subcategories. Move or delete them first.");
        }

        // IgnoreQueryFilters: soft-deleted products still point to the category in the database.
        if (await _db.Products.IgnoreQueryFilters().AnyAsync(p => p.CategoryId == id, cancellationToken))
        {
            throw new ConflictException("This category still contains products. Move them to another category first.");
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetSelfAndDescendantIdsAsync(int categoryId, CancellationToken cancellationToken)
    {
        var parentOf = await _db.Categories.AsNoTracking()
            .Select(c => new { c.Id, c.ParentId })
            .ToListAsync(cancellationToken);

        var result = new List<int> { categoryId };
        var queue = new Queue<int>(result);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in parentOf.Where(c => c.ParentId == current))
            {
                result.Add(child.Id);
                queue.Enqueue(child.Id);
            }
        }
        return result;
    }

    private async Task ValidateParentAsync(int? parentId, int? categoryId, CancellationToken cancellationToken)
    {
        if (parentId is null)
        {
            return;
        }

        if (!await _db.Categories.AnyAsync(c => c.Id == parentId, cancellationToken))
        {
            throw new BadRequestException($"Parent category {parentId} doesn't exist.");
        }

        // A category can't be moved under itself or under one of its own subcategories (that would make a loop).
        if (categoryId is not null)
        {
            var ownSubtree = await GetSelfAndDescendantIdsAsync(categoryId.Value, cancellationToken);
            if (ownSubtree.Contains(parentId.Value))
            {
                throw new BadRequestException("A category can't be placed inside itself or its own subcategory.");
            }
        }
    }

    private async Task EnsureNameIsFreeAsync(string name, int? parentId, int? categoryId, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();
        var taken = await _db.Categories.AnyAsync(
            c => c.Name == trimmed && c.ParentId == parentId && c.Id != categoryId, cancellationToken);
        if (taken)
        {
            throw new ConflictException($"A category named '{trimmed}' already exists at this level.");
        }
    }

    private static List<CategoryTreeDto> BuildChildren(
        int? parentId, List<Category> all, IReadOnlyDictionary<int, int> productCounts)
    {
        return all
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c =>
            {
                var children = BuildChildren(c.Id, all, productCounts);
                var ownCount = productCounts.GetValueOrDefault(c.Id);
                var total = ownCount + children.Sum(child => child.ProductCount);
                return new CategoryTreeDto(c.Id, c.ParentId, c.Name, c.Slug, c.Description,
                    MediaUrl.From(c.ImagePath), c.SortOrder, total, children);
            })
            .ToList();
    }

    private static CategoryTreeDto ToDto(Category c, int productCount)
    {
        return new CategoryTreeDto(c.Id, c.ParentId, c.Name, c.Slug, c.Description,
            MediaUrl.From(c.ImagePath), c.SortOrder, productCount, []);
    }
}
