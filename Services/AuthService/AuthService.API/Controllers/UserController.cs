using AuthService.API.Endpoints.Users;
using AuthService.Application.Auths.Commands.EditInForUser;
using AuthService.Application.Auths.Queries.GetUser;
using AuthService.Application.Auths.Queries.GetUsers;
using AutoMapper;
using BuildingBlocks.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/user")]
public class UserController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public UserController(IMapper mapper, ISender sender)
    {
        _mapper = mapper;
        _sender = sender;
    }

    [HttpGet("get-user")]
    public async Task<IActionResult> GetUser(string id)
    {
        var result = await _sender.Send(new GetUserQuery(id));
        var response = new GetUserResponse(result, IsSuccess: (bool)(result != null),
            Message: result != null ? "Get User Successful" : "Get User Failure");
        return Ok(response);
    }

    [HttpGet("get-users")]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationRequest request)
    {
        var result = await _sender.Send(new GetUsersQuery(request));
        var response = new GetUsersResponse(MetaData: result.PaginatedResult, Message: "Get Users successful");
        return Ok(response);
    }

    [HttpPatch("update-user")]
    public async Task<IActionResult> UpdateUser(EditInForUserRequest req)
    {
        var command = _mapper.Map<EditInForUserCommand>(req);
        var result = await _sender.Send(command);

        var response = new EditInForUserResponse(result, IsSuccess: !string.IsNullOrEmpty(result.UserId),
            Message: !string.IsNullOrEmpty(result.UserId) ? "Success" : "Failure");

        return Ok(response);
    }

    [HttpGet("test-auth")]
    public async Task<IActionResult> TestAuth()
    {
        return Ok(new GetUserResponse(Message: "Testing auth", IsSuccess: true, MetaData: null));
    }
    
}