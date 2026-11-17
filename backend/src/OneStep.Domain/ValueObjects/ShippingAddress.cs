namespace OneStep.Domain.ValueObjects;

/// <summary>
/// A copy of the delivery address, frozen onto the order when it is placed.
/// It is not a table of its own: EF stores it as Ship* columns of the Orders table.
/// It is copied (not linked) so that editing the address book later never changes old orders.
/// </summary>
public class ShippingAddress
{
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string CountryCode { get; set; } = "GE";
}
