using BuildingBlocks.Abstractions;
using BuildingBlocks.Abstractions.Entities;
using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.Post.Responses;

public class PostResponseDto : EntityAuditBase<Guid>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? GroupId { get; set; } = null;
    public string Content { get; set; }
    public string MediaUrl { get; set; }
    public int VotesCount { get; set; } = 0;
    public int CommentsCount { get; set; } = 0;
    public double? Score { get; set; }
    public PrivacyPost Privacy { get; set; } = PrivacyPost.Public;
    public UserWithInfoDto? User { get; set; }
}