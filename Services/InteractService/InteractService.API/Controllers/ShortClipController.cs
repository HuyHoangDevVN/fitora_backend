using BuildingBlocks.DTOs;
using InteractService.Application.Usecases.ShortClips;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace InteractService.API.Controllers;

// Mục 12 audit: gate bằng feature flag "ShortClip" (appsettings.json > FeatureManagement) —
// cho phép tắt module Short Clip theo môi trường mà không cần deploy lại.
[FeatureGate("ShortClip")]
[Route("api/interact/short-clips")]
[ApiController]
public class ShortClipController : Controller
{
    private readonly IMediator _mediator;
    public ShortClipController(IMediator mediator) => _mediator = mediator;

    // Create/Delete cần AuthorId lấy từ token (auth.GetUserFromClaimToken()) —
    // controller trước đây thiếu [Authorize] nên request không token vẫn vào được handler.
    [Authorize]
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateShortClipDto dto)
    {
        var r = await _mediator.Send(new CreateShortClipCommand(dto.VideoUrl, dto.ThumbnailUrl, dto.Caption ?? "", dto.Duration));
        return Ok(new ResponseDto(r));
    }
    [HttpGet("")]
    public async Task<IActionResult> List([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 20)
    {
        var r = await _mediator.Send(new GetShortClipsQuery(pageIndex, pageSize));
        return Ok(new ResponseDto(r));
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var r = await _mediator.Send(new GetShortClipByIdQuery(id));
        return Ok(new ResponseDto(r));
    }
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await _mediator.Send(new DeleteShortClipCommand(id));
        return Ok(r);
    }
}
public record CreateShortClipDto(string VideoUrl, string? ThumbnailUrl, string? Caption, int Duration);
