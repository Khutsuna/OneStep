using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Notifications;

namespace OneStep.Api.Controllers;

/// <summary>Staff alerts: low stock, out of stock and new orders.</summary>
[ApiController]
[Route("api/notifications")]
[Authorize(Policy = Policies.StaffOnly)]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    /// <summary>The newest alerts, and how many are unread.</summary>
    [HttpGet]
    [ProducesResponseType<NotificationListDto>(StatusCodes.Status200OK)]
    public async Task<NotificationListDto> Get(
        CancellationToken cancellationToken,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int take = 20)
    {
        return await _notifications.GetAsync(unreadOnly, take, cancellationToken);
    }

    /// <summary>Marks one alert as read.</summary>
    /// <response code="404">No alert has this id.</response>
    [HttpPut("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        await _notifications.MarkReadAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Marks every alert as read.</summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await _notifications.MarkAllReadAsync(cancellationToken);
        return NoContent();
    }
}
