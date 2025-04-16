using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class GroupInvite : Entity<Guid>
{
    public Guid GroupId { get; set; }
    public Guid SenderUserId { get; set; } = default!;
    public Guid ReceiverUserId { get; set; } = default!;
    public StatusGroupInvite Status { get; set; } = StatusGroupInvite.Pending;
    public Group Group { get; set; } = default!;
    public User? SenderUser { get; set; }
    public User? ReceiverUser { get; set; }
}