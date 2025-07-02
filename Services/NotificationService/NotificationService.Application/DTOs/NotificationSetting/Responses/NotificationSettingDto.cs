namespace NotificationService.Application.DTOs.NotificationSetting.Responses;

public class NotificationSettingDto
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public int NotificationTypeId { get; set; }
    public string TypeCode { get; set; }
    public bool IsEnabled { get; set; }
}