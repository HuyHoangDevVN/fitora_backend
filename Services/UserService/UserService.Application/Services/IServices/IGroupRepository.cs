namespace UserService.Application.Services.IServices;

public interface IGroupRepository
{
    Task<bool> CreateAsync(Group group);
    Task<bool> UpdateAsync(Group group);
    Task<bool> DeleteAsync(Guid id);
}