using BuildingBlocks.Abstractions;

namespace NotificationService.Domain.Models;

public class NotificationType : Entity<int>
{
    public string Code { get; set; }
    public string Name { get; set; }
}