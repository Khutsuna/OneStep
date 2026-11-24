using System.ComponentModel.DataAnnotations;

namespace OneStep.Application.Features.Analytics;

public class DashboardRequest
{
    /// <summary>How many days back, including today (7, 30, 90...).</summary>
    [Range(1, 365)]
    public int Days { get; set; } = 30;
}

/// <summary>The key numbers at the top of the admin dashboard.</summary>
public record DashboardSummaryDto(
    decimal Revenue,
    int PaidOrderCount,
    int OrderCount,
    decimal AverageOrderValue,
    int NewCustomers,
    int PendingOrders,
    int OrdersToShip,
    int LowStockProducts);

public record DailySalesDto(DateOnly Day, int OrderCount, decimal Revenue);

public record TopProductDto(int ProductId, string ProductName, int UnitsSold, decimal Revenue);

/// <summary>Everything the dashboard shows, in one response.</summary>
public record DashboardDto(
    DateOnly From,
    DateOnly To,
    DashboardSummaryDto Summary,
    IReadOnlyList<DailySalesDto> SalesByDay,
    IReadOnlyList<TopProductDto> TopProducts);
