using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using InteractService.Application.DTOs.Post.Requests;
using InteractService.Application.Usecases.Posts.Commands.CreatePost;
using InteractService.Application.Usecases.Posts.Commands.DeletePost;
using InteractService.Application.Usecases.Posts.Commands.SavePost;
using InteractService.Application.Usecases.Posts.Commands.UnsavePost;
using InteractService.Application.Usecases.Posts.Commands.UpdatePost;
using InteractService.Application.Usecases.Posts.Commands.VotePost;
using InteractService.Application.Usecases.Posts.Queries.GetByIdPost;
using InteractService.Application.Usecases.Posts.Queries.GetExploreFeed;
using InteractService.Application.Usecases.Posts.Queries.GetNewfeed;
using InteractService.Application.Usecases.Posts.Queries.GetPersonal;
using InteractService.Application.Usecases.Posts.Queries.GetSavedPosts;
using InteractService.Application.Usecases.Posts.Queries.GetTrending;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

[Route("api/post")]
[ApiController]
public class PostController : Controller
{
    private readonly IMediator _mediator;
    private readonly IAuthorizeExtension _authorizeExtension;

    public PostController(IMediator mediator, IAuthorizeExtension authorizeExtension)
    {
        _mediator = mediator;
        _authorizeExtension = authorizeExtension;
    }

    [HttpPost("create-post")]
    public async Task<IActionResult> Create([FromBody] CreatePostFromBody req)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;
        var post = await _mediator.Send(new CreatePostCommand(new CreatePostRequest(
            userGuid, req.Content, req.MediaUrl, req.Privacy,
            req.GroupId, req.CategoryId
        )));
        var response = new ResponseDto(post, Message: "Create Successful");
        return Ok(response);
    }

    [HttpGet("get-by-id/{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var post = await _mediator.Send(new GetPostByIdQuery(id));
        var response = new ResponseDto(post, Message: "Get By Id Successful");
        return Ok(response);
    }

    [HttpPut("update-post/{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePostRequest updatePostRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedPost = await _mediator.Send(new UpdatePostCommand(id, updatePostRequest));
        return Ok(updatedPost);
    }

    [HttpPut("vote")]
    public async Task<IActionResult> Vote([FromBody] VotePostRequest request)
    {
        var userGuid = request.UserId != Guid.Empty ? request.UserId : _authorizeExtension.GetUserFromClaimToken().Id;
        var response =
            await _mediator.Send(new VotePostCommand(new VotePostRequest(userGuid, request.PostId, request.VoteType)));
        return Ok(response);
    }

    [HttpPost("save-post")]
    public async Task<IActionResult> SavePost([FromBody] SavePostRequest request)
    {
        var userGuid = request.UserId != Guid.Empty ? request.UserId : _authorizeExtension.GetUserFromClaimToken().Id;
        var response = await _mediator.Send(new SavePostCommand(new SavePostRequest(userGuid, request.PostId)));
        return Ok(response);
    }

    [HttpDelete("unsave-post")]
    public async Task<IActionResult> UnSavePost([FromQuery] Guid postId)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;
        var response = await _mediator.Send(new UnSavePostCommand(new SavePostRequest(userGuid, postId)));
        return Ok(response);
    }

    [HttpDelete("delete-post/{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var post = await _mediator.Send(new DeletePostCommand(id));
        if (!post)
        {
            return BadRequest(new { Message = "Failed to delete post." });
        }

        var response = new ResponseDto(Message: "Delete Successful");
        return Ok(response);
    }

    [HttpGet("newfeed")]
    public async Task<IActionResult> GetNewFeed([FromQuery] GetPostRequest request)
    {
        var userGuid = request.Id != Guid.Empty ? request.Id : _authorizeExtension.GetUserFromClaimToken().Id;

        var post = await _mediator.Send(
            new GetNewfeedQuery(new GetPostRequest(userGuid, request.FeedType, request.CategoryId, request.Cursor,
                request.Limit, request.GroupId))
        );

        var response = new ResponseDto(post, IsSuccess: true, "Get Successful");
        return Ok(response);
    }

    [HttpGet("trending-feed")]
    public async Task<IActionResult> GetTrendingFeed([FromQuery] GetTrendingPostRequest request)
    {
        var userGuid = request.Id != Guid.Empty ? request.Id : _authorizeExtension.GetUserFromClaimToken().Id;

        var post = await _mediator.Send(
            new GetTrendingQuery(new GetTrendingPostRequest(userGuid, request.Cursor,
                request.Limit, request.GroupId))
        );

        var response = new ResponseDto(post, IsSuccess: true, "Get Successful");
        return Ok(response);
    }

    [HttpGet("explore-feed")]
    public async Task<IActionResult> GetExploreFeed([FromQuery] GetExplorePostRequest request)
    {
        var userGuid = request.Id != Guid.Empty ? request.Id : _authorizeExtension.GetUserFromClaimToken().Id;

        var post = await _mediator.Send(
            new GetExploreFeedQuery(new GetExplorePostRequest(userGuid, request.Cursor,
                request.Limit))
        );

        var response = new ResponseDto(post, IsSuccess: true, "Get Successful");
        return Ok(response);
    }

    [HttpGet("saved-posts")]
    public async Task<IActionResult> GetSavedPosts([FromQuery] GetSavedPostsRequest request)
    {
        var userGuid = request.Id != Guid.Empty ? request.Id : _authorizeExtension.GetUserFromClaimToken().Id;

        var post = await _mediator.Send(
            new GetSavedPostsQuery(new GetSavedPostsRequest(userGuid, request.Cursor,
                request.Limit, request.GroupId))
        );

        var response = new ResponseDto(post, IsSuccess: true, "Get Successful");
        return Ok(response);
    }

    [HttpGet("personal")]
    public async Task<IActionResult> GetPersonal([FromQuery] GetPostRequest request)
    {
        var userGuid = request.Id != Guid.Empty ? request.Id : _authorizeExtension.GetUserFromClaimToken().Id;

        var post = await _mediator.Send(
            new GetPersonalQuery(new GetPostRequest(userGuid, request.FeedType, request.CategoryId, request.Cursor,
                request.Limit, request.GroupId))
        );

        var response = new ResponseDto(post, IsSuccess: true, "Get Successful");
        return Ok(response);
    }
}