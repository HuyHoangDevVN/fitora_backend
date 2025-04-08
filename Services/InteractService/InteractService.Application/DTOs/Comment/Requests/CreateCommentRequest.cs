namespace InteractService.Application.DTOs.Comment.Requests;

public record CreateCommentRequest(
    Guid UserId,
    Guid PostId,
    Guid? ParentCommentId,
    string Content,
    string MediaUrl
);

public record CreateCommentFormBody(
    Guid PostId,
    Guid? ParentCommentId,
    string Content,
    string MediaUrl
);