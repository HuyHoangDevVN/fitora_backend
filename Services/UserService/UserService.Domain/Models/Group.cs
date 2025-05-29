using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class Group : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GroupPrivacy Privacy { get; set; } = GroupPrivacy.Public;
    public bool RequirePostApproval { get; set; } = false;
    public string? CoverImageUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public GroupStatus Status { get; set; } = GroupStatus.Active;

}