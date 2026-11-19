using System.ComponentModel.DataAnnotations;

namespace OneStep.Application.Features.Brands;

public record BrandDto(int Id, string Name, string Slug, string? LogoUrl, int ProductCount);

public class BrandRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
