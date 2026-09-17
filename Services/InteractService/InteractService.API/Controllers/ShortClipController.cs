using BuildingBlocks.DTOs;
using InteractService.Application.Usecases.ShortClips;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

[Route("api/interact/short-clips")]
[ApiController]
public class ShortClipController : Controller
{
    private readonly IMediator _mediator;
    public ShortClipController(IMediator mediator) => _mediator = mediator;

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
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await _mediator.Send(new DeleteShortClipCommand(id));
        return Ok(r);
    }
}
public record CreateShortClipDto(string VideoUrl, string? ThumbnailUrl, string? Caption, int Duration);
