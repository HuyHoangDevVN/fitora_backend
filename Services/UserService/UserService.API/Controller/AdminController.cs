using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Group.Requests;
using UserService.Application.Usecases.Group.Commands.DeleteGroup;
using UserService.Application.Usecases.Group.Queries.GetGroupById;
using UserService.Application.Usecases.Group.Queries.GetGroups;
using UserService.Application.Usecases.GroupMember.Queries.GetById;

namespace UserService.API.Controller;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/user/admin")]
public class AdminController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ISender _sender;
    private readonly IAuthorizeExtension _authorizeExtension;

    public AdminController(ISender sender, IAuthorizeExtension authorizeExtension)
    {
        _sender = sender;
        _authorizeExtension = authorizeExtension;
    }

    [HttpGet("get-group")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var group = await _sender.Send(new GetGroupByIdQuery(id));
        var groupMember = await _sender.Send(new GetMemberByIdQuery(userId, group.Id));
        return Ok(new ResponseDto(
            new
            {
                group,
                groupMember
            }));
    }

    [HttpGet("get-groups")]
    public async Task<IActionResult> GetGroups([FromQuery] GetGroupsRequest request)
    {
        var result = await _sender.Send(new GetGroupsQuery(request));
        return Ok(new ResponseDto(result));
    }

    [HttpDelete("delete-group")]
    public async Task<IActionResult> DeleteGroupAsync([FromQuery] Guid id)
    {
        var result = await _sender.Send(new DeleteGroupCommand(id));
        return Ok(result);
    }
}