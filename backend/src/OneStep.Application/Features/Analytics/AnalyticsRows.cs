namespace OneStep.Application.Features.Analytics;

// The rows the stored procedures return. EF fills them by matching column names to property names,
// so these names must stay the same as the columns in database/04_procedures.sql.

/// <summary>One row from dbo.usp_GetDashboardSummary.</summary>
public class DashboardSummaryRow
{
    public decimal Revenue { get; set; }
    public int PaidOrderCount { get; set; }
    public int OrderCount { get; set; }
    public int NewCustomers { get; set; }
    public int PendingOrders { get; set; }
    public int OrdersToShip { get; set; }
    public int LowStockProducts { get; set; }
}

/// <summary>One row from dbo.usp_GetSalesByDay.</summary>
public class DailySalesRow
{
    public DateTime Day { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>One row from dbo.usp_GetTopProducts.</summary>
public class TopProductRow
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
}
