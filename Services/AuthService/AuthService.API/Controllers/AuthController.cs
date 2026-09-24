using AuthService.Application.Auths.Commands.AuthChangePassword;
using AuthService.Application.Auths.Commands.AuthDeleteAccount;
using AuthService.Application.Auths.Commands.AuthLockAccount;
using AuthService.Application.Auths.Commands.AuthLogin;
using AuthService.Application.Auths.Commands.AuthRegister;
using AuthService.Application.Auths.Commands.ForgotPassword;
using AuthService.Application.Auths.Commands.RefreshToken;
using AuthService.Application.Auths.Commands.ResetPassword;
using AuthService.Application.Auths.Commands.VerifyResetOtp;
using AuthService.Application.DTOs.Auth.Requests;
using AuthService.Application.DTOs.Key.Requests;
using AuthService.Application.Services.IServices;
using AutoMapper;
using BuildingBlocks.Attributes;
using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[Route("api/auth/auth")]
[ApiController]
public class AuthController : Controller
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;
    private readonly IAuthRepository _authoRepo;
    private readonly IAuthorizeExtension _authorizeExtension;

    public AuthController(ISender sender, IMapper mapper, IAuthRepository authorizationService,
        IAuthorizeExtension authorizeExtension)
    {
        _sender = sender;
        _mapper = mapper;
        _authoRepo = authorizationService;
        _authorizeExtension = authorizeExtension;
    }

    [RedisRateLimit(10, 60, "auth-register")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto req)
    {
        var requestModel = _mapper.Map<AuthRegisterCommand>(req);
        var result = await _sender.Send(requestModel);
        var registerResult = _mapper.Map<AuthRegisterResult>(result);
        var response = new ResponseDto(registerResult.LoginResponseDto, Message: "Register Successful");
        return Ok(response);
    }

    [RedisRateLimit(10, 60, "auth-login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto req)
    {
        var requestModel = _mapper.Map<AuthLoginCommand>(req);
        var result = await _sender.Send(requestModel);
        if (result.Token != null) _authoRepo.SetTokenInsideCookie(result.Token, HttpContext);
        return Ok(result);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
        return Ok(new ResponseDto(Message: "Đăng xuất thành công !"));
    }

    [RedisRateLimit(10, 60, "auth-change-password")]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto req)
    {
        var requestModel = _mapper.Map<AuthChangePasswordCommand>(req);
        var result = await _sender.Send(requestModel);
        var changePasswordResult = _mapper.Map<AuthChangePasswordResult>(result);
        var response = new ResponseDto(changePasswordResult, Message: "Change Password Successful");
        return Ok(response);
    }

    [HttpPost("lock-account")]
    public async Task<IActionResult> LockAccount(LockUserRequestDto req)
    {
        var requestModel = _mapper.Map<AuthLockAccountCommand>(req);
        var result = await _sender.Send(requestModel);
        var lockAccountResult = _mapper.Map<AuthLockAccountResult>(result);
        var response = new ResponseDto(lockAccountResult, Message: "Lock Account Successful");
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var cookieToken = Request.Cookies["refreshToken"];
        if (cookieToken != null)
        {
            var userId = _authorizeExtension.DecodeExpiredToken().Id;
            var command = _mapper.Map<RefreshTokenCommand>(new RefreshTokenByUserRequestDto(cookieToken, userId));
            var result = await _sender.Send(command);
            var response = new ResponseDto(result, Message: "Refresh Token Successful");
            return Ok(response);
        }

        return Ok(new ResponseDto(Message: "Refresh Token Failed", IsSuccess: false));
    }

    // 27.1/13.6: self-service delete — bắt buộc đăng nhập; userId luôn lấy từ token trong
    // AuthRepository.DeleteUserAsync (không tin req.UserId), và bắt buộc verify Password đúng.
    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount(DeleteUserRequestDto req)
    {
        var requestModel = _mapper.Map<AuthDeleteAccountCommand>(req);
        var result = await _sender.Send(requestModel);
        var deleteAccountResult = _mapper.Map<AuthDeleteAccountResult>(result);
        var response = new ResponseDto(deleteAccountResult, Message: "Delete Account Successful");
        return Ok(response);
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var cookieToken = Request.Cookies["accessToken"];
        var user = _authorizeExtension.DecodeToken();

        if (string.IsNullOrEmpty(cookieToken))
        {
            return Ok(new ResponseDto(user, IsSuccess: false, "User is not logged in"));
        }

        return Ok(new ResponseDto(cookieToken, IsSuccess: true, "User is logged in"));
    }
    
    [HttpGet("check-cookie")]
    public IActionResult CheckCookie()
    {
        var token = Request.Cookies["accessToken"];
        return Ok(new { accessToken = token });
    }

    [AllowAnonymous]
    [RedisRateLimit(5, 60, "auth-forgot-password")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto req)
    {
        var result = await _sender.Send(new ForgotPasswordCommand(req.Email));
        return Ok(result);
    }

    [AllowAnonymous]
    [RedisRateLimit(10, 60, "auth-verify-reset-otp")]
    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyResetOtp(VerifyResetOtpRequestDto req)
    {
        var result = await _sender.Send(new VerifyResetOtpCommand(req.Email, req.Otp));
        return Ok(result);
    }

    [AllowAnonymous]
    [RedisRateLimit(5, 60, "auth-reset-password")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto req)
    {
        var result = await _sender.Send(new ResetPasswordCommand(req.Email, req.Otp, req.NewPassword));
        return Ok(result);
    }
}