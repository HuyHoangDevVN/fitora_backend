using AuthService.Application.DTOs.Auth.Requests;
using AuthService.Application.DTOs.Auth.Responses;

namespace AuthService.Application.Services.IServices;

public interface IAuthRepository
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<bool> ChangePasswordAsync(ChangePasswordRequestDto dto);
    Task<bool> LockUserAsync(LockUserRequestDto dto);
    Task<bool> DeleteUserAsync(DeleteUserRequestDto dto);
}