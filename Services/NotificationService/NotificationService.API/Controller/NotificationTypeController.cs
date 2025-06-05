using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs.NotificationType.Requests;
using NotificationService.Application.Usecases.NotificationType.Commands.Create;
using NotificationService.Application.Usecases.NotificationType.Commands.Delete;
using NotificationService.Application.Usecases.NotificationType.Queries.GetNotificationType;
using NotificationService.Application.Usecases.NotificationType.Queries.GetNotificationTypes;

namespace NotificationService.API.Controller
{
    [ApiController]
    [Route("api/noti-notification-type")]
    [Authorize]
    public class NotificationTypeController : ControllerBase
    {
        private readonly ISender _sender;

        public NotificationTypeController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetNotiType([FromQuery] int id)
        {
            var result = await _sender.Send(new GetNotiTypeByIdQuery(id));
            return Ok(result);
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetNotiTypes([FromQuery] GetListNotificationTypesRequest request)
        {
            var result = await _sender.Send(new GetNotiTypesQuery(request));
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateNotificationTypeRequest request)
        {
            var result = await _sender.Send(new CreateNotiTypeCommand(request));
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationTypeRequest request)
        {

            var result = await _sender.Send(new CreateNotiTypeCommand(request));
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteNotiType(int id)
        {
            var result = await _sender.Send(new DeleteNotiTypeCommand(id));
            return Ok(result);
        }

    }
}