using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using InteractService.Application.Usecases.Shares;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/interact/share")]
public class ShareController : Controller
{
    private readonly IMediator _mediator;

    public ShareController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Chia sẻ bài viết — tạo 1 Post mới của người chia sẻ, trỏ về bài gốc.</summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateShareFormBody body)
    {
        var result = await _mediator.Send(new CreateShareCommand(body.OriginalPostId, body.Caption, body.ShareTo));
        return Ok(new ResponseDto(result, IsSuccess: true, Message: "Chia sẻ bài viết thành công"));
    }

    /// <summary>Tổng số lượt chia sẻ của 1 bài viết gốc.</summary>
    [HttpGet("count")]
    public async Task<IActionResult> Count([FromQuery] Guid originalPostId)
    {
        var count = await _mediator.Send(new GetSharesCountQuery(originalPostId));
        return Ok(new ResponseDto(count, IsSuccess: true));
    }
}

public record CreateShareFormBody(Guid OriginalPostId, string? Caption, int ShareTo = 0);
