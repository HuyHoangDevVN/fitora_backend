using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using InteractService.Application.DTOs.Post.Requests;
using InteractService.Application.DTOs.Post.Responses;
using InteractService.Application.Usecases.Posts.Commands.CreatePost;
using InteractService.Application.Usecases.Posts.Commands.DeletePost;
using InteractService.Application.Usecases.Posts.Commands.UpdatePost;
using InteractService.Application.Usecases.Posts.Queries.GetAllPost;
using InteractService.Application.Usecases.Posts.Queries.GetByIdPost;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

public record CreatePostFromBody(string Content, string MediaUrl, int Privacy, Guid? GroupId = null);

[Route("api/post")]
[ApiController]
public class PostController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IAuthorizeExtension _authorizeExtension;


    public PostController(ISender sender, IMapper mapper, IMediator mediator, IAuthorizeExtension authorizeExtension)
    {
        _sender = sender;
        _mapper = mapper;
        _mediator = mediator;
        _authorizeExtension = authorizeExtension;
    }

    [HttpPost("create-post")]
    public async Task<IActionResult> Create([FromBody] CreatePostFromBody createPostFromBody)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;
        if (userGuid == Guid.Empty)
        {
            return Unauthorized();
        }

        var post = await _mediator.Send(new CreatePostCommand(new CreatePostRequest(
            userGuid, createPostFromBody.Content, createPostFromBody.MediaUrl, createPostFromBody.Privacy,
            createPostFromBody.GroupId
        )));
        var response = new ResponseDto(post, Message: "Create Successful");
        return Ok(response);
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var posts = await _mediator.Send(new GetAllPostQuery());
        if (posts is null)
        {
            return NoContent();
        }

        var results = posts.Select(post => _mapper.Map<PostResponseDto>(post));
        var response = new ResponseDto(results, Message: "Get All Successful");
        return Ok(response);
    }

    [HttpGet("get-by-id/{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var post = await _mediator.Send(new GetPostByIdQuery(id));
        if (post is null)
        {
            return NotFound(new { Message = "Post not found" });
        }

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
        var response = new ResponseDto(Message: "Update Successful");
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
}