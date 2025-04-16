using AuthService.Application.Auths.Commands.EditInForUser;
using AuthService.Application.Auths.Queries.GetUser;
using AuthService.Application.Auths.Queries.GetUsers;
using AuthService.Application.DTOs.Users.Requests;
using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination;
using BuildingBlocks.Pagination.Base;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

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
        var response = new ResponseDto(result, IsSuccess: (bool)(result != null),
            Message: result != null ? "Get User Successful" : "Get User Failure");
        return Ok(response);
    }

    [HttpGet("get-users")]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationRequest request)
    {
        var result = await _sender.Send(new GetUsersQuery(request));
        var response = new ResponseDto(Data: result.PaginatedResult, Message: "Get Users successful");
        return Ok(response);
    }

    [HttpPatch("update-user")]
    public async Task<IActionResult> UpdateUser(EditInfoUserRequest req)
    {
        var command = _mapper.Map<EditInForUserCommand>(req);
        var result = await _sender.Send(command);

        var response = new ResponseDto(result, IsSuccess: !string.IsNullOrEmpty(result.UserId),
            Message: !string.IsNullOrEmpty(result.UserId) ? "Success" : "Failure");

        return Ok(response);
    }
}