using BuildingBlocks.Pagination.Cursor;

namespace InteractService.Application.Usecases.Posts.Queries.GetTrending;

public class GetTrendingHandler(IPostRepository postRepo, ICategoryRepository categoryRepo, IMapper mapper)
    : IQueryHandler<GetTrendingQuery, PaginatedCursorResult<PostResponseDto>>
{
    public async Task<PaginatedCursorResult<PostResponseDto>> Handle(GetTrendingQuery query,
        CancellationToken cancellationToken)
    {
        var trendingCategories = await categoryRepo.GetTrendingCategories(10, TimeSpan.FromDays(7));
        var trendingPosts = await postRepo.GetTrendingFeed(query.Request, trendingCategories);
        return trendingPosts;
    }
}