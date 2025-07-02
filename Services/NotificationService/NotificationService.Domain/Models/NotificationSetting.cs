using BuildingBlocks.Abstractions;

namespace NotificationService.Domain.Models;

public class NotificationSetting : Entity<long>
{
    public Guid UserId { get; set; }
    public int NotificationTypeId { get; set; }
    public NotificationType NotificationType { get; set; } = null!;
    public bool IsEnabled { get; set; }
}