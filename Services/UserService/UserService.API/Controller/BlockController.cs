using BuildingBlocks.Pagination.Base;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Usecases.Blocks.Commands.BlockGroup;
using UserService.Application.Usecases.Blocks.Commands.BlockUser;
using UserService.Application.Usecases.Blocks.Commands.UnblockGroup;
using UserService.Application.Usecases.Blocks.Commands.UnblockUser;
using UserService.Application.Usecases.Blocks.Queries.GetBlockedGroups;
using UserService.Application.Usecases.Blocks.Queries.GetBlockedUsers;

namespace UserService.API.Controller;

/// <summary>
/// Block / unblock user — mục 13.7 / 26.8 Claude.md.
/// Danh sách đã có Entity + DbSet nhưng chưa từng có Controller/CQRS; FE không
/// gọi được API nào. Filter server-side (feed/search/notification) nên triển
/// khai ở InteractService & NotificationService sau khi BE Block ổn định.
/// </summary>
[Route("api/user/block")]
[ApiController]
[Authorize]
public class BlockController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ISender _sender;
    private readonly IAuthorizeExtension _auth;

    public BlockController(ISender sender, IAuthorizeExtension auth)
    {
        _sender = sender;
        _auth = auth;
    }

    [HttpPost("users/{userId:guid}")]
    public async Task<IActionResult> BlockUser([FromRoute] Guid userId)
    {
        var blockerId = _auth.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new BlockUserCommand(blockerId, userId));
        return Ok(result);
    }

    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> UnblockUser([FromRoute] Guid userId)
    {
        var blockerId = _auth.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new UnblockUserCommand(blockerId, userId));
        return Ok(result);
    }

    [HttpPost("groups/{groupId:guid}")]
    public async Task<IActionResult> BlockGroup([FromRoute] Guid groupId)
    {
        var blockerId = _auth.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new BlockGroupCommand(blockerId, groupId));
        return Ok(result);
    }

    [HttpDelete("groups/{groupId:guid}")]
    public async Task<IActionResult> UnblockGroup([FromRoute] Guid groupId)
    {
        var blockerId = _auth.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new UnblockGroupCommand(blockerId, groupId));
        return Ok(result);
    }

    [HttpGet("groups")]
    public async Task<IActionResult> GetBlockedGroups(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20)
    {
        var blockerId = _auth.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new GetBlockedGroupsQuery(blockerId, pageIndex, pageSize));
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetBlockedUsers(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 20)
    {
        var blockerId = _auth.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new GetBlockedUsersQuery(blockerId, pageIndex, pageSize));
        return Ok(result);
    }
}
