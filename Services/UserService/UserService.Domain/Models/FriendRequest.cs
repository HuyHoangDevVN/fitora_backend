using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class FriendRequest : Entity<int>
{
    public int SenderId { get; set; } = default!;
    public int ReceiverId { get; set; } = default!;
    public StatusFriendRequest Status { get; set; } = StatusFriendRequest.Pending;
    
    public User? Sender { get; set; }
    public User? Receiver { get; set; }
}