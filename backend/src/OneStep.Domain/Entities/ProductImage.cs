using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string FilePath { get; set; } = string.Empty;       // e.g. "products/12/3f9c....jpg"
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsMain { get; set; }
}
