namespace NotificationService.Application.DTOs.NotificationType.Requests;

public record UpdateNotificationTypeRequest(int Id, string? Code, string? Name);