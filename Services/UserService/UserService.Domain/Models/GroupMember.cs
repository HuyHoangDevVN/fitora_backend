using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class GroupMember : Entity<int>
{
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public GroupRole Role { get; set; } = GroupRole.Member;
}