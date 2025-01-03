using AutoMapper;
using BuildingBlocks.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.User.Requests;
using UserService.Application.Usecases.Users.Commands;
using UserService.Application.Usecases.Users.Commands.CreateUser;
using UserService.Application.Usecases.Users.Commands.UpdateUser;
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
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest updateUserRequest)
    {
        var result = await _sender.Send(new UpdateUserCommand(updateUserRequest));
        var response = new ResponseDto(result, Message: "Update Successful");
        return Ok(response);
    }
}