using BuildingBlocks.Abstractions;

namespace NotificationService.Domain.Models;

public class NotificationTemplate : Entity<int>
{
    public int NotificationTypeId { get; set; }
    public NotificationType NotificationType { get; set; } = null!;
    public string Template { get; set; } 
}