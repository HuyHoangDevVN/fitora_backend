namespace BuildingBlocks.Pagination.Cursor;

public class PaginatedCursorResult<TEntity>
    (long? cursor, int limit, long count, IEnumerable<TEntity> data, long? nextCursor)
{
    public long? Cursor { get; } = cursor;
    public int Limit { get; } = limit;
    public long Count { get; } = count;
    public IEnumerable<TEntity> Data { get; } = data;
    public long? NextCursor { get; } = nextCursor; // Cursor để load trang tiếp theo
}