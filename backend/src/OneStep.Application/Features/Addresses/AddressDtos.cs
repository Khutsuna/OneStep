using System.ComponentModel.DataAnnotations;

namespace OneStep.Application.Features.Addresses;

public record AddressDto(
    int Id,
    string Label,
    string RecipientName,
    string Phone,
    string Line1,
    string? Line2,
    string City,
    string? Region,
    string? PostalCode,
    string CountryCode,
    bool IsDefault);

public class AddressRequest
{
    [Required]
    [StringLength(50)]
    public string Label { get; set; } = "Home";

    [Required]
    [StringLength(150)]
    public string RecipientName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Line1 { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Line2 { get; set; }

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Region { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string CountryCode { get; set; } = "GE";

    public bool IsDefault { get; set; }
}
