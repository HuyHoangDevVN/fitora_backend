using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs.Notification.Requests;

public record CreateNotificationRequest(
    Guid UserId,
    Guid? SenderId,
    int NotificationTypeId,
    Guid? ObjectId,
    string? Title,
    string? Content,
    NotificationChannel Channel);