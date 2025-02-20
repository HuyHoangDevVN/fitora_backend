using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Cursor;

namespace InteractService.Application.Usecases.Posts.Queries.GetPersonal;

public record GetPersonalQuery(GetPostRequest Request) : IQuery<PaginatedCursorResult<PostResponseDto>>;
