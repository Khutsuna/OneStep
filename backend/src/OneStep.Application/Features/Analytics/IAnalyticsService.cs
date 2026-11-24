namespace OneStep.Application.Features.Analytics;

public interface IAnalyticsService
{
    Task<DashboardDto> GetDashboardAsync(DashboardRequest request, CancellationToken cancellationToken);
}
