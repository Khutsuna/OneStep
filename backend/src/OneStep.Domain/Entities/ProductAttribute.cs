using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

/// <summary>
/// A kind of characteristic, such as "Color" or "Storage".
/// Named ProductAttribute (not Attribute) to avoid clashing with .NET's System.Attribute.
/// </summary>
public class ProductAttribute : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
}
