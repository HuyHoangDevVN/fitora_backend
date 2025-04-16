namespace UserService.Application.DTOs.GroupInvite.Requests;

public record CreateGroupInvitesRequest(
    Guid GroupId,
    Guid SenderUserId,
    List<Guid> ReceiverUserId);