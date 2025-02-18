using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Follow.Requests;
using UserService.Application.DTOs.Friendship.Requests;
using UserService.Application.Usecases.Follow.Command.Follow;
using UserService.Application.Usecases.Follow.Command.Unfollow;
using UserService.Application.Usecases.Follow.Queries.GetFollowees;
using UserService.Application.Usecases.Follow.Queries.GetFollowers;
using UserService.Application.Usecases.Friendship.Command.AccpectFriendRequest;
using UserService.Application.Usecases.Friendship.Command.CreateFriendRequest;
using UserService.Application.Usecases.Friendship.Command.DeleteFriendRequest;
using UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Received;
using UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Sended;
using UserService.Application.Usecases.Friendship.Queries.GetFriends;

namespace UserService.API.Controller;

[Route("api/follow")]
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
    public async Task<IActionResult> Follow([FromBody] FollowRequest request)
    {
        var result = await _sender.Send(new FollowCommand(request));
        return Ok(result);
    }
    
    [HttpPost("unfollow")]
    public async Task<IActionResult> Unfollow([FromBody] FollowRequest request)
    {
        var result = await _sender.Send(new UnfollowCommand(request));
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