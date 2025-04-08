namespace BuildingBlocks.Pagination.Base;

public record PaginationRequest(int PageIndex = 0, int PageSize = 10);