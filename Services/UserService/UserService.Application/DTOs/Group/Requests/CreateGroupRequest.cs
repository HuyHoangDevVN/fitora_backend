using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Group.Requests;

public record CreateGroupRequest(
    Guid UserId,
    string Name,
    string Description,
    GroupPrivacy Privacy,
    bool RequirePostApproval,
    string? CoverImageUrl,
    string? AvatarUrl
);

public record CreateGroupFromBody(
    string Name,
    string Description,
    GroupPrivacy Privacy,
    bool RequirePostApproval,
    string? CoverImageUrl,
    string? AvatarUrl
);