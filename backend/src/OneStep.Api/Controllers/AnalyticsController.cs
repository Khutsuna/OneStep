using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Analytics;

namespace OneStep.Api.Controllers;

/// <summary>Sales analytics for the admin dashboard. Admins and store managers only.</summary>
[ApiController]
[Route("api/analytics")]
[Authorize(Policy = Policies.StaffOnly)]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;

    public AnalyticsController(IAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    /// <summary>Key numbers, revenue per day and best sellers for the last N days (default 30).</summary>
    /// <response code="401">No valid token.</response>
    /// <response code="403">Logged in, but not staff.</response>
    [HttpGet("dashboard")]
    [ProducesResponseType<DashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<DashboardDto> GetDashboard([FromQuery] DashboardRequest request, CancellationToken cancellationToken)
    {
        return await _analytics.GetDashboardAsync(request, cancellationToken);
    }
}
