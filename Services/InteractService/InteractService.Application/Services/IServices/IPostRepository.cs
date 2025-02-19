namespace InteractService.Application.Services.IServices;

public interface IPostRepository
{
    Task<bool> CreateAsync(Post post);
    Task<IEnumerable<Post>> GetAllAsync();
    Task<IEnumerable<Post>> GetListAsync();
    Task<Post> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Post post);
    Task<bool> DeleteAsync(Guid id);
}