using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class EventRsvp : Entity<Guid>
{
    public Guid EventId { get; set; }
    public GroupEvent Event { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public RsvpStatus Status { get; set; } = RsvpStatus.Maybe;
}