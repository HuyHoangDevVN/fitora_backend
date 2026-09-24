namespace InteractService.Domain.Models;

using BuildingBlocks.Abstractions;

public class ShortClip : Entity<Guid>
{
    public Guid AuthorId { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string Caption { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string Visibility { get; set; } = "Public";
    public string Status { get; set; } = "Published";
}
