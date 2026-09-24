using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Search.Requests;
using UserService.Application.Usecases.RecentSearch.Commands.AddRecentSearch;
using UserService.Application.Usecases.RecentSearch.Commands.ClearRecentSearches;
using UserService.Application.Usecases.RecentSearch.Commands.DeleteRecentSearch;
using UserService.Application.Usecases.RecentSearch.Queries.GetRecentSearches;

namespace UserService.API.Controller;

[Route("api/user/search")]
[ApiController]
[Authorize]
public class SearchController(ISender sender) : ControllerBase
{
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent()
    {
        var result = await sender.Send(new GetRecentSearchesQuery());
        return Ok(result);
    }

    [HttpPost("recent")]
    public async Task<IActionResult> AddRecent([FromBody] AddRecentSearchRequest request)
    {
        var result = await sender.Send(new AddRecentSearchCommand(request.Query));
        return Ok(result);
    }

    [HttpDelete("recent/{id:guid}")]
    public async Task<IActionResult> DeleteRecent(Guid id)
    {
        var result = await sender.Send(new DeleteRecentSearchCommand(id));
        return Ok(result);
    }

    [HttpDelete("recent")]
    public async Task<IActionResult> ClearRecent()
    {
        var result = await sender.Send(new ClearRecentSearchesCommand());
        return Ok(result);
    }
}
