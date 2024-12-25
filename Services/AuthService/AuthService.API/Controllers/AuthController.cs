using AuthService.API.Endpoints.Auths;
using AuthService.Application.Auths.Commands.AuthChangePassword;
using AuthService.Application.Auths.Commands.AuthDeleteAccount;
using AuthService.Application.Auths.Commands.AuthLockAccount;
using AuthService.Application.Auths.Commands.AuthLogin;
using AuthService.Application.Auths.Commands.AuthRegister;
using AuthService.Application.DTOs.Auth.Requests;
using AutoMapper;
using BuildingBlocks.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : Controller
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public AuthController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
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
        var loginResult = _mapper.Map<AuthLoginResult>(result);
        var response = new ResponseDto(loginResult, Message: "Login Successful");
        return Ok(response);
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

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount(DeleteAccountRequest req)
    {
        var requestModel = _mapper.Map<AuthDeleteAccountCommand>(req);
        var result = await _sender.Send(requestModel);
        var deleteAccountResult = _mapper.Map<AuthDeleteAccountResult>(result);
        var response = new ResponseDto(deleteAccountResult, Message: "Delete Account Successful");
        return Ok(response);
    }
}