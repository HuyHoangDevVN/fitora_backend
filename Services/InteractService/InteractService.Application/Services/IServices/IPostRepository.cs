using BuildingBlocks.Pagination.Cursor;
using InteractService.Application.DTOs.Category.Response;

namespace InteractService.Application.Services.IServices;

public interface IPostRepository
{
    Task<bool> CreateAsync(Post post);
    Task<Post> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Post post);
    Task<bool> VoteAsync(VotePostRequest request);
    Task<bool> SavePostAsync(SavePostRequest request);
    Task<bool> UnSavePostAsync(SavePostRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<PaginatedCursorResult<PostResponseDto>> GetSavedPosts(GetSavedPostsRequest request);
    Task<PaginatedCursorResult<PostResponseDto>> GetNewfeed(GetPostRequest request);
    Task<PaginatedCursorResult<PostResponseDto>> GetPersonal(GetPostRequest request);

    Task<PaginatedCursorResult<PostResponseDto>> GetTrendingFeed(GetTrendingPostRequest request,
        IEnumerable<CategoryResponseDto> trendingCategories);

    Task<PaginatedCursorResult<PostResponseDto>> GetExploreFeed(GetExplorePostRequest request);

}