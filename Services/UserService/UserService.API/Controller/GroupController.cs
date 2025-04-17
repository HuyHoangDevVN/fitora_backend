using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Group.Requests;
using UserService.Application.DTOs.GroupInvite.Requests;
using UserService.Application.DTOs.GroupMember.Requests;
using UserService.Application.DTOs.GroupPost.Requests;
using UserService.Application.Usecases.Group.Commands.CreateGroup;
using UserService.Application.Usecases.Group.Commands.DeleteGroup;
using UserService.Application.Usecases.Group.Commands.UpdateGroup;
using UserService.Application.Usecases.Group.Queries.GetGroupById;
using UserService.Application.Usecases.Group.Queries.GetJoinedGroups;
using UserService.Application.Usecases.Group.Queries.GetManagedGroups;
using UserService.Application.Usecases.GroupInvite.Commands.AcceptGroupInvite;
using UserService.Application.Usecases.GroupInvite.Commands.CreateGroupInvite;
using UserService.Application.Usecases.GroupInvite.Commands.CreateGroupInvites;
using UserService.Application.Usecases.GroupInvite.Commands.DeleteGroupInvite;
using UserService.Application.Usecases.GroupInvite.Queries.GetReceivedList;
using UserService.Application.Usecases.GroupInvite.Queries.GetSentList;
using UserService.Application.Usecases.GroupMember.Commands.AssignRoleMember;
using UserService.Application.Usecases.GroupMember.Commands.DeleteMember;
using UserService.Application.Usecases.GroupMember.Queries.GetByGroupId;
using UserService.Application.Usecases.GroupMember.Queries.GetById;
using UserService.Application.Usecases.GroupPost.Commands.CreateGroupPost;
using UserService.Application.Usecases.GroupPost.Commands.DeleteGroupPost;
using UserService.Application.Usecases.GroupPost.Commands.UpdateGroupPost;

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

    // ==============================
    // Group Management
    // ==============================

    /// <summary>
    /// Creates a new group.
    /// </summary>
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

    /// <summary>
    /// Updates an existing group.
    /// </summary>
    [HttpPut("update")]
    public async Task<IActionResult> UpdateGroupAsync([FromBody] UpdateGroupRequest request)
    {
        var result = await _sender.Send(new UpdateGroupCommand(request));
        return Ok(result);
    }

    /// <summary>
    /// Deletes a group by ID.
    /// </summary>
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteGroupAsync([FromQuery] Guid id)
    {
        var result = await _sender.Send(new DeleteGroupCommand(id));
        return Ok(result);
    }

    /// <summary>
    /// Gets a group by ID.
    /// </summary>
    [HttpGet("get-by-id")]
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

    // ==============================
    // Group Membership
    // ==============================

    /// <summary>
    /// Assigns a role to a group member.
    /// </summary>
    [HttpPost("assign-role-member")]
    public async Task<IActionResult> AssignRoleMemberAsync([FromBody] AssignRoleGroupMemberFromBody body)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new AssignRoleMemberCommand(
            new AssignRoleGroupMemberRequest(
                userId,
                body.GroupId,
                body.MemberId,
                body.Role
            )
        ));
        return Ok(result);
    }

    /// <summary>
    /// Invites a single member to a group.
    /// </summary>
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

    /// <summary>
    /// Invites multiple members to a group.
    /// </summary>
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

    /// <summary>
    /// Deletes a member from a group.
    /// </summary>
    [HttpDelete("delete-member")]
    public async Task<IActionResult> DeleteMemberAsync([FromQuery] Guid memberId)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new DeleteMemberCommand(
            memberId,
            userId
        ));
        return Ok(result);
    }

    /// <summary>
    /// Gets the list of members in a group.
    /// </summary>
    [HttpGet("get-members")]
    public async Task<IActionResult> GetMembersAsync([FromQuery] GetByGroupIdRequest request)
    {
        var groupMembers = await _sender.Send(new GetMembersByGroupIdQuery(request));
        return Ok(groupMembers);
    }

    // ==============================
    // Group Invites
    // ==============================

    /// <summary>
    /// Gets the list of sent group invites.
    /// </summary>
    [HttpGet("get-sent-group-invites")]
    public async Task<IActionResult> GetSentGroupInvitesAsync([FromQuery] GetSentGroupInviteFromQuery query)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var groupInvites = await _sender.Send(new GetSentListQuery(
            new GetSentGroupInviteRequest(
                userId,
                query.PageIndex,
                query.PageSize
            )
        ));
        return Ok(groupInvites);
    }

    /// <summary>
    /// Gets the list of received group invites.
    /// </summary>
    [HttpGet("get-received-group-invites")]
    public async Task<IActionResult> GetReceivedGroupInvitesAsync([FromQuery] GetReceivedGroupInviteFromQuery query)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var groupInvites = await _sender.Send(new GetReceivedListQuery(
            new GetReceivedGroupInviteRequest(
                userId,
                query.PageIndex,
                query.PageSize
            )
        ));
        return Ok(groupInvites);
    }

    /// <summary>
    /// Accepts a group invite.
    /// </summary>
    [HttpPost("accept-group-invite")]
    public async Task<IActionResult> AccpetGroupInviteAsync([FromQuery] Guid Id)
    {
        var result = await _sender.Send(new AcceptGroupInviteCommand(Id));
        return Ok(result);
    }

    /// <summary>
    /// Deletes a group invite.
    /// </summary>
    [HttpDelete("delete-group-invite")]
    public async Task<IActionResult> DeleteGroupInviteAsync([FromQuery] Guid Id)
    {
        var result = await _sender.Send(new DeleteGroupInviteCommand(Id));
        return Ok(result);
    }

    // ==============================
    // Group Posts
    // ==============================

    /// <summary>
    /// Creates a new group post.
    /// </summary>
    [HttpPost("create-group-post")]
    public async Task<IActionResult> CreateGroupPostAsync([FromBody] CreateGroupPostRequest request)
    {
        var result = await _sender.Send(new CreateGroupPostCommand(request));
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing group post.
    /// </summary>
    [HttpPut("update-group-post")]
    public async Task<IActionResult> UpdateGroupPostAsync([FromBody] UpdateGroupPostRequest request)
    {
        var result = await _sender.Send(new UpdateGroupPostCommand(request));
        return Ok(result);
    }

    /// <summary>
    /// Deletes a group post by ID.
    /// </summary>
    [HttpDelete("delete-group-post")]
    public async Task<IActionResult> DeleteGroupPostAsync([FromQuery] Guid id)
    {
        var result = await _sender.Send(new DeleteGroupPostCommand(id));
        return Ok(result);
    }

    // ==============================
    // Group Queries
    // ==============================

    /// <summary>
    /// Gets the list of groups managed by the user.
    /// </summary>
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

    /// <summary>
    /// Gets the list of groups the user has joined.
    /// </summary>
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
}