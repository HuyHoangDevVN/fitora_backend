using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class GroupNotification : Entity<Guid>
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public Guid ReceiverUserId { get; set; }
    public User ReceiverUser { get; set; } = null!;

    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
}