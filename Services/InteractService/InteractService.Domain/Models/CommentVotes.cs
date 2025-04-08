using BuildingBlocks.Abstractions;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class CommentVotes : Entity<Guid>
{
    public Guid UserId { get; set; }
    public Guid CommentId { get; set; }
    public VoteType VoteType { get; set; } = VoteType.UpVote;
    public DateTime Timestamp { get; set; }
    public Comment Comment { get; set; }
}