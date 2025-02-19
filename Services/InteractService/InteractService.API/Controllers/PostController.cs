using AutoMapper;
using BuildingBlocks.DTOs;
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

[Microsoft.AspNetCore.Components.Route("api/v{v}/Post")]
[ApiController]
public class PostController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public PostController(ISender sender, IMapper mapper, IMediator mediator)
    {
        _sender = sender;
        _mapper = mapper;
        _mediator = mediator;
    }

    [HttpPost("create-post")]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest createPostRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var post = await _mediator.Send(new CreatePostCommand(createPostRequest));
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
            return NotFound(new {Message = "Post not found"});
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