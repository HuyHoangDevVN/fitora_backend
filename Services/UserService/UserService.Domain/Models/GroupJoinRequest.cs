using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class GroupJoinRequest : Entity<Guid>
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public GroupJoinRequestStatus Status { get; set; } = GroupJoinRequestStatus.Pending;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComment { get; set; }

    public Guid? ReviewedBy { get; set; }
    public User? ReviewedByUser { get; set; }
}