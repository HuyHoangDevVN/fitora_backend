using AuthService.Application.Auths.Commands.AuthChangePassword;
using AuthService.Application.Auths.Commands.AuthDeleteAccount;
using AuthService.Application.Auths.Commands.AuthLockAccount;
using AuthService.Application.Auths.Commands.AuthLogin;
using AuthService.Application.Auths.Commands.AuthRegister;
using AuthService.Application.Auths.Commands.RefreshToken;
using AuthService.Application.DTOs.Auth.Requests;
using AuthService.Application.DTOs.Key.Requests;
using AuthService.Application.Services.IServices;
using AutoMapper;
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

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto req)
    {
        var requestModel = _mapper.Map<AuthRegisterCommand>(req);
        var result = await _sender.Send(requestModel);
        var registerResult = _mapper.Map<AuthRegisterResult>(result);
        var response = new ResponseDto(registerResult.LoginResponseDto, Message: "Register Successful");
        return Ok(response);
    }

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
            var userId = _authorizeExtension.DecodeToken().Id;
            var command = _mapper.Map<RefreshTokenCommand>(new RefreshTokenByUserRequestDto(cookieToken, userId));
            var result = await _sender.Send(command);
            var response = new ResponseDto(result, Message: "Refresh Token Successful");
            return Ok(response);
        }

        return Ok(new ResponseDto(Message: "Refresh Token Failed", IsSuccess: false));
    }

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

        if (string.IsNullOrEmpty(cookieToken))
        {
            return Ok(new ResponseDto(null, IsSuccess: false, "User is not logged in"));
        }

        return Ok(new ResponseDto(cookieToken, IsSuccess: true, "User is logged in"));
    }
    
    [HttpGet("check-cookie")]
    public IActionResult CheckCookie()
    {
        var token = Request.Cookies["accessToken"];
        return Ok(new { accessToken = token });
    }
}