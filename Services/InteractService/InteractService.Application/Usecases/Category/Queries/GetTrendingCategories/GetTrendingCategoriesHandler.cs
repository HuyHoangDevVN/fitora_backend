using InteractService.Application.DTOs.Category.Response;

namespace InteractService.Application.Usecases.Category.Queries.GetTrendingCategories;

public class GetTrendingCategoriesHandler(ICategoryRepository categoryRepo, IMapper mapper) : IQueryHandler<GetTrendingCategoriesQuery, IEnumerable<CategoryResponseDto>>
{
    public async Task<IEnumerable<CategoryResponseDto>> Handle(GetTrendingCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await categoryRepo.GetTrendingCategories(query.Request.Limit, query.Request.TimeRange);
        return mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
    }
}