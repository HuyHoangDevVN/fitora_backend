using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using InteractService.Application.DTOs.Category.Requests;
using InteractService.Application.DTOs.Comment.Requests;
using InteractService.Application.DTOs.Post.Requests;
using InteractService.Application.DTOs.Report.Requests;
using InteractService.Application.Usecases.Category.Commands.CreateCategory;
using InteractService.Application.Usecases.Category.Commands.DeleteCategory;
using InteractService.Application.Usecases.Category.Commands.UpdateCategory;
using InteractService.Application.Usecases.Category.Queries.GetCategories;
using InteractService.Application.Usecases.Category.Queries.GetCategory;
using InteractService.Application.Usecases.Comments.Commands.DeleteComment;
using InteractService.Application.Usecases.Comments.Queries.GetListComment;
using InteractService.Application.Usecases.Posts.Commands.DeletePost;
using InteractService.Application.Usecases.Posts.Queries.GetByIdPost;
using InteractService.Application.Usecases.Posts.Queries.GetListPost;
using InteractService.Application.Usecases.Report.Commands.DeleteReport;
using InteractService.Application.Usecases.Report.Commands.UpdateStatus;
using InteractService.Application.Usecases.Report.Queries.GetById;
using InteractService.Application.Usecases.Report.Queries.GetList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/interact/admin")]
public class AdminController : Controller
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;
    private readonly IAuthorizeExtension _authorizeExtension;

    public AdminController(IMapper mapper, ISender sender, IAuthorizeExtension authorizeExtension)
    {
        _mapper = mapper;
        _sender = sender;
        _authorizeExtension = authorizeExtension;
    }

    #region Report

    [HttpGet("get-report")]
    public async Task<IActionResult> GetReportAsync([FromQuery] Guid id)
    {
        var report = await _sender.Send(new GetReportQuery(id));
        return Ok(new ResponseDto(Data: report, Message: "Lấy báo cáo thành công !", IsSuccess: true));
    }

    [HttpGet("get-reports")]
    public async Task<IActionResult> GetReportsAsync([FromQuery] GetReportsRequest request)
    {
        var reports = await _sender.Send(new GetListReportQuery(request));
        return Ok(new ResponseDto(Data: reports, Message: "Lấy danh sách báo cáo thành công !", IsSuccess: true));
    }

    [HttpPost("update-report-status")]
    public async Task<IActionResult> UpdateReportStatusAsync([FromBody] UpdateStatusReportFormBody formBody)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var result =
            await _sender.Send(
                new UpdateStatusReportCommand(new UpdateStatusReportRequest(formBody.ReportId, formBody.Status,
                    userId)));
        return Ok(new ResponseDto(Data: result, Message: "Cập nhật trạng thái báo cáo thành công !", IsSuccess: true));
    }

    [HttpDelete("delete-report")]
    public async Task<IActionResult> DeleteReportAsync([FromQuery] Guid id)
    {
        var result = await _sender.Send(new DeleteReportCommand(id));
        return Ok(new ResponseDto(Data: result, Message: result ? "Xoá báo cáo thành công !" : "Xóa báo cáo thất bại !",
            IsSuccess: result));
    }

    #endregion

    #region Category

    [HttpPost("create-category")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryFromBody request)
    {
        var userId = _authorizeExtension.GetUserFromClaimToken().Id;
        var command = new CreateCategoryCommand(
            new CreateCategoryRequest(
                request.Name,
                request.Slug,
                request.Description,
                request.ParentId,
                userId
            )
        );

        var response = await _sender.Send(command);
        return Ok(response);
    }

    [HttpGet("get-categories")]
    public async Task<IActionResult> GetList([FromQuery] GetCategoriesRequest request)
    {
        var response = await _sender.Send(new GetCategoriesQuery(request));
        return Ok(new ResponseDto(response));
    }

    [HttpGet("get-category")]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        var response = await _sender.Send(new GetCategoryQuery(id));
        return Ok(new ResponseDto(response));
    }
    
    [HttpPut("update-category")]
    public async Task<IActionResult> UpdateCategoryAsync([FromBody] UpdateCategoryRequest request)
    {
        var command = new UpdateCategoryCommand(request);
        var result = await _sender.Send(command);
        return Ok(new ResponseDto(Data: result,
            Message: result ? "Cập nhật danh mục thành công !" : "Cập nhật danh mục thất bại !",
            IsSuccess: result));
    }

    [HttpDelete("delete-category")]
    public async Task<IActionResult> DeleteCategoryAsync([FromQuery] Guid id)
    {
        var result = await _sender.Send(new DeleteCategoryCommand(id));
        return Ok(new ResponseDto(Data: result,
            Message: result ? "Xoá danh mục thành công !" : "Xóa danh mục thất bại !",
            IsSuccess: result));
    }

    #endregion

    #region Post

    [HttpGet("get-post")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        var post = await _sender.Send(new GetPostByIdQuery(id));
        var response = new ResponseDto(post, Message: "Get By Id Successful");
        return Ok(response);
    }

    [HttpGet("get-posts")]
    public async Task<IActionResult> GetListPost([FromQuery] GetListPostRequest request)
    {
        var posts = await _sender.Send(new GetListPostQuery(request));
        return Ok(new ResponseDto(posts, Message: "Get List Post Successful"));
    }


    [HttpDelete("delete-post/{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var post = await _sender.Send(new DeletePostCommand(id));
        if (!post)
        {
            return BadRequest(new { Message = "Failed to delete post." });
        }

        var response = new ResponseDto(Message: "Delete Successful");
        return Ok(response);
    }

    #endregion


    #region Comment

    [HttpGet("get-comments")]
    public async Task<IActionResult> GetComments([FromQuery] GetListCommentRequest request)
    {
        var comments = await _sender.Send(new GetListCommentQuery(request));
        return Ok(new ResponseDto(comments, Message: "Get List Comment Successful"));
    }

    [HttpDelete("delete-comment")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var result = await _sender.Send(new DeleteCommentCommand(id));
        return Ok(result);
    }

    #endregion
}