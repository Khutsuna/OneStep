using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoPath { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
