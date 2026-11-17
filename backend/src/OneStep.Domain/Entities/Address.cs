using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

/// <summary>An entry in a customer's address book.</summary>
public class Address : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Label { get; set; } = string.Empty;          // "Home", "Work"
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string CountryCode { get; set; } = "GE";
    public bool IsDefault { get; set; }
}
