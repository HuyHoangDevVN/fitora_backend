namespace InteractService.Application.Services.IServices;

public interface IElasticsearchPostService
{
    Task IndexPostAsync(Post post);
    Task<Post?> GetPostByIdAsync(Guid id);
    Task UpdatePostAsync(Post post);
    Task DeletePostAsync(Guid id);
    Task<List<Post>> SearchByContentAsync(string keyword, int size = 10);
}