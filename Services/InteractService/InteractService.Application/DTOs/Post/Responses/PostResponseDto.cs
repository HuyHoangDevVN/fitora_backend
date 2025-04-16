using BuildingBlocks.Abstractions;
using BuildingBlocks.Abstractions.Entities;
using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.Post.Responses;

public class PostResponseDto : EntityAuditBase<Guid>
{
    public Guid Id { get; set; }
    public Guid? GroupId { get; set; } = null;
    public Guid? CategoryId { get; set; } = null;
    public string? CategoryName { get; set; } = null;
    public string Content { get; set; }
    public string MediaUrl { get; set; }
    public int VotesCount { get; set; } = 0;
    public int CommentsCount { get; set; } = 0;
    public double? Score { get; set; }
    public bool? IsCategoryFollowed { get; set; } = false;
    public PrivacyPost Privacy { get; set; } = PrivacyPost.Public;
    public UserWithInfoDto? User { get; set; }
    public VoteType? UserVoteType { get; set; }
}