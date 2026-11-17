namespace OneStep.Domain.Enums;

/// <summary>
/// Normal flow: Pending -> Paid -> Shipped -> Delivered.
/// Cancelled is allowed while the order is Pending or Paid.
/// </summary>
public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}
