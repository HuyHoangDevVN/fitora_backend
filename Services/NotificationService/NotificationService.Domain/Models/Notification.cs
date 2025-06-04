using BuildingBlocks.Abstractions;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Models;

public class Notification : Entity<long>
{
    public Guid UserId { get; set; }
    public Guid? SenderId { get; set; }
    public int NotificationTypeId { get; set; }
    public NotificationType NotificationType { get; set; } = null!;
    public Guid? ObjectId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDelivered { get; set; }
    public NotificationChannel Channel { get; set; }
}