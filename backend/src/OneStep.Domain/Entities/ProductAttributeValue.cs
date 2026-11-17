namespace OneStep.Domain.Entities;

/// <summary>
/// One product's value for one attribute, e.g. (iPhone 17, Color) = "Black".
/// Its primary key is the pair (ProductId, ProductAttributeId), so it has no Id of its own.
/// </summary>
public class ProductAttributeValue
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int ProductAttributeId { get; set; }
    public ProductAttribute ProductAttribute { get; set; } = null!;

    public string Value { get; set; } = string.Empty;
}
