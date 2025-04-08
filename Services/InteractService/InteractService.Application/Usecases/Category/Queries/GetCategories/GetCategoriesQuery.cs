using BuildingBlocks.Pagination.Base;
using InteractService.Application.DTOs.Category.Requests;

namespace InteractService.Application.Usecases.Category.Queries.GetCategories;

public record GetCategoriesQuery(GetCategoriesRequest Request) : IQuery<PaginatedResult<Domain.Models.Category>>;