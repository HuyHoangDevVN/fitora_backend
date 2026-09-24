using AuthService.Application.Auths.Commands.RevokeAllOtherKeys;
using AuthService.Application.Auths.Commands.RevokeKey;
using AuthService.Application.Auths.Queries.GetKeys;
using AuthService.Application.DTOs.Key.Responses;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[Route("api/auth/key")]
[ApiController]
public class KeyController : Controller
{
    private readonly ISender _sender;
    private readonly IAuthorizeExtension _authorizeExtension;

    public KeyController(ISender sender, IAuthorizeExtension authorizeExtension)
    {
        _sender = sender;
        _authorizeExtension = authorizeExtension;
    }

    [HttpGet("get-keys")]
    public async Task<IActionResult> GetKeys([FromQuery] PaginationRequest req)
    {
        var userId = _authorizeExtension.DecodeToken().Id.ToString();
        var result = await _sender.Send(new GetKeysQuery(userId, req));
        var response = new GetKeysResponse(MetaData: result.PaginatedResult, Message: "Get Keys Successful");
        return Ok(response);
    }

    [HttpDelete("revoke/{keyId:guid}")]
    public async Task<IActionResult> RevokeKey([FromRoute] Guid keyId)
    {
        var result = await _sender.Send(new RevokeKeyCommand(keyId));
        return Ok(new ResponseDto(result, IsSuccess: result.IsSuccess, Message: result.Message));
    }

    [HttpPost("revoke-all-except-current")]
    public async Task<IActionResult> RevokeAllExceptCurrent()
    {
        var result = await _sender.Send(new RevokeAllOtherKeysCommand());
        return Ok(new ResponseDto(result, Message: result.Message));
    }
}
