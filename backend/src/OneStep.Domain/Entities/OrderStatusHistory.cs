using OneStep.Domain.Common;
using OneStep.Domain.Enums;

namespace OneStep.Domain.Entities;

/// <summary>
/// One row per status change. Powers the order-tracking timeline the customer sees.
/// CreatedAt is the moment of the change.
/// </summary>
public class OrderStatusHistory : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public OrderStatus? FromStatus { get; set; }               // null for the first row
    public OrderStatus ToStatus { get; set; }

    public int? ChangedByUserId { get; set; }
    public User? ChangedByUser { get; set; }

    public string? Note { get; set; }
}
