using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Group.Requests;
using UserService.Application.Usecases.Group.Commands.CreateGroup;
using UserService.Application.Usecases.Group.Commands.DeleteGroup;
using UserService.Application.Usecases.Group.Commands.UpdateGroup;

namespace UserService.API.Controller;

[Route("api/group")]
[ApiController]
[Authorize]
public class GroupController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ISender _sender;
    private readonly IAuthorizeExtension _authorizeExtension;

    public GroupController(ISender sender, IAuthorizeExtension authorizeExtension)
    {
        _sender = sender;
        _authorizeExtension = authorizeExtension;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateGroupAsync([FromBody] CreateGroupFromBody request)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new CreateGroupCommand(
            new CreateGroupRequest(
                userId,
                request.Name,
                request.Description,
                request.Privacy,
                request.RequirePostApproval,
                request.CoverImageUrl,
                request.AvatarUrl
            )
        ));
        return Ok(result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateGroupAsync([FromBody] UpdateGroupRequest request)
    {
        var result = await _sender.Send(new UpdateGroupCommand(request));
        return Ok(result);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteGroupAsync([FromQuery] Guid id)
    {
        var result = await _sender.Send(new DeleteGroupCommand(id));
        return Ok(result);
    }
}