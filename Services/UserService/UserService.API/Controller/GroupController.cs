using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Group.Requests;
using UserService.Application.DTOs.GroupInvite.Requests;
using UserService.Application.Usecases.Group.Commands.CreateGroup;
using UserService.Application.Usecases.Group.Commands.DeleteGroup;
using UserService.Application.Usecases.Group.Commands.UpdateGroup;
using UserService.Application.Usecases.Group.Queries.GetGroupById;
using UserService.Application.Usecases.Group.Queries.GetJoinedGroups;
using UserService.Application.Usecases.Group.Queries.GetManagedGroups;
using UserService.Application.Usecases.GroupInvite.Commands.CreateGroupInvite;
using UserService.Application.Usecases.GroupInvite.Commands.CreateGroupInvites;

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
    public async Task<IActionResult> CreateGroupAsync([FromBody] CreateGroupFromBody body)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new CreateGroupCommand(
            new CreateGroupRequest(
                userId,
                body.Name,
                body.Description,
                body.Privacy,
                body.RequirePostApproval,
                body.CoverImageUrl,
                body.AvatarUrl
            )
        ));
        return Ok(result);
    }

    [HttpPost("invite-new-member")]
    public async Task<IActionResult> InviteNewMemberAsync([FromBody] CreateGroupInviteFormBody body)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new CreateGroupInviteCommand(
            new CreateGroupInviteRequest(
                body.GroupId,
                userId,
                body.ReceiverUserId
            )
        ));
        return Ok(result);
    }

    [HttpPost("invite-new-members")]
    public async Task<IActionResult> InviteNewMembersAsync([FromBody] CreateGroupInvitesFormBody body)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new CreateGroupInvitesCommand(
            new CreateGroupInvitesRequest(
                body.GroupId,
                userId,
                body.ReceiverUserIds
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

    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        var group = await _sender.Send(new GetGroupByIdQuery(
            id
        ));
        return Ok(group);
    }

    [HttpGet("get-managed-groups")]
    public async Task<IActionResult> GetManagedGroupsAsync([FromQuery] GetManagedGroupsFromQuery query)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var groups = await _sender.Send(new GetManagedGroupsQuery(
            new GetManagedGroupsRequest(
                userId,
                query.PageIndex,
                query.PageSize
            )
        ));
        return Ok(groups);
    }

    [HttpGet("get-joined-groups")]
    public async Task<IActionResult> GetJoinedGroupsAsync([FromQuery] GetJoinedGroupsFromQuery query)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var groups = await _sender.Send(new GetJoinedGroupsQuery(
            new GetJoinedGroupsRequest(
                userId,
                query.IsAll,
                query.PageIndex,
                query.PageSize
            )
        ));
        return Ok(groups);
    }


    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteGroupAsync([FromQuery] Guid id)
    {
        var result = await _sender.Send(new DeleteGroupCommand(id));
        return Ok(result);
    }
}