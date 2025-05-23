using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.User.Requests;
using UserService.Application.DTOs.User.Responses;
using UserService.Application.Usecases.Users.Commands.CreateUser;
using UserService.Application.Usecases.Users.Commands.UpdateUser;
using UserService.Application.Usecases.Users.Queries.GetUser;
using UserService.Application.Usecases.Users.Queries.GetUsers;

namespace UserService.API.Controller;

[Route("api/user")]
[ApiController]
[Authorize]
public class UserController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;
    private readonly IAuthorizeExtension _authorizeExtension;


    public UserController(ISender sender, IMapper mapper, IAuthorizeExtension authorizeExtension)
    {
        _sender = sender;
        _mapper = mapper;
        _authorizeExtension = authorizeExtension;
    }


    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest createUserRequest)
    {
        var result = await _sender.Send(new CreateUserCommand(createUserRequest));
        var response = new ResponseDto(result, Message: "Create Successful");
        return Ok(response);
    }

    [HttpPut("update-user")]
    public async Task<IActionResult> UpdateUser([FromBody] UserInfoDto updateUserInfoRequest)
    {
        var result = await _sender.Send(new UpdateUserCommand(updateUserInfoRequest));
        var response = new ResponseDto(result, Message: "Update Successful");
        return Ok(response);
    }

    [HttpGet("get-user")]
    public async Task<IActionResult> GetUser([FromQuery] GetUserRequest getUserRequest)
    {
        var user = _authorizeExtension.GetUserFromClaimToken();
        if (getUserRequest.Id == Guid.Empty)
        {
            getUserRequest = getUserRequest with { Id = user.Id };
        }
        var result = await _sender.Send(new GetUserQuery(getUserRequest));
        var response = new ResponseDto(result, Message: "Get Successful");
        return Ok(response);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = _authorizeExtension.GetUserFromClaimToken();
        var result =
            await _sender.Send(
                new GetUserQuery(new GetUserRequest(Id: user.Id, GetId: null, Username: user.UserName,
                    Email: user.FullName)));
        var response = new ResponseDto(result, Message: "Get Successful");
        return Ok(response);
    }


    [HttpGet("get-users")]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest getUsersRequest)
    {
        var result = await _sender.Send(new GetUsersQuery(getUsersRequest));
        return Ok(result);
    }
}