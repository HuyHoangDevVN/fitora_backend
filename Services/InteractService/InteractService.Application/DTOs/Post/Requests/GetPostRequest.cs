namespace InteractService.Application.DTOs.Post.Requests;

public record GetPostRequest(Guid Id, long? Cursor = null, int Limit = 10)
{
    public long? Cursor { get; init; } = Cursor;
    public int Limit { get; init; } = Limit > 0 ? Limit : 10; 
}