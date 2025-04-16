using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Group.Responses;

public class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GroupPrivacy Privacy { get; set; } = GroupPrivacy.Public;
    public bool RequirePostApproval { get; set; } = false;
    public string? CoverImageUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public GroupStatus Status { get; set; } = GroupStatus.Active;
}