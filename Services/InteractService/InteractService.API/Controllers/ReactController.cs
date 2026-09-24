using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using InteractService.Application.DTOs.React.Requests;
using InteractService.Application.Usecases.React.Commands.ToggleReact;
using InteractService.Application.Usecases.React.Queries.GetReacts;
using InteractService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/interact/react")]
public class ReactController : Controller
{
    private readonly IMediator _mediator;
    private readonly IAuthorizeExtension _auth;

    public ReactController(IMediator mediator, IAuthorizeExtension auth)
    {
        _mediator = mediator;
        _auth = auth;
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle([FromBody] ToggleReactFormBody body)
    {
        var req = new ToggleReactRequest(body.TargetType, body.TargetId, body.ReactType);
        var result = await _mediator.Send(new ToggleReactCommand(req));
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] TargetType targetType, [FromQuery] Guid targetId)
    {
        var result = await _mediator.Send(new GetReactsQuery(new GetReactsRequest(targetType, targetId)));
        return Ok(result);
    }
}
