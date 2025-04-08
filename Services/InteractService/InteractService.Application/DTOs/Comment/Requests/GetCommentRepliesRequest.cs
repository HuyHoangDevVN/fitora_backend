namespace InteractService.Application.DTOs.Comment.Requests;

public record GetCommentRepliesRequest(
    Guid UserId,
    Guid ParentCommentId,
    string? Cursor = null,
    int Limit = 10)
{
    public string? Cursor { get; init; } = Cursor;
    public int Limit { get; init; } = Limit > 0 ? Limit : 10;
}