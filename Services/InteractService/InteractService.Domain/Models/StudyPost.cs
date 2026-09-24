namespace InteractService.Domain.Models;

using BuildingBlocks.Abstractions;

public class StudyPost : Entity<Guid>
{
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string AttachmentUrlsJson { get; set; } = "[]";
    public string Status { get; set; } = "Published";
}
