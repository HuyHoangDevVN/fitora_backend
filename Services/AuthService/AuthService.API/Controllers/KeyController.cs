using AuthService.Application.Auths.Queries.GetKeys;
using AuthService.Application.DTOs.Key.Responses;
using AutoMapper;
using BuildingBlocks.Pagination;
using BuildingBlocks.Pagination.Base;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[Route("api/auth/key")]
[ApiController]
public class KeyController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public KeyController(IMapper mapper, ISender sender)
    {
        _mapper = mapper;
        _sender = sender;
    }

    [HttpGet("get-keys")]
    public async Task<IActionResult> GetKeys([FromQuery] PaginationRequest req)
    {
        var result = await _sender.Send(new GetKeysQuery(req));
        var response = new GetKeysResponse(MetaData: result.PaginatedResult, Message: "Get Keys Successful");
        return Ok(response);
    }
}