using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class GroupInvite : Entity<int>
{
    public int GroupId { get; set; }
    public int SenderUserID { get; set; } = default!;
    public int ReceiverUserID { get; set; } = default!;
    public StatusGroupInvite Status { get; set; } = StatusGroupInvite.Pending;
    public Group Group { get; set; } = default!;
    public User? SenderUser { get; set; }
    public User? ReceiverUser { get; set; }
}