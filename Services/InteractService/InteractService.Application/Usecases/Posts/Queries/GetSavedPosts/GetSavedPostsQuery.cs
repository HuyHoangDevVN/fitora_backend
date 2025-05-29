using BuildingBlocks.Pagination.Cursor;

namespace InteractService.Application.Usecases.Posts.Queries.GetSavedPosts;

public record GetSavedPostsQuery(GetSavedPostsRequest Request) : IQuery<PaginatedCursorResult<PostResponseDto>>;