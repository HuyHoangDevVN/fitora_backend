using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class RecentSearch : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string Query { get; set; } = default!;
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}
