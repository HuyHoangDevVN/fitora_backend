using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using InteractService.Application.DTOs.Report.Requests;
using InteractService.Application.Usecases.Report.Commands.CreateReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

[Route("api/interact/report")]
[ApiController]
public class ReportController : Controller
{
    private readonly IMediator _mediator;
    private readonly IAuthorizeExtension _authorizeExtension;

    public ReportController(IMediator mediator, IAuthorizeExtension authorizeExtension)
    {
        _mediator = mediator;
        _authorizeExtension = authorizeExtension;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateReportFormBody req)
    {
        var userGuid = _authorizeExtension.GetUserFromClaimToken().Id;
        var report = await _mediator.Send(new CreateReportCommand(new CreateReportRequest(
            userGuid, req.TargetType, req.TargetId, req.Reason
        )));
        var response = new ResponseDto(report, Message: "Create Successful");
        return Ok(response);
    }
}