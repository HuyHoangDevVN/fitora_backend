using BuildingBlocks.Pagination.Cursor;

namespace InteractService.Application.Usecases.Posts.Queries.GetNewfeed;

public record GetNewfeedQuery(GetPostRequest Request) : IQuery<PaginatedCursorResult<PostResponseDto>>;