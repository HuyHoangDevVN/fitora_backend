using BuildingBlocks.Abstractions;
using BuildingBlocks.Abstractions.Entities;

namespace InteractService.Domain.Models;

public class Comment : Entity<int>, ISoftDelete
{
    public int UserId { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public int ParentCommentId { get; set; }
    public Comment ParentComment { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public int Votes { get; set; } = 0;
    public int ReplyCount { get; set; } = 0;
    public bool IsDeleted { get; set; }
}