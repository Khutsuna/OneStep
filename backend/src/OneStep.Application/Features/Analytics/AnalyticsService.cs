using OneStep.Application.Abstractions;

namespace OneStep.Application.Features.Analytics;

/// <summary>
/// Builds the admin dashboard from the three stored procedures.
/// SQL does the heavy work (sums and grouping); C# only shapes the result.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private const int TopProductCount = 5;

    private readonly IAnalyticsQueries _queries;

    public AnalyticsService(IAnalyticsQueries queries)
    {
        _queries = queries;
    }

    public async Task<DashboardDto> GetDashboardAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        // The period: the last N days including today. "to" is the start of tomorrow (exclusive).
        var to = DateTime.UtcNow.Date.AddDays(1);
        var from = to.AddDays(-request.Days);

        // One after another, not in parallel: a DbContext can only run one query at a time.
        var summary = await _queries.GetSummaryAsync(from, to, cancellationToken);
        var sales = await _queries.GetSalesByDayAsync(from, to, cancellationToken);
        var topProducts = await _queries.GetTopProductsAsync(from, to, TopProductCount, cancellationToken);

        var averageOrderValue = summary.PaidOrderCount == 0
            ? 0m
            : Math.Round(summary.Revenue / summary.PaidOrderCount, 2, MidpointRounding.AwayFromZero);

        return new DashboardDto(
            DateOnly.FromDateTime(from),
            DateOnly.FromDateTime(to.AddDays(-1)),
            new DashboardSummaryDto(
                summary.Revenue,
                summary.PaidOrderCount,
                summary.OrderCount,
                averageOrderValue,
                summary.NewCustomers,
                summary.PendingOrders,
                summary.OrdersToShip,
                summary.LowStockProducts),
            FillMissingDays(sales, DateOnly.FromDateTime(from), request.Days),
            topProducts.Select(p => new TopProductDto(p.ProductId, p.ProductName, p.UnitsSold, p.Revenue)).ToList());
    }

    /// <summary>
    /// The procedure only returns days that had sales. The chart needs every day,
    /// so days without a row get zero. Pure function, so it's unit tested.
    /// </summary>
    public static IReadOnlyList<DailySalesDto> FillMissingDays(IEnumerable<DailySalesRow> rows, DateOnly firstDay, int days)
    {
        var byDay = rows.ToDictionary(r => DateOnly.FromDateTime(r.Day));

        return Enumerable.Range(0, days)
            .Select(offset => firstDay.AddDays(offset))
            .Select(day => byDay.TryGetValue(day, out var row)
                ? new DailySalesDto(day, row.OrderCount, row.Revenue)
                : new DailySalesDto(day, 0, 0m))
            .ToList();
    }
}
