using BuildingBlocks.Abstractions;

namespace InteractService.Domain.Models;

public class FollowCategory : Entity<Guid>
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
}