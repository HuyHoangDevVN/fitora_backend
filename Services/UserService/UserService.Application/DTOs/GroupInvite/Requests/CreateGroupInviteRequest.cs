namespace UserService.Application.DTOs.GroupInvite.Requests;

public record CreateGroupInviteRequest(
    Guid GroupId,
    Guid SenderUserId,
    Guid ReceiverUserId);

public record CreateGroupInviteFormBody(
    Guid GroupId,
    Guid ReceiverUserId);