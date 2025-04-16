using BuildingBlocks.Abstractions;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class UserVoted : Entity<Guid>
{
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public VoteType VoteType { get; set; } = VoteType.UnVote;
    public DateTime Timestamp { get; set; }
    public Post Post { get; set; }
}