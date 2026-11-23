using OneStep.Domain.Enums;
using OneStep.Domain.Rules;

namespace OneStep.UnitTests.Orders;

/// <summary>[Theory] runs the same test once per [InlineData] row.</summary>
public class OrderStatusTransitionsTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Paid)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Paid, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Paid, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    public void Allowed_moves(OrderStatus from, OrderStatus to)
    {
        Assert.True(OrderStatusTransitions.CanMove(from, to));
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped)]      // can't ship before payment
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled)]    // already on its way
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending)]    // no going back
    [InlineData(OrderStatus.Cancelled, OrderStatus.Paid)]
    [InlineData(OrderStatus.Paid, OrderStatus.Paid)]            // no "change" to the same status
    public void Forbidden_moves(OrderStatus from, OrderStatus to)
    {
        Assert.False(OrderStatusTransitions.CanMove(from, to));
    }

    [Fact]
    public void Delivered_and_cancelled_are_final()
    {
        Assert.Empty(OrderStatusTransitions.NextStatuses(OrderStatus.Delivered));
        Assert.Empty(OrderStatusTransitions.NextStatuses(OrderStatus.Cancelled));
    }
}
