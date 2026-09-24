using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Group.Requests;

public record UpdateGroupRequest(
    Guid Id,
    string Name,
    string Description,
    GroupPrivacy Privacy,
    bool RequirePostApproval,
    bool RequireJoinApproval,
    string? CoverImageUrl,
    string? AvatarUrl
);