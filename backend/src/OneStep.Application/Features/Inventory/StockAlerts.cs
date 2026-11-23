using OneStep.Domain.Entities;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Inventory;

/// <summary>
/// Decides when stock changes need a staff notification. Used by product editing and by checkout.
/// An alert is created only when stock CROSSES the line, so staff don't get a new alert on every sale.
/// </summary>
public static class StockAlerts
{
    public static Notification? CreateIfNeeded(Product product, int previousQuantity)
    {
        if (product.StockQuantity == 0 && previousQuantity > 0)
        {
            return new Notification
            {
                Type = NotificationType.OutOfStock,
                Message = $"{product.Name} is out of stock.",
                Product = product
            };
        }

        if (product.StockQuantity > 0
            && product.StockQuantity <= product.LowStockThreshold
            && previousQuantity > product.LowStockThreshold)
        {
            return new Notification
            {
                Type = NotificationType.LowStock,
                Message = $"{product.Name} is low on stock ({product.StockQuantity} left).",
                Product = product
            };
        }

        return null;
    }
}
