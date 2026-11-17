using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

/// <summary>
/// Written by the SQL trigger trg_Products_PriceHistory whenever a price changes.
/// CreatedAt is the moment of the change.
/// </summary>
public class ProductPriceHistory : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
}
