using AuthService.Application.Auths.Commands.AssignRoles;
using AuthService.Application.Auths.Commands.AuthDeleteAccount;
using AuthService.Application.Auths.Commands.AuthLockAccount;
using AuthService.Application.Auths.Commands.CreateRole;
using AuthService.Application.Auths.Commands.DeleteRole;
using AuthService.Application.Auths.Commands.EditInForUser;
using AuthService.Application.Auths.Commands.LockAccByAdmin;
using AuthService.Application.Auths.Commands.RemoveRoles;
using AuthService.Application.Auths.Commands.UnlockAccByAdmin;
using AuthService.Application.Auths.Commands.UpdateRole;
using AuthService.Application.Auths.Queries.GetRole;
using AuthService.Application.Auths.Queries.GetRoles;
using AuthService.Application.Auths.Queries.GetUser;
using AuthService.Application.Auths.Queries.GetUsers;
using AuthService.Application.DTOs.Auth.Requests;
using AuthService.Application.DTOs.Roles.Requests;
using AuthService.Application.DTOs.Users.Requests;
using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination;
using BuildingBlocks.Pagination.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[Authorize(Roles = "ADMIN")]
[ApiController]
[Route("api/auth/admin")]
public class AdminController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public AdminController(IMapper mapper, ISender sender)
    {
        _mapper = mapper;
        _sender = sender;
    }

    [HttpGet("is-authorized")]
    public IActionResult IsAuthorized()
    {
        var isAuthorized = User.Identity?.IsAuthenticated ?? false;
        return Ok(new ResponseDto(IsSuccess: true, Data: isAuthorized,
            Message: isAuthorized ? "Admin đã được xác thực" : "Admin chưa được xác thực"));
    }

    [HttpGet("get-account")]
    public async Task<IActionResult> GetUser(string id)
    {
        var result = await _sender.Send(new GetUserQuery(id));
        var response = new ResponseDto(result, IsSuccess: (bool)(result != null),
            Message: result != null ? "Lấy thông tin người dùng thành công" : "Lấy thông tin người dùng thất bại");
        return Ok(response);
    }

    [HttpGet("get-accounts")]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationRequest request)
    {
        var result = await _sender.Send(new GetUsersQuery(request));
        var response = new ResponseDto(Data: result.PaginatedResult, Message: "Lấy danh sách người dùng thành công");
        return Ok(response);
    }

    [HttpPatch("update-account")]
    public async Task<IActionResult> UpdateUser(EditInfoUserRequest req)
    {
        var command = _mapper.Map<EditInForUserCommand>(req);
        var result = await _sender.Send(command);

        var response = new ResponseDto(result, IsSuccess: !string.IsNullOrEmpty(result.UserId),
            Message: !string.IsNullOrEmpty(result.UserId)
                ? "Cập nhật thông tin thành công"
                : "Cập nhật thông tin thất bại");

        return Ok(response);
    }

    [HttpPost("lock-account")]
    public async Task<IActionResult> LockAccount(Guid id)
    {
        var result = await _sender.Send(new LockAccByAdminCommand(id));
        var lockAccountResult = _mapper.Map<AuthLockAccountResult>(result);
        var response = new ResponseDto(lockAccountResult, Message: "Khóa tài khoản thành công");
        return Ok(response);
    }

    [HttpPost("unlock-account")]
    public async Task<IActionResult> UnlockAccount(Guid id)
    {
        var result = await _sender.Send(new UnlockAccByAdminCommand(id));
        var lockAccountResult = _mapper.Map<AuthLockAccountResult>(result);
        var response = new ResponseDto(lockAccountResult, Message: "Mở khóa tài khoản thành công");
        return Ok(response);
    }

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount(DeleteUserRequestDto req)
    {
        var requestModel = _mapper.Map<AuthDeleteAccountCommand>(req);
        var result = await _sender.Send(requestModel);
        var deleteAccountResult = _mapper.Map<AuthDeleteAccountResult>(result);
        var response = new ResponseDto(deleteAccountResult, Message: "Delete Account Successful");
        return Ok(response);
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

    [HttpPost("remove-role")]
    public async Task<IActionResult> RemoveRole(AssignRoleRequestDto req)
    {
        var command = _mapper.Map<RemoveRolesCommand>(req);
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
    public async Task<IActionResult> GetRoles([FromQuery] PaginationRequest req)
    {
        var result = await _sender.Send(new GetRolesQuery(req.PageIndex, req.PageSize));
        var response = new ResponseDto(Data: result, Message: "Get Roles Successful");
        return Ok(response);
    }

    [HttpGet("get-role")]
    public async Task<IActionResult> GetRoles([FromQuery] string id)
    {
        var result = await _sender.Send(new GetRoleQuery(id));
        var response = new ResponseDto(Data: result, Message: "Get Role Successful");
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