using BuildingBlocks.Pagination.Cursor;

namespace InteractService.Application.Usecases.Posts.Queries.GetExploreFeed;

public record GetExploreFeedQuery(GetExplorePostRequest Request): IQuery<PaginatedCursorResult<PostResponseDto>>;