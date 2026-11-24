using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Features.Analytics;

namespace OneStep.Infrastructure.Persistence.Queries;

/// <summary>
/// Runs the reporting stored procedures from database/04_procedures.sql.
/// The $"..." strings are NOT glued together as text: EF turns every {value} into a SQL parameter
/// (@p0, @p1...), so this is safe from SQL injection.
/// EXEC can't be combined with more LINQ, so each call goes straight to ToListAsync().
/// </summary>
public class SqlAnalyticsQueries : IAnalyticsQueries
{
    private readonly AppDbContext _db;

    public SqlAnalyticsQueries(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryRow> GetSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var rows = await _db.Database
            .SqlQuery<DashboardSummaryRow>($"EXEC dbo.usp_GetDashboardSummary @From = {from}, @To = {to}")
            .ToListAsync(cancellationToken);

        return rows.Single();
    }

    public async Task<IReadOnlyList<DailySalesRow>> GetSalesByDayAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        return await _db.Database
            .SqlQuery<DailySalesRow>($"EXEC dbo.usp_GetSalesByDay @From = {from}, @To = {to}")
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TopProductRow>> GetTopProductsAsync(DateTime from, DateTime to, int top, CancellationToken cancellationToken)
    {
        return await _db.Database
            .SqlQuery<TopProductRow>($"EXEC dbo.usp_GetTopProducts @From = {from}, @To = {to}, @Top = {top}")
            .ToListAsync(cancellationToken);
    }
}
