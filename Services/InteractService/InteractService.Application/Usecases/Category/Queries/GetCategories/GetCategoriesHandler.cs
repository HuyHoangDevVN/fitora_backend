using BuildingBlocks.Pagination.Base;

namespace InteractService.Application.Usecases.Category.Queries.GetCategories;

public class GetCategoriesHandler (ICategoryRepository categoryRepo, IMapper mapper) : IQueryHandler<GetCategoriesQuery, PaginatedResult<Domain.Models.Category>>
{
    public async Task<PaginatedResult<Domain.Models.Category>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var result = await categoryRepo.GetCategories(query.Request);
        return result;
    }
}