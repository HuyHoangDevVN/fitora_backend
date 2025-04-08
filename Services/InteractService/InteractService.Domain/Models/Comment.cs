using BuildingBlocks.Abstractions;
using BuildingBlocks.Abstractions.Entities;

namespace InteractService.Domain.Models;

public class Comment : Entity<Guid>, ISoftDelete
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; } = null;
    public string Content { get; set; } = string.Empty;
    public string MediaUrl { get; set; }
    public int VotesCont { get; set; } = 0;
    public ICollection<CommentVotes> CommentVotes { get; set; } = new List<CommentVotes>();

    public int ReplyCount { get; set; } = 0;
    public bool IsDeleted { get; set; }
}