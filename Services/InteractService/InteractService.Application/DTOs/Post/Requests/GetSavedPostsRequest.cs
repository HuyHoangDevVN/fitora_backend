namespace InteractService.Application.DTOs.Post.Requests;

public record GetSavedPostsRequest
(
    Guid Id,
    string? Cursor = null,
    int Limit = 10,
    Guid? GroupId = null)
{
    public string? Cursor { get; init; } = Cursor;
    public int Limit { get; init; } = Limit > 0 ? Limit : 10;
}