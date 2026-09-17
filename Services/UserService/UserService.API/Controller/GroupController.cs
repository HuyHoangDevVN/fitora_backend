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
using UserService.Application.Usecases.Group.Commands.DissolveGroup;
using UserService.Application.Usecases.Group.Commands.TransferOwner;
using UserService.Application.Usecases.Group.Commands.UpdateGroup;
using UserService.Application.Usecases.Group.Commands.UpdatePrivacy;
using UserService.Application.Usecases.Group.Queries.GetGroupById;
using UserService.Application.Usecases.Group.Queries.GetGroups;
using UserService.Application.Usecases.Group.Queries.GetJoinedGroups;
using UserService.Application.Usecases.Group.Queries.GetManagedGroups;
using UserService.Application.Usecases.GroupPost.Commands.ApproveGroupPost;
using UserService.Application.Usecases.GroupPost.Commands.RejectGroupPost;
using UserService.Application.Usecases.GroupPost.Queries.GetPendingGroupPosts;
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
using UserService.Application.Usecases.Group.Queries.SearchGroups;
using UserService.Application.Usecases.GroupEvent.Commands.CreateGroupEvent;
using UserService.Application.Usecases.GroupEvent.Commands.DeleteGroupEvent;
using UserService.Application.Usecases.GroupEvent.Commands.RsvpEvent;
using UserService.Application.Usecases.GroupEvent.Commands.UpdateGroupEvent;
using UserService.Application.Usecases.GroupEvent.Queries.GetGroupEventById;
using UserService.Application.Usecases.GroupEvent.Queries.GetGroupEvents;
using UserService.Domain.Enums;

namespace UserService.API.Controller;

[Route("api/user/group")]
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

    /// <summary>
    /// Gets groups.
    /// </summary>
    [HttpGet("get-list")]
    public async Task<IActionResult> GetList([FromQuery] GetGroupsRequest request)
    {
        var result = await _sender.Send(new GetGroupsQuery(request));
        return Ok(new ResponseDto(result));
    }

    /// <summary>
    /// Privacy-aware group search: public groups visible to everyone;
    /// private/secret groups only when the caller is a member.
    /// Keyword matches Name/Description (case-insensitive contains).
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchGroups(
        [FromQuery] string? query, [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new SearchGroupsQuery(
            new SearchGroupsRequest(query, pageIndex, pageSize), userId));
        return Ok(new ResponseDto(result));
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

    // ==============================
    // Group Post Moderation (26.3)
    // ==============================

    /// <summary>Pending posts of a group (paginated).</summary>
    [HttpGet("{groupId:guid}/pending-posts")]
    public async Task<IActionResult> GetPendingGroupPosts(
        [FromRoute] Guid groupId, [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new GetPendingGroupPostsQuery(groupId, pageIndex, pageSize));
        return Ok(new ResponseDto(result));
    }

    public record RejectBody(string? Reason);

    /// <summary>Approve a pending group post (Owner/Admin/Moderator).</summary>
    [HttpPost("posts/{postId:guid}/approve")]
    public async Task<IActionResult> ApproveGroupPost([FromRoute] Guid postId)
    {
        var result = await _sender.Send(new ApproveGroupPostCommand(postId));
        return Ok(result);
    }

    /// <summary>Reject a pending group post (Owner/Admin/Moderator).</summary>
    [HttpPost("posts/{postId:guid}/reject")]
    public async Task<IActionResult> RejectGroupPost([FromRoute] Guid postId, [FromBody] RejectBody body)
    {
        var result = await _sender.Send(new RejectGroupPostCommand(postId, body?.Reason));
        return Ok(result);
    }

    // ==============================
    // Ownership & Dissolve (26.2)
    // ==============================

    public record TransferOwnerBody(Guid NewOwnerId);
    public record UpdatePrivacyBody(GroupPrivacy Privacy);

    /// <summary>Transfer group ownership (Owner only).</summary>
    [HttpPost("{groupId:guid}/transfer-owner")]
    public async Task<IActionResult> TransferOwner([FromRoute] Guid groupId, [FromBody] TransferOwnerBody body)
    {
        var result = await _sender.Send(new TransferOwnerCommand(groupId, body.NewOwnerId));
        return Ok(result);
    }

    /// <summary>Dissolve group (Owner only).</summary>
    [HttpDelete("{groupId:guid}/dissolve")]
    public async Task<IActionResult> DissolveGroup([FromRoute] Guid groupId)
    {
        var result = await _sender.Send(new DissolveGroupCommand(groupId));
        return Ok(result);
    }

    /// <summary>Update group privacy (Owner/Admin only).</summary>
    [HttpPut("{groupId:guid}/privacy")]
    public async Task<IActionResult> UpdatePrivacy([FromRoute] Guid groupId, [FromBody] UpdatePrivacyBody body)
    {
        var result = await _sender.Send(new UpdatePrivacyCommand(groupId, body.Privacy));
        return Ok(result);
    }

    // ==============================
    // Group Events (27.2 / 26.16)
    // ==============================

    public record CreateEventBody(string Title, string Description, DateTime EventDate, string? Location);
    public record UpdateEventBody(string Title, string Description, DateTime EventDate, string? Location);
    public record RsvpBody(RsvpStatus Status);

    /// <summary>Tạo sự kiện (Owner/Admin/Moderator).</summary>
    [HttpPost("{idGroup:guid}/events")]
    public async Task<IActionResult> CreateGroupEvent([FromRoute] Guid idGroup, [FromBody] CreateEventBody body)
    {
        var result = await _sender.Send(new CreateGroupEventCommand(
            idGroup, body.Title, body.Description, body.EventDate, body.Location));
        return Ok(result);
    }

    /// <summary>Danh sách sự kiện của nhóm.</summary>
    [HttpGet("{idGroup:guid}/events")]
    public async Task<IActionResult> GetGroupEvents(
        [FromRoute] Guid idGroup, [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new GetGroupEventsQuery(idGroup, pageIndex, pageSize));
        return Ok(new ResponseDto(result));
    }

    /// <summary>Chi tiết một sự kiện.</summary>
    [HttpGet("events/{eventId:guid}")]
    public async Task<IActionResult> GetGroupEventById([FromRoute] Guid eventId)
    {
        var result = await _sender.Send(new GetGroupEventByIdQuery(eventId));
        return Ok(new ResponseDto(result));
    }

    /// <summary>Cập nhật sự kiện (Owner/Admin/Moderator).</summary>
    [HttpPut("events/{eventId:guid}")]
    public async Task<IActionResult> UpdateGroupEvent([FromRoute] Guid eventId, [FromBody] UpdateEventBody body)
    {
        var result = await _sender.Send(new UpdateGroupEventCommand(
            eventId, body.Title, body.Description, body.EventDate, body.Location));
        return Ok(result);
    }

    /// <summary>Xóa sự kiện (Owner/Admin/Moderator).</summary>
    [HttpDelete("events/{eventId:guid}")]
    public async Task<IActionResult> DeleteGroupEvent([FromRoute] Guid eventId)
    {
        var result = await _sender.Send(new DeleteGroupEventCommand(eventId));
        return Ok(result);
    }

    /// <summary>RSVP đi/không thể/không (member).</summary>
    [HttpPost("events/{eventId:guid}/rsvp")]
    public async Task<IActionResult> RsvpGroupEvent([FromRoute] Guid eventId, [FromBody] RsvpBody body)
    {
        var result = await _sender.Send(new RsvpEventCommand(eventId, body.Status));
        return Ok(result);
    }
}