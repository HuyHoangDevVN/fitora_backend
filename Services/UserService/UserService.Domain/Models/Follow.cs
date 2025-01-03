using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class Follow : Entity<Guid>
{
    public Guid FollowerId { get; set; } = default!;
    public Guid FollowedId { get; set; } = default!;
    public StatusFollow Status { get; set; } = StatusFollow.Pending;
    public User? Follower { get; set; }
    public User? Followed { get; set; }
}