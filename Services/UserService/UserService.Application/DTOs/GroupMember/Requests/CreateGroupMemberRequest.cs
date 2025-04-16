using UserService.Domain.Enums;

namespace UserService.Application.DTOs.GroupMember.Requests;

public record CreateGroupMemberRequest(
    Guid GroupId,
    Guid UserId,
    GroupRole? Role
);