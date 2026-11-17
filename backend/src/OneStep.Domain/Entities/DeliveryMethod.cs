using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

public class DeliveryMethod : BaseEntity
{
    public string Name { get; set; } = string.Empty;           // "Courier", "Store pickup"
    public string? Description { get; set; }
    public decimal Price { get; set; }

    /// <summary>Shipping is free when the order subtotal reaches this amount. Null = never free.</summary>
    public decimal? FreeOverAmount { get; set; }

    public byte MinDays { get; set; }
    public byte MaxDays { get; set; }
    public bool IsActive { get; set; } = true;
}
