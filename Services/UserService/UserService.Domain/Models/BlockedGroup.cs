using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class BlockedGroup : Entity<Guid>
{
    public Guid BlockerUserId { get; set; }
    public User? BlockerUser { get; set; }
    public Guid GroupId { get; set; }
    public Group? Group { get; set; }
}
