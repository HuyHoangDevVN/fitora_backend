using BuildingBlocks.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

/// <summary>
/// CCCD/OCR (26.17) — dữ liệu nhạy cảm nhất trong hệ thống (số CCCD, ảnh giấy tờ,
/// ngày sinh, địa chỉ). Trước đây thiếu [Authorize]: request không token vẫn chạy
/// tới tận Application layer mới bị chặn (GetUserFromClaimToken throw), thay vì bị
/// chặn sớm ở middleware với 401 chuẩn.
/// </summary>
[Route("api/auth/identity-verification")]
[ApiController]
[Authorize]
public class IdentityVerificationController : Controller
{
    private readonly ISender _sender;
    public IdentityVerificationController(ISender sender) => _sender = sender;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromBody] UploadDto dto)
    {
        var r = await _sender.Send(new AuthService.Application.Auths.IdentityVerification.UploadDocumentCommand(dto.FrontImageUrl, dto.BackImageUrl));
        return Ok(new ResponseDto(r));
    }
    [HttpPost("ocr")]
    public async Task<IActionResult> Ocr([FromBody] OcrDto dto)
    {
        var r = await _sender.Send(new AuthService.Application.Auths.IdentityVerification.OcrDocumentCommand(dto.VerificationId));
        return Ok(new ResponseDto(r));
    }
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitDto dto)
    {
        var r = await _sender.Send(new AuthService.Application.Auths.IdentityVerification.SubmitVerificationCommand(
            dto.VerificationId, dto.DocumentNumber, dto.FullName, dto.DateOfBirth, dto.Address));
        return Ok(new ResponseDto(r, IsSuccess: r.IsSuccess, Message: r.Message));
    }
    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var r = await _sender.Send(new AuthService.Application.Auths.IdentityVerification.GetVerificationStatusQuery());
        return Ok(new ResponseDto(r));
    }
}
public record UploadDto(string FrontImageUrl, string? BackImageUrl);
public record OcrDto(Guid VerificationId);
public record SubmitDto(Guid VerificationId, string DocumentNumber, string FullName, DateTime DateOfBirth, string Address);
