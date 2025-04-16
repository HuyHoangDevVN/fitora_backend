using BuildingBlocks.Abstractions;

namespace UserService.Domain.Models;

public class GroupEvent : Entity<Guid>
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }
    public string? Location { get; set; }
}