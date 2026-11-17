using OneStep.Domain.Common;

namespace OneStep.Domain.Entities;

/// <summary>
/// One line of an order. Name, SKU and prices are copied from the product
/// at order time, so the order still shows what was bought even if the product changes.
/// </summary>
public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal UnitDiscount { get; set; }
    public int Quantity { get; set; }

    /// <summary>Calculated by SQL Server: (UnitPrice - UnitDiscount) * Quantity.</summary>
    public decimal LineTotal { get; private set; }
}
