using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class FriendShip : Entity<int>
{
    public int UserId1 { get; set; } = default!;
    public int UserId2 { get; set; } = default!;
    
}