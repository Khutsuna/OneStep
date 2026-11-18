using OneStep.Domain.Common;
using OneStep.Domain.Enums;
using OneStep.Domain.ValueObjects;

namespace OneStep.Domain.Entities;

public class Order : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // The money breakdown is stored, not recalculated, so receipts never change
    // when prices, discounts or the tax rate change later.
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public int DeliveryMethodId { get; set; }
    public DeliveryMethod DeliveryMethod { get; set; } = null!;

    public PaymentMethod PaymentMethod { get; set; }
    public ShippingAddress ShippingAddress { get; set; } = new();
    public string? CustomerNote { get; set; }
    public DateTime? PaidAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
