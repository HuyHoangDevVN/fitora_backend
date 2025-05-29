using InteractService.Application.DTOs.Category.Response;

namespace InteractService.Application.Usecases.Category.Queries.GetCategory;

public record GetCategoryQuery(Guid Id) : IQuery<CategoryResponseDto>;