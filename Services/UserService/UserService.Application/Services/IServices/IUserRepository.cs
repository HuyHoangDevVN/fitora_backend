namespace UserService.Application.Services.IServices;

public interface IUserRepository
{
    Task<bool> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(UpdateUserRequest request);
}