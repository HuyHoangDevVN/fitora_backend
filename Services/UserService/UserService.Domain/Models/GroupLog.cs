using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class GroupLog : Entity<Guid>
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public Guid PerformedBy { get; set; }
    public User PerformedByUser { get; set; } = null!;

    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}