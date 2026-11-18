using OneStep.Domain.Common;
using OneStep.Domain.Enums;

namespace OneStep.Domain.Entities;

/// <summary>An alert for staff, e.g. "iPhone 17 is low on stock (3 left)".</summary>
public class Notification : BaseEntity
{
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public bool IsRead { get; set; }
}
