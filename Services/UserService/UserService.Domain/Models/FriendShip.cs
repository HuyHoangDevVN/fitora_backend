using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class FriendShip : Entity<int>
{
    public int User1Id { get; set; } = default!;
    public int User2Id { get; set; } = default!;
    public User User1 { get; set; } = default!;
    public User User2 { get; set; } = default!;
}