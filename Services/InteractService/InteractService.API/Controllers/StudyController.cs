using BuildingBlocks.DTOs;
using InteractService.Application.Usecases.StudyPosts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

[Route("api/interact/study")]
[ApiController]
public class StudyController : Controller
{
    private readonly IMediator _mediator;
    public StudyController(IMediator mediator) => _mediator = mediator;

    [Authorize]
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateStudyPostDto dto)
    {
        var r = await _mediator.Send(new CreateStudyPostCommand(dto.Title, dto.Content, dto.CategoryId, dto.AttachmentUrls));
        return Ok(new ResponseDto(r));
    }
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<IActionResult> List([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 20, [FromQuery] Guid? categoryId = null, [FromQuery] string? q = null)
    {
        var r = await _mediator.Send(new GetStudyPostsQuery(pageIndex, pageSize, categoryId, q));
        return Ok(new ResponseDto(r));
    }
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var r = await _mediator.Send(new GetStudyPostByIdQuery(id));
        return Ok(new ResponseDto(r));
    }
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudyPostDto dto)
    {
        var r = await _mediator.Send(new UpdateStudyPostCommand(id, dto.Title, dto.Content));
        return Ok(new ResponseDto(r));
    }
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await _mediator.Send(new DeleteStudyPostCommand(id));
        return Ok(r);
    }
}
public record CreateStudyPostDto(string Title, string Content, Guid? CategoryId, List<string>? AttachmentUrls);
public record UpdateStudyPostDto(string Title, string Content);
