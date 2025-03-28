using BuildingBlocks.Pagination.Cursor;

namespace InteractService.Application.Services.IServices;

public interface IPostRepository
{
    Task<bool> CreateAsync(Post post);
    Task<Post> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Post post);
    Task<bool> VoteAsync(VotePostRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<PaginatedCursorResult<PostResponseDto>> GetNewfeed(GetPostRequest request);
    Task<PaginatedCursorResult<PostResponseDto>> GetPersonal(GetPostRequest request);
    
}