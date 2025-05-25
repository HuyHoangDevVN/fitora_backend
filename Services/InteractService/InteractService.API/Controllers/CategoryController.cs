using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using InteractService.Application.DTOs.Category.Requests;
using InteractService.Application.Usecases.Category.Commands.CreateCategory;
using InteractService.Application.Usecases.Category.Commands.FollowCategory;
using InteractService.Application.Usecases.Category.Commands.UnfollowCategory;
using InteractService.Application.Usecases.Category.Queries.GetCategories;
using InteractService.Application.Usecases.Category.Queries.GetCategoriesFollowed;
using InteractService.Application.Usecases.Category.Queries.GetCategory;
using InteractService.Application.Usecases.Category.Queries.GetTrendingCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

[Route("api/interact/category")]
[ApiController]
public class CategoryController : Controller
{
    private readonly IMediator _mediator;
    private readonly IAuthorizeExtension _authorizeExtension;

    public CategoryController(IMediator mediator, IAuthorizeExtension authorizeExtension)
    {
        _mediator = mediator;
        _authorizeExtension = authorizeExtension;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryFromBody request)
    {
        var userGuid = GetCurrentUserId();
        var command = new CreateCategoryCommand(
            new CreateCategoryRequest(
                request.Name,
                request.Slug,
                request.Description,
                request.ParentId,
                userGuid
            )
        );

        var response = await _mediator.Send(command);
        return Ok(response);
    }
    
    [HttpPost("follow")]
    public async Task<IActionResult> Follow([FromBody] Guid id)
    {
        var userGuid = GetCurrentUserId();
        var response = await _mediator.Send(new FollowCategoryCommand(new FollowCategoryRequest(userGuid, id)));
        return Ok(response);
    }

    [HttpPost("unfollow")]
    public async Task<IActionResult> Unfollow([FromBody] Guid id)
    {
        var userGuid = GetCurrentUserId();
        var response = await _mediator.Send(new UnfollowCategoryCommand(new FollowCategoryRequest(userGuid, id)));
        return Ok(response);
    }


    [HttpGet("get-list")]
    public async Task<IActionResult> GetList([FromQuery] GetCategoriesRequest request)
    {
        var response = await _mediator.Send(new GetCategoriesQuery(request));
        return Ok(response);
    }

    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        var response = await _mediator.Send(new GetCategoryQuery(id));
        return Ok(response);
    }

    
    [HttpGet("get-followed")]
    public async Task<IActionResult> GetFollowed([FromQuery] string? keySearch)
    {
        var userGuid = GetCurrentUserId();
        var response =
            await _mediator.Send(new GetCategoriesFollowedQuery(new GetCategoriesFollowedRequest(userGuid, keySearch)));
        return Ok(response);
    }

    [HttpGet("get-trending")]
    public async Task<IActionResult> GetTrending([FromQuery] GetTrendingCategoriesRequest request)
    {
        var response = await _mediator.Send(new GetTrendingCategoriesQuery(request));
        return Ok(new ResponseDto(response));
    }

    private Guid GetCurrentUserId()
    {
        return _authorizeExtension.GetUserFromClaimToken().Id;
    }
}