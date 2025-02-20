using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Pagination.Cursor;

public class PagedCursorResult<T>
{
    public const int UpperLimit = 100;
    public const int DefaultLimit = 15;

    private PagedCursorResult(List<T> items, int limit, long? nextCursor, long count)
    {
        Items = items;
        Limit = limit;
        NextCursor = nextCursor;
        Count = count; 
    }

    public List<T> Items { get; }
    public int Limit { get; }
    public long? NextCursor { get; }
    public long Count { get; } 

    public static async Task<PagedCursorResult<T>> CreateAsync(
        IQueryable<T> query, long? cursor, int limit, Func<T, long> cursorSelector)
    {
        limit = limit <= 0 ? DefaultLimit : Math.Min(limit, UpperLimit);

        if (cursor.HasValue)
        {
            query = query.Where(item => cursorSelector(item) < cursor.Value);
        }

        // Lấy tổng số bản ghi trong query
        long totalCount = await query.CountAsync();

        // Lấy danh sách dữ liệu, dùng await để tránh chạy đồng bộ
        var items = await query.OrderByDescending(cursorSelector)
            .Take(limit + 1)
            .AsQueryable() 
            .ToListAsync(); 


        // Nếu lấy đủ limit + 1 phần tử, gán giá trị nextCursor là phần tử cuối cùng
        long? nextCursor = items.Count > limit ? cursorSelector(items[^1]) : null;

        return new(items.Take(limit).ToList(), limit, nextCursor, totalCount);
    }

    public static PagedCursorResult<T> Create(List<T> items, int limit, long? nextCursor, long count)
        => new(items, limit, nextCursor, count);
}