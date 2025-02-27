// using AuthService.API.Endpoints.Roles;
using AuthService.Application.Auths.Commands.AssignRoles;
using AuthService.Application.Auths.Commands.CreateRole;
using AuthService.Application.Auths.Commands.DeleteRole;
using AuthService.Application.Auths.Commands.UpdateRole;
using AuthService.Application.Auths.Queries.GetRoles;
using AuthService.Application.DTOs.Roles.Requests;
using AutoMapper;
using BuildingBlocks.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[Route("api/role")]
[ApiController]
public class RoleController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public RoleController(IMapper mapper, ISender sender)
    {
        _mapper = mapper;
        _sender = sender;
    }

    [HttpPost("create-role")]
    public async Task<IActionResult> CreateRole(CreateRoleRequestDto req)
    {
        var command = _mapper.Map<CreateRoleCommand>(req);
        var result = await _sender.Send(command);
        var response = new ResponseDto(IsSuccess: result.IsSuccess,
            Data: result.IsSuccess ? "Create Role Successful" : "Create Role Failure");
        return Ok(response);
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(AssignRoleRequestDto req)
    {
        var command = _mapper.Map<AssignRolesCommand>(req);
        var result = await _sender.Send(command);
        var response = new ResponseDto(Data: null, IsSuccess: result.IsSuccess,
            Message: result.IsSuccess ? "AssignRole Successful" : "AssignRole Failure");
        return Ok(response);
    }

    [HttpPut("update-role")]
    public async Task<IActionResult> UpdateRole(UpdateRoleRequestDto req)
    {
        var command = _mapper.Map<UpdateRoleCommand>(req);
        var result = await _sender.Send(command);
        var response = new ResponseDto(Data: null, IsSuccess: result.IsSuccess,
            Message: result.IsSuccess ? "Update Role Successful" : "Update Role Failure");
        return Ok(response);
    }

    [HttpGet("get-roles")]
    public async Task<IActionResult> GetRoles()
    {
        var result = await _sender.Send(new GetRolesQuery());
        var response = new ResponseDto(Data: result.Response, Message: "Get Roles Successful");
        return Ok(response);
    }

    [HttpDelete("delete-role")]
    public async Task<IActionResult> DeleteRole(string name)
    {
        var request = new DeleteRoleRequestDto(name);
        var command = _mapper.Map<DeleteRoleCommand>(request);
        var result = await _sender.Send(command);
        var response = new ResponseDto(Data: null, IsSuccess: result.IsSuccess,
            Message: result.IsSuccess ? "Delete Role Successful" : "Delete role failure");

        return Ok(response);
    }
}