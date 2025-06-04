namespace NotificationService.Application.DTOs.Notification.Requests;

public record UpdateNotificationRequest(long Id, bool? IsRead, bool? IsDelivered, string? Content);