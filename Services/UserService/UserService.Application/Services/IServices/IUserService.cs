namespace UserService.Application.Services.IServices;

public interface IUserService
{
    Task<bool> CreateUserAsync(CreateUserRequest dto);
}