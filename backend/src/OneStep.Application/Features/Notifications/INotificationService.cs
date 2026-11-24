namespace OneStep.Application.Features.Notifications;

public interface INotificationService
{
    Task<NotificationListDto> GetAsync(bool unreadOnly, int take, CancellationToken cancellationToken);
    Task MarkReadAsync(int id, CancellationToken cancellationToken);
    Task MarkAllReadAsync(CancellationToken cancellationToken);
}
