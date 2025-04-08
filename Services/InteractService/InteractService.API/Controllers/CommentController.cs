using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using InteractService.Application.DTOs.Comment.Requests;
using InteractService.Application.Usecases.Comments.Commands.CreateComment;
using InteractService.Application.Usecases.Comments.Commands.DeleteComment;
using InteractService.Application.Usecases.Comments.Commands.UpdateComment;
using InteractService.Application.Usecases.Comments.Commands.VoteComment;
using InteractService.Application.Usecases.Comments.Queries.GetCommentReplies;
using InteractService.Application.Usecases.Comments.Queries.GetCommentsByPost;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

[Route("api/comment")]
[ApiController]
public class CommentController : Controller
{
    private readonly IMediator _mediator;
    private readonly IAuthorizeExtension _authorizeExtension;

    public CommentController(IMediator mediator, IAuthorizeExtension authorizeExtension)
    {
        _mediator = mediator;
        _authorizeExtension = authorizeExtension;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentFormBody formBody)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new CreateCommentCommand(new CreateCommentRequest(
            userId, formBody.PostId, formBody.ParentCommentId, formBody.Content, formBody.MediaUrl
        )));
        return Ok(result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentRequest request)
    {
        var result = await _mediator.Send(new UpdateCommentCommand(request));
        return Ok(result);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var result = await _mediator.Send(new DeleteCommentCommand(id));
        return Ok(result);
    }

    [HttpPut("vote")]
    public async Task<IActionResult> Vote([FromBody] VoteCommentRequest request)
    {
        var userGuid = request.UserId != Guid.Empty ? request.UserId : _authorizeExtension.GetUserFromClaimToken().Id;
        var response =
            await _mediator.Send(
                new VoteCommentCommand(new VoteCommentRequest(userGuid, request.CommentId, request.VoteType)));
        return Ok(response);
    }

    [HttpGet("get-by-post")]
    public async Task<IActionResult> GetCommentsByPostId([FromQuery] GetPostCommentsRequest request)
    {
        var comments = await _mediator.Send(new GetCommentsByPostQuery(request));
        return Ok(new ResponseDto(
            comments, IsSuccess: true, "Get Successful"));
    }

    [HttpGet("get-replies")]
    public async Task<IActionResult> GetCommentsReplies([FromQuery] GetCommentRepliesRequest request)
    {
        var comments = await _mediator.Send(new GetCommentRepliesQuery(request));
        return Ok(new ResponseDto(
            comments, IsSuccess: true, "Get Successful"));
    }

    private Guid GetCurrentUserId()
    {
        return _authorizeExtension.GetUserFromClaimToken().Id;
    }
}