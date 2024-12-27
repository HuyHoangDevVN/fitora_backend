using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class Block : Entity<int>
{
    public int BlockerUserId { get; set; } = default!;
    public User BlockerUser { get; set; } = default!;
    public int BlockedUserId { get; set; } = default!;
    public User BlockedUser { get; set; } = default!;
}