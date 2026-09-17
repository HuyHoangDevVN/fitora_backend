using BuildingBlocks.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[Route("api/auth/2fa")]
[ApiController]
public class TwoFactorController : Controller
{
    private readonly ISender _sender;
    public TwoFactorController(ISender sender) => _sender = sender;

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var r = await _sender.Send(new AuthService.Application.Auths.TwoFactor.GetTwoFactorStatusQuery());
        return Ok(new ResponseDto(r));
    }
    [HttpPost("setup")]
    public async Task<IActionResult> Setup()
    {
        var r = await _sender.Send(new AuthService.Application.Auths.TwoFactor.SetupTwoFactorCommand());
        return Ok(new ResponseDto(r));
    }
    [HttpPost("verify-setup")]
    public async Task<IActionResult> VerifySetup([FromBody] VerifySetupDto dto)
    {
        var r = await _sender.Send(new AuthService.Application.Auths.TwoFactor.VerifySetupCommand(dto.Code));
        return Ok(new ResponseDto(r, IsSuccess: r.IsSuccess, Message: r.Message));
    }
    [HttpPost("enable")]
    public async Task<IActionResult> Enable()
    {
        var r = await _sender.Send(new AuthService.Application.Auths.TwoFactor.EnableTwoFactorCommand());
        return Ok(new ResponseDto(r, IsSuccess: r.IsSuccess, Message: r.Message));
    }
    [HttpPost("disable")]
    public async Task<IActionResult> Disable()
    {
        var r = await _sender.Send(new AuthService.Application.Auths.TwoFactor.DisableTwoFactorCommand());
        return Ok(new ResponseDto(r, IsSuccess: r.IsSuccess, Message: r.Message));
    }
    [HttpPost("verify-login")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> VerifyLogin([FromBody] VerifyLoginDto dto)
    {
        var r = await _sender.Send(new AuthService.Application.Auths.TwoFactor.VerifyLoginCommand(dto.UserId, dto.Code));
        return Ok(new ResponseDto(r, IsSuccess: r.IsSuccess, Message: r.Message));
    }
    [HttpPost("regenerate-recovery-codes")]
    public async Task<IActionResult> Regenerate()
    {
        var r = await _sender.Send(new AuthService.Application.Auths.TwoFactor.RegenerateRecoveryCodesCommand());
        return Ok(new ResponseDto(r));
    }
}
public record VerifySetupDto(string Code);
public record VerifyLoginDto(string UserId, string Code);
