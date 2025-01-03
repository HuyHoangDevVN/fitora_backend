using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class Block : Entity<Guid>
{
    public Guid BlockerUserId { get; set; } = default!;
    public User? BlockerUser { get; set; }
    public Guid BlockedUserId { get; set; } = default!;
    public User? BlockedUser { get; set; }
}