using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.Comment.Responses;

public class CommentResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MediaUrl { get; set; }
    public int Votes { get; set; } = 0;
    public int ReplyCount { get; set; } = 0;
    public double? Score { get; set; }
    public UserWithInfoDto? User { get; set; }
    public VoteType? UserVoteType { get; set; }
    public bool IsDeleted { get; set; }
}