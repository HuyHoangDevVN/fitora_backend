using AutoMapper;
using BuildingBlocks.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.User.Requests;
using UserService.Application.DTOs.User.Responses;
using UserService.Application.Usecases.Users.Commands;
using UserService.Application.Usecases.Users.Commands.CreateUser;
using UserService.Application.Usecases.Users.Commands.UpdateUser;
using UserService.Application.Usecases.Users.Queries.GetUser;
using UserService.Application.Usecases.Users.Queries.GetUsers;
using UserService.Domain.Models;

namespace UserService.API.Controller;

[Route("api/user")]
[ApiController]
public class UserController : Microsoft.AspNetCore.Mvc.Controller
{
    
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public UserController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;       
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
        var result = await _sender.Send(new GetUserQuerry(getUserRequest));
        var response = new ResponseDto(result, Message: "Get Successful");
        return Ok(response);
    }

    [HttpGet("get-users")]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest getUsersRequest)
    {
        var result = await _sender.Send(new GetUsersQuerry(getUsersRequest));
        return Ok(result);
    }
}