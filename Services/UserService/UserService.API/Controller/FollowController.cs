using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Follow.Requests;
using UserService.Application.DTOs.Friendship.Requests;
using UserService.Application.Usecases.Follow.Commands.Follow;
using UserService.Application.Usecases.Follow.Commands.Unfollow;
using UserService.Application.Usecases.Follow.Queries.GetFollowees;
using UserService.Application.Usecases.Follow.Queries.GetFollowers;
using UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Received;
using UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Sended;
using UserService.Application.Usecases.Friendship.Queries.GetFriends;

namespace UserService.API.Controller;

[Route("api/user/follow")]
[ApiController]
[Authorize]
public class FollowController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ISender _sender;
    private readonly IAuthorizeExtension _authorizeExtension;

    public FollowController(ISender sender, IMapper mapper, IAuthorizeExtension authorizeExtension)
    {
        _sender = sender;
        _authorizeExtension = authorizeExtension;
    }

    [HttpPost("follow")]
    public async Task<IActionResult> Follow([FromBody] Guid id)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new FollowCommand(new FollowRequest(userGuid, id)
        ));
        return Ok(result);
    }

    [HttpPost("unfollow")]
    public async Task<IActionResult> Unfollow([FromBody] Guid id)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new UnfollowCommand(new FollowRequest(userGuid, id)));
        return Ok(result);
    }

    [HttpGet("get-followers")]
    public async Task<IActionResult> GetFollowers([FromQuery] PaginationRequest request)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;

        var sended = new GetFollowersRequest(userGuid, request.PageIndex, request.PageSize);

        try
        {
            var result = await _sender.Send(new GetFollowersQuery(sended));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("get-followees")]
    public async Task<IActionResult> GetFollowees([FromQuery] PaginationRequest request)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;

        var sended = new GetFollowersRequest(userGuid, request.PageIndex, request.PageSize);

        try
        {
            var result = await _sender.Send(new GetFolloweesQuery(sended));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}