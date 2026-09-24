using AuthService.Application.Services.IServices;
using BuildingBlocks.Attributes;
using BuildingBlocks.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace AuthService.API.Controllers;

// Mục 12 audit: gate bằng feature flag "Auth2FA" (appsettings.json > FeatureManagement) —
// cho phép rollout dần 2FA theo môi trường mà không cần deploy lại code.
[FeatureGate("Auth2FA")]
[Route("api/auth/2fa")]
[ApiController]
public class TwoFactorController : Controller
{
    private readonly ISender _sender;
    private readonly IAuthRepository _authRepo;
    public TwoFactorController(ISender sender, IAuthRepository authRepo)
    {
        _sender = sender;
        _authRepo = authRepo;
    }

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
    [RedisRateLimit(5, 60, "2fa_verify_setup")]
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
    [RedisRateLimit(5, 60, "2fa_verify_login")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> VerifyLogin([FromBody] VerifyLoginDto dto)
    {
        var r = await _sender.Send(new AuthService.Application.Auths.TwoFactor.VerifyLoginCommand(dto.UserId, dto.Code));
        // OTP/recovery code đúng => Tokens.Token có giá trị thật; set cookie giống luồng login thường (13.9).
        if (r.Tokens?.Token != null) _authRepo.SetTokenInsideCookie(r.Tokens.Token, HttpContext);
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
