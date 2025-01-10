using AutoMapper;
using BuildingBlocks.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Friendship.Requests;
using UserService.Application.Usecases.Friendship.Command.AccpectFriendRequest;
using UserService.Application.Usecases.Friendship.Command.CreateFriendRequest;
using UserService.Application.Usecases.Friendship.Command.DeleteFriendRequest;
using UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Received;
using UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Sended;
using UserService.Application.Usecases.Friendship.Queries.GetFriends;

namespace UserService.API.Controller;

[Route("api/friendship")]
[ApiController]
public class FriendShipController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public FriendShipController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpPost("add-friend")]
    public async Task<IActionResult> AddFriend([FromBody] CreateFriendRequest request)
    {
        var result = await _sender.Send(new CreateFriendRequestCommand(request));
        return Ok(result);
    }

    [HttpGet("get-sent-friend-requests")]
    public async Task<IActionResult> GetSentFriendRequests([FromQuery] GetSentFriendRequest request)
    {
        var result = await _sender.Send(new GetSentFriendRequestQuerry(request));
        return Ok(result);
    }

    [HttpGet("get-received-friend-requests")]
    public async Task<IActionResult> GetReceivedFriendRequests([FromQuery] GetReceivedFriendRequest request)
    {
        var result = await _sender.Send(new GetReceivedFriendRequestQuerry(request));
        return Ok(result);
    }

    [HttpGet("get-friends")]
    public async Task<IActionResult> GetFriends([FromQuery] GetFriendsRequest request)
    {
        var result = await _sender.Send(new GetFriendsQuerry(request));
        return Ok(result);
    }
    
    [HttpPut("accept-friend-request")]
    public async Task<IActionResult> AccpectFriendRequest(Guid id)
    {
        var result = await _sender.Send(new AcceptFriendRequestCommand(id));
        return Ok(result);
    }

    [HttpDelete("delete-request")]
    public async Task<IActionResult> DeleteFriendRequest(Guid requestId)
    {
        var result = await _sender.Send(new DeleteFriendRequestCommand(requestId));
        return Ok(new ResponseDto(null, result, result ? "Successed" : "Failed"));
    }
}