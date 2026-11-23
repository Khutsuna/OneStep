using OneStep.Domain.Enums;

namespace OneStep.Domain.Rules;

/// <summary>
/// The order status "state machine": which status can follow which.
///   Pending -> Paid -> Shipped -> Delivered
///   Pending or Paid -> Cancelled
/// Every status change in the app goes through this one class, so an illegal jump
/// (e.g. Delivered -> Pending) is impossible. Pure logic, no database: easy to unit test.
/// </summary>
public static class OrderStatusTransitions
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Allowed = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Paid, OrderStatus.Cancelled],
        [OrderStatus.Paid] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped] = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = []
    };

    public static bool CanMove(OrderStatus from, OrderStatus to)
    {
        return Allowed[from].Contains(to);
    }

    public static IReadOnlyList<OrderStatus> NextStatuses(OrderStatus from)
    {
        return Allowed[from];
    }
}
