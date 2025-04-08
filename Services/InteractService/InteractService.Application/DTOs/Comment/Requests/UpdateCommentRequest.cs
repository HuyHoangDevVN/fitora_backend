namespace InteractService.Application.DTOs.Comment.Requests;

public record UpdateCommentRequest(
    Guid Id,
    string Content,
    string MediaUrl);