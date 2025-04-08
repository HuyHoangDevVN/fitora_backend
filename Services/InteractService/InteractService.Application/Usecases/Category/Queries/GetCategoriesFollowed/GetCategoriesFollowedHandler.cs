using BuildingBlocks.Pagination.Base;
using InteractService.Application.DTOs.Category.Response;

namespace InteractService.Application.Usecases.Category.Queries.GetCategoriesFollowed;

public class GetCategoriesFollowedHandler (ICategoryRepository categoryRepo, IMapper mapper) : IQueryHandler<GetCategoriesFollowedQuery, PaginatedResult<CategoryResponseDto>>
{
    public async Task<PaginatedResult<CategoryResponseDto>> Handle(GetCategoriesFollowedQuery query, CancellationToken cancellationToken)
    {
        var result = await categoryRepo.GetCategoriesFollowed(query.Request);
        return result;
    }
}