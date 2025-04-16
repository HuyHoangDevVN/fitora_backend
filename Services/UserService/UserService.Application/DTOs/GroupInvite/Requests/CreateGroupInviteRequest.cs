namespace UserService.Application.DTOs.GroupInvite.Requests;

public record CreateGroupInviteRequest(
    Guid GroupId,
    Guid SenderUserId,
    Guid ReceiverUserId);