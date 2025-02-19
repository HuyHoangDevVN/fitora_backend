using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class FriendShip : Entity<Guid>
{
    public Guid User1Id { get; set; } = default!;
    public Guid? User2Id { get; set; } = default!;
    public User User1 { get; set; } = default!;
    public User User2 { get; set; } = default!;
}