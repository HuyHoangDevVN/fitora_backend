using BuildingBlocks.Abstractions;
using BuildingBlocks.Abstractions.Entities;
using InteractService.Domain.Enums;

namespace InteractService.Domain.Models;

public class Post : Entity<Guid>, ISoftDelete
{
    public Guid UserId { get; set; }
    public Guid? GroupId { get; set; } = null;
    public string Content { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public PrivacyPost Privacy { get; set; } = PrivacyPost.Public;
    public bool IsDeleted { get; set; } = false;
    public int VotesCount { get; set; } = 0;
    public ICollection<UserVoted> UserVoteds { get; set; } = new List<UserVoted>();
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool? IsApproved { get; set; } = false;
    public int CommentsCount { get; set; } = 0;
    public double? Score { get; set; } = 0;
}