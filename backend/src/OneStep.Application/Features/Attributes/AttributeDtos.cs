using System.ComponentModel.DataAnnotations;

namespace OneStep.Application.Features.Attributes;

public record AttributeDto(int Id, string Name);

public class AttributeRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
