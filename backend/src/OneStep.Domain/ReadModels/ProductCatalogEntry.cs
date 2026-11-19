using OneStep.Domain.Enums;

namespace OneStep.Domain.ReadModels;

/// <summary>
/// One row of the SQL view dbo.vw_ProductCatalog (database/03_views.sql).
/// It is read-only and has no key of its own: EF maps it to the view, not to a table,
/// so the product search runs on top of the SQL you wrote.
/// </summary>
public class ProductCatalogEntry
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus Status { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }
    public string? MainImagePath { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
