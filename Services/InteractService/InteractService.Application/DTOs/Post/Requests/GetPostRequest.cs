namespace InteractService.Application.DTOs.Post.Requests;

public record GetPostRequest(
    Guid Id,
    FeedType FeedType,
    Guid? CategoryId = null,
    string? Cursor = null,
    int Limit = 10,
    Guid? GroupId = null)
{
    public string? Cursor { get; init; } = Cursor;
    public int Limit { get; init; } = Limit > 0 ? Limit : 10;

    public Guid? CategoryId { get; init; } = FeedType == FeedType.Category && !CategoryId.HasValue
        ? throw new ArgumentException("Vui lòng truyền vào CategoryID.")
        : CategoryId;
}

public enum FeedType
{
    All = 1,
    Category = 2
}