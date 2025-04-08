using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Friendship.Requests;
using UserService.Application.Usecases.Friendship.Command.AccpectFriendRequest;
using UserService.Application.Usecases.Friendship.Command.CreateFriendRequest;
using UserService.Application.Usecases.Friendship.Command.DeleteFriendRequest;
using UserService.Application.Usecases.Friendship.Command.UnFriend;
using UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Received;
using UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Sended;
using UserService.Application.Usecases.Friendship.Queries.GetFriends;

namespace UserService.API.Controller;

[Route("api/friendship")]
[ApiController]
[Authorize]
public class FriendShipController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;
    private readonly IAuthorizeExtension _authorizeExtension;

    public FriendShipController(ISender sender, IMapper mapper, IAuthorizeExtension authorizeExtension)
    {
        _sender = sender;
        _mapper = mapper;
        _authorizeExtension = authorizeExtension;
    }

    [HttpPost("add-friend")]
    public async Task<IActionResult> AddFriend([FromBody] Guid id)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new CreateFriendRequestCommand(new CreateFriendRequest(userGuid, id)));
        return Ok(result);
    }

    [HttpGet("get-sent-friend-requests")]
    public async Task<IActionResult> GetSentFriendRequests([FromQuery] PaginationRequest request)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;

        var sended = new GetSentFriendRequest(userGuid, request.PageIndex, request.PageSize);

        try
        {
            var result = await _sender.Send(new GetSentFriendRequestQuerry(sended));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("get-received-friend-requests")]
    public async Task<IActionResult> GetReceivedFriendRequests([FromQuery] PaginationRequest request)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;

        var sended = new GetReceivedFriendRequest(userGuid, request.PageIndex, request.PageSize);

        try
        {
            var result = await _sender.Send(new GetReceivedFriendRequestQuerry(sended));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("get-friends")]
    public async Task<IActionResult> GetFriends([FromQuery] PaginationRequest request)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;

        var sended = new GetFriendsRequest(userGuid, request.PageIndex, request.PageSize);

        try
        {
            var result = await _sender.Send(new GetFriendsQuerry(sended));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("accept-friend-request")]
    public async Task<IActionResult> AcceptFriendRequest([FromBody] Guid id)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;

        var result = await _sender.Send(new AcceptFriendRequestCommand(
            new CreateFriendRequest(id, userGuid)));
        return Ok(result);
    }

    [HttpDelete("delete-request")]
    public async Task<IActionResult> DeleteFriendRequest([FromQuery] Guid id)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;
        var result = await _sender.Send(new DeleteFriendRequestCommand(new CreateFriendRequest(userGuid, id)));
        return Ok(new ResponseDto(null, result, result ? "Successed" : "Failed"));
    }

    [HttpDelete("unfriend")]
    public async Task<IActionResult> Unfriend([FromQuery] Guid id)
    {
        var result = await _sender.Send(new UnfriendCommand(id));
        return Ok(new ResponseDto(null, result, result ? "Successed" : "Failed"));
    }
}