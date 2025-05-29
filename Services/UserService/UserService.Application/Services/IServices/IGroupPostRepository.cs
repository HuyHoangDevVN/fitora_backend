namespace UserService.Application.Services.IServices;

public interface IGroupPostRepository
{
    Task<bool> CreateAsync(GroupPost groupPost);
    Task<bool> UpdateAsync(GroupPost groupPost);
    Task<bool> DeleteAsync(Guid id);
}