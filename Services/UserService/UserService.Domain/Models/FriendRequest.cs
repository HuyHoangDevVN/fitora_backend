using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class FriendRequest : Entity<Guid>
{
    public Guid SenderId { get; set; } = default!;
    public Guid? ReceiverId { get; set; } = default!;
    public StatusFriendRequest Status { get; set; } = StatusFriendRequest.Pending;
    
    public User? Sender { get; set; }
    public User? Receiver { get; set; }
}