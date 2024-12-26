using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class FriendRequest : Entity<int>
{
    public int SenderUserId { get; set; } = default!;
    public int ReceiverUserId { get; set; } = default!;
    public StatusFriendRequest Status { get; set; } = StatusFriendRequest.Pending;
    
    public User? Sender { get; set; }
    public User? Receiver { get; set; }
}