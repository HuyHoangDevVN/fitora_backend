using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.Post.Requests;

public record VotePostRequest(Guid UserId, Guid PostId, VoteType VoteType);