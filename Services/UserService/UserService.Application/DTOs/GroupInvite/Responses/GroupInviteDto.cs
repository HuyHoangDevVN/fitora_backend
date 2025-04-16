using UserService.Domain.Enums;

namespace UserService.Application.DTOs.GroupInvite.Responses;

public class GroupInviteDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = null!;
    public string? GroupImageUrl { get; set; }
    public Guid SenderUserId { get; set; } = Guid.Empty!;
    public string SenderName { get; set; } = null!;
    public string? SenderImageUrl { get; set; }
    public Guid ReceiverUserId { get; set; } = Guid.Empty!;
    public string ReceiverName { get; set; } = null!;
    public string? ReceiverImageUrl { get; set; }
    public StatusGroupInvite Status { get; set; } = StatusGroupInvite.Pending;
}