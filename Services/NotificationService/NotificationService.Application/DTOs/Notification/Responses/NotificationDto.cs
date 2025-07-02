using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs.Notification.Responses;

public class NotificationDto
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? SenderId { get; set; }
    public int NotificationTypeId { get; set; }
    public Domain.Models.NotificationType NotificationType { get; set; } = null!;
    public Guid? ObjectId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDelivered { get; set; }
    public NotificationChannel Channel { get; set; }
}