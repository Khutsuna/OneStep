using OneStep.Application.Features.Analytics;

namespace OneStep.Application.Abstractions;

/// <summary>
/// The sales reports, which run as stored procedures in SQL Server (database/04_procedures.sql).
/// Calling raw SQL needs EF's SQL Server package, which only Infrastructure references,
/// so Application defines this interface and Infrastructure implements it (SqlAnalyticsQueries).
/// </summary>
public interface IAnalyticsQueries
{
    Task<DashboardSummaryRow> GetSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken);

    Task<IReadOnlyList<DailySalesRow>> GetSalesByDayAsync(DateTime from, DateTime to, CancellationToken cancellationToken);

    Task<IReadOnlyList<TopProductRow>> GetTopProductsAsync(DateTime from, DateTime to, int top, CancellationToken cancellationToken);
}
