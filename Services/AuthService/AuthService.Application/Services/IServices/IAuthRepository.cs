using AuthService.Application.Auths.Commands.AuthLogin;
using AuthService.Application.DTOs.Auth.Requests;
using AuthService.Application.DTOs.Auth.Responses;
using Microsoft.AspNetCore.Http;

namespace AuthService.Application.Services.IServices;

public interface IAuthRepository
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);

    /// <summary>Phát token cho user đã xác thực đầy đủ — dùng lại ở luồng verify-login OTP (13.9).</summary>
    Task<LoginResponseDto> IssueLoginTokensAsync(ApplicationUser user);
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<bool> ChangePasswordAsync(ChangePasswordRequestDto dto);
    Task<bool> LockUserAsync(LockUserRequestDto dto);
    Task<bool> DeleteUserAsync(DeleteUserRequestDto dto);
    void SetTokenInsideCookie(LoginTokenResponseDto result, HttpContext context);
    Task<bool> LockUserByAdminAsync(Guid userId);
    Task<bool> UnlockUserByAdminAsync(Guid userId);
    Task<bool> DeleteUserByAdminAsync(Guid userId);
}