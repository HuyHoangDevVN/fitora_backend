using InteractService.Application.DTOs.Category.Response;

namespace InteractService.Application.Usecases.Category.Queries.GetCategory;

public class GetCategoryHandler(ICategoryRepository categoryRepo, IMapper mapper) : IQueryHandler<GetCategoryQuery, CategoryResponseDto>
{
    public async Task<CategoryResponseDto> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        var category = await categoryRepo.GetCategory(query.Id);
        return mapper.Map<CategoryResponseDto>(category);
    }
}