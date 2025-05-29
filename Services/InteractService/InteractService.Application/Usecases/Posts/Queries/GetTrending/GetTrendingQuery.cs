using BuildingBlocks.Pagination.Cursor;

namespace InteractService.Application.Usecases.Posts.Queries.GetTrending;

public record GetTrendingQuery(GetTrendingPostRequest Request): IQuery<PaginatedCursorResult<PostResponseDto>>;