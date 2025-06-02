using AuthService.Application.Auths.Commands.AuthLogin;
using AuthService.Application.DTOs.Auth.Requests;
using AuthService.Application.DTOs.Auth.Responses;
using Microsoft.AspNetCore.Http;

namespace AuthService.Application.Services.IServices;

public interface IAuthRepository
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<bool> ChangePasswordAsync(ChangePasswordRequestDto dto);
    Task<bool> LockUserAsync(LockUserRequestDto dto);
    Task<bool> DeleteUserAsync(DeleteUserRequestDto dto);
    void SetTokenInsideCookie(LoginTokenResponseDto result, HttpContext context);
    Task<bool> LockUserByAdminAsync(Guid userId);
    Task<bool> UnlockUserByAdminAsync(Guid userId);
}