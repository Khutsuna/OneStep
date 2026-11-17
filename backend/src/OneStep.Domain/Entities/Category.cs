using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

/// <summary>
/// Categories form a tree: a category with ParentId = null is top level,
/// and Children are its subcategories (a self-referencing relationship).
/// </summary>
public class Category : BaseEntity
{
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;           // URL-friendly name, e.g. "smart-phones"
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public int SortOrder { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
