using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs.Notification.Requests;
using NotificationService.Application.Services.IServices;
using NotificationService.Domain.Models;

namespace NotificationService.API.Controller
{
    [ApiController]
    [Route("api/notification")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet("get-notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] GetNotificationsRequest request)
        {
            var result = await _notificationRepository.GetNotificationsAsync(request);
            return Ok(result);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications([FromQuery] GetUnreadNotificationsRequest request)
        {
            var result = await _notificationRepository.GetUnreadNotificationsAsync(request);
            return Ok(result);
        }

        [HttpGet("get-notification")]
        public async Task<IActionResult> GetNotification([FromQuery] long id)
        {
            var result = await _notificationRepository.GetNotificationByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("create-notification")]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            var notification = new Notification
            {
                UserId = request.UserId,
                SenderId = request.SenderId,
                NotificationTypeId = request.NotificationTypeId,
                ObjectId = request.ObjectId,
                Content = request.Content,
                Channel = request.Channel,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                IsDelivered = false
            };

            var created = await _notificationRepository.CreateAsync(notification);
            return created ? Ok() : BadRequest();
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateNotification(long id, [FromBody] UpdateNotificationRequest request)
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification == null)
                return NotFound();

            var updatedNotification = new Notification
            {
                Id = id,
                IsRead = request.IsRead ?? notification.IsRead,
                IsDelivered = request.IsDelivered ?? notification.IsDelivered,
                Content = request.Content ?? notification.Content,
                UserId = notification.UserId,
                SenderId = notification.SenderId,
                NotificationTypeId = notification.NotificationTypeId,
                ObjectId = notification.ObjectId,
                Channel = notification.Channel,
            };

            var updated = await _notificationRepository.UpdateAsync(updatedNotification);
            return updated ? Ok() : BadRequest();
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteNotification(long id)
        {
            var deleted = await _notificationRepository.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
        }
    }
}