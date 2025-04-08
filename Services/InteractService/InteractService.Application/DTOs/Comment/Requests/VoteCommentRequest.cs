using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.Comment.Requests;

public record VoteCommentRequest(Guid UserId, Guid CommentId, VoteType VoteType);