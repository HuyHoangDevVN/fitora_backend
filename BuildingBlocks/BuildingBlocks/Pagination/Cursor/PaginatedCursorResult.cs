namespace BuildingBlocks.Pagination.Cursor;

public class PaginatedCursorResult<TEntity>
    (string? cursor, int limit, long count, IEnumerable<TEntity> data, string? nextCursor)
{
    public string? Cursor { get; } = cursor;
    public int Limit { get; } = limit;
    public long Count { get; } = count;
    public IEnumerable<TEntity> Data { get; } = data;
    public string? NextCursor { get; } = nextCursor; // Cursor để load trang tiếp theo
}