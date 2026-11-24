using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common.Exceptions;

namespace OneStep.Application.Features.Notifications;

/// <summary>
/// Staff alerts. They are created elsewhere: by checkout and product edits (StockAlerts)
/// and by checkout (new order). This service only lists them and marks them as read.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IAppDbContext _db;

    public NotificationService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<NotificationListDto> GetAsync(bool unreadOnly, int take, CancellationToken cancellationToken)
    {
        var query = _db.Notifications.AsNoTracking();
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Take(Math.Clamp(take, 1, 100))
            .Select(n => new NotificationDto(n.Id, n.Type, n.Message, n.ProductId, n.OrderId, n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken);

        var unreadCount = await _db.Notifications.CountAsync(n => !n.IsRead, cancellationToken);

        return new NotificationListDto(items, unreadCount);
    }

    public async Task MarkReadAsync(int id, CancellationToken cancellationToken)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Notification {id} was not found.");

        notification.IsRead = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(CancellationToken cancellationToken)
    {
        var unread = await _db.Notifications.Where(n => !n.IsRead).ToListAsync(cancellationToken);
        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }
}
