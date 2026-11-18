using OneStep.Domain.Common;
using OneStep.Domain.Enums;

namespace OneStep.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;            // unique stock-keeping code
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }                         // without VAT

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    /// <summary>Soft delete: the row stays so old orders can still show it.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Changed by SQL Server on every update. EF uses it to detect when two people
    /// edit (or buy) the same product at the same time.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
