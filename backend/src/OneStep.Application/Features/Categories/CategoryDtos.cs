using System.ComponentModel.DataAnnotations;

namespace OneStep.Application.Features.Categories;

/// <summary>A category with its subcategories nested inside (the tree the shop menu shows).</summary>
public record CategoryTreeDto(
    int Id,
    int? ParentId,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    int ProductCount,
    IReadOnlyList<CategoryTreeDto> Children);

public class CategoryRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Null for a top-level category.</summary>
    public int? ParentId { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, 10_000)]
    public int SortOrder { get; set; }
}
