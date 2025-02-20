namespace BuildingBlocks.Pagination.Cursor;

public record PaginationCursorRequest(string? Cursor, int Limit = 10);