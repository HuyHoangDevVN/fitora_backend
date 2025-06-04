namespace NotificationService.Application.DTOs.NotificationSetting.Requests;

public record UpdateNotificationSettingRequest(Guid UserId, int NotificationTypeId, bool IsEnabled);