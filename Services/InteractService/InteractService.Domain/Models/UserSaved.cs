using BuildingBlocks.Abstractions;

namespace InteractService.Domain.Models;

public class UserSaved: Entity<Guid>
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public DateTime Timestamp { get; set; }
    public Post Post { get; set; }
}