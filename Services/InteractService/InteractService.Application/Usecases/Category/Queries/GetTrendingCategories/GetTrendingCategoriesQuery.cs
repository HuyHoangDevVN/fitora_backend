using InteractService.Application.DTOs.Category.Requests;
using InteractService.Application.DTOs.Category.Response;

namespace InteractService.Application.Usecases.Category.Queries.GetTrendingCategories;

public record GetTrendingCategoriesQuery(GetTrendingCategoriesRequest Request) : IQuery<IEnumerable<CategoryResponseDto>>;