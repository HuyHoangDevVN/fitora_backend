using UserService.Domain.Enums;

namespace UserService.Application.DTOs.GroupMember.Requests;

public record AssignRoleGroupMemberRequest(Guid AssignedBy, Guid GroupId, Guid MemberId, GroupRole Role);
public record AssignRoleGroupMemberFromBody(Guid AssignedBy, Guid GroupId, Guid MemberId, GroupRole Role);