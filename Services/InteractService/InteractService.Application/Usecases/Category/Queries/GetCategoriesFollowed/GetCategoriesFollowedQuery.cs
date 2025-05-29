using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using InteractService.Application.DTOs.Category.Requests;
using InteractService.Application.DTOs.Category.Response;

namespace InteractService.Application.Usecases.Category.Queries.GetCategoriesFollowed;

public record GetCategoriesFollowedQuery(GetCategoriesFollowedRequest Request) : IQuery<PaginatedResult<CategoryResponseDto>>;