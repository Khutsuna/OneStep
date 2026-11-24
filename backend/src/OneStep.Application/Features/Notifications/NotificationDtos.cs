using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Notifications;

/// <summary>A staff alert: low stock, out of stock or a new order.</summary>
public record NotificationDto(
    int Id,
    NotificationType Type,
    string Message,
    int? ProductId,
    int? OrderId,
    bool IsRead,
    DateTime CreatedAt);

public record NotificationListDto(IReadOnlyList<NotificationDto> Items, int UnreadCount);
