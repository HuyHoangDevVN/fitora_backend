using BuildingBlocks.Pagination.Base;

namespace InteractService.Application.Usecases.Posts.Queries.GetListPost;

public record GetListPostQuery(GetListPostRequest Request) : IQuery<PaginatedResult<PostResponseDto>>;