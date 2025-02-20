namespace BuildingBlocks.Pagination;

public record PaginationCursorRequest(long? Cursor, int Limit = 10);