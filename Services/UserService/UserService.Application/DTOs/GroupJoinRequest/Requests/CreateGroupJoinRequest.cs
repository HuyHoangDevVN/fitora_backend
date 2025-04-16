namespace UserService.Application.DTOs.GroupJoinRequest.Requests;

public record CreateGroupJoinRequest(
    Guid GroupId,
    Guid UserId
);