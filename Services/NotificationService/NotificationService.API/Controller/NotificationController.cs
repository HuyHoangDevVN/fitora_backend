using BuildingBlocks.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs.Notification.Requests;
using NotificationService.Application.Services.IServices;
using NotificationService.Domain.Models;

namespace NotificationService.API.Controller
{
    [ApiController]
    [Route("api/noti/notification")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuthorizeExtension _authorizeExtension;
        private readonly INotificationSettingRepository _settingRepo;
        private readonly IBlockedIdsProvider _blockedIdsProvider;

        public NotificationController(
            INotificationRepository notificationRepository,
            IAuthorizeExtension authorizeExtension,
            INotificationSettingRepository settingRepo,
            IBlockedIdsProvider blockedIdsProvider)
        {
            _notificationRepository = notificationRepository;
            _authorizeExtension = authorizeExtension;
            _settingRepo = settingRepo;
            _blockedIdsProvider = blockedIdsProvider;
        }

        [HttpGet("get-notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] GetNotificationsRequest request)
        {
            var result = await _notificationRepository.GetNotificationsAsync(request);
            var filtered = await FilterBlockedAsync(request.UserId, result);
            return Ok(filtered);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isRead = null,
            [FromQuery] int? notificationTypeId = null)
        {
            var userId = _authorizeExtension.GetUserFromClaimToken().Id;
            var req = new GetNotificationsRequest(userId, isRead, notificationTypeId)
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
            };
            var result = await _notificationRepository.GetNotificationsAsync(req);
            var filtered = await FilterBlockedAsync(userId, result);
            return Ok(filtered);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications([FromQuery] GetUnreadNotificationsRequest request)
        {
            var result = await _notificationRepository.GetUnreadNotificationsAsync(request);
            var filtered = await FilterBlockedAsync(request.UserId, result);
            return Ok(filtered);
        }

        [HttpGet("get-notification")]
        public async Task<IActionResult> GetNotification([FromQuery] long id)
        {
            var result = await _notificationRepository.GetNotificationByIdAsync(id);
            if (result == null)
                return NotFound();
            if (result.SenderId.HasValue)
            {
                try
                {
                    var currentUserId = _authorizeExtension.GetUserFromClaimToken().Id;
                    var blocked = await _blockedIdsProvider.GetBlockedUserIdsAsync(currentUserId);
                    if (blocked.Contains(result.SenderId.Value))
                        return NotFound();
                }
                catch { }
            }
            return Ok(result);
        }

        [HttpPost("create-notification")]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            var enabled = await _settingRepo.IsEnabledAsync(request.UserId, request.NotificationTypeId);
            if (!enabled)
                return Ok(new { skipped = true, reason = "recipient disabled this notification type" });

            if (request.SenderId.HasValue)
            {
                try
                {
                    var blocked = await _blockedIdsProvider.GetBlockedUserIdsAsync(request.UserId);
                    if (blocked.Contains(request.SenderId.Value))
                        return Ok(new { skipped = true, reason = "recipient has blocked sender" });
                }
                catch { }
            }

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

        [HttpPut("{id:long}/read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            var userId = _authorizeExtension.GetUserFromClaimToken().Id;
            var dto = await _notificationRepository.GetNotificationByIdAsync(id);
            if (dto == null) return NotFound();
            if (dto.UserId != userId) return Forbid();
            if (dto.IsRead) return Ok();
            var entity = new Notification
            {
                Id = id,
                IsRead = true,
                IsDelivered = dto.IsDelivered,
                Content = dto.Content,
                UserId = dto.UserId,
                SenderId = dto.SenderId,
                NotificationTypeId = dto.NotificationTypeId,
                ObjectId = dto.ObjectId,
                Channel = dto.Channel,
            };
            var ok = await _notificationRepository.UpdateAsync(entity);
            return ok ? Ok() : BadRequest();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _authorizeExtension.GetUserFromClaimToken().Id;
            await _notificationRepository.MarkAllAsReadAsync(userId);
            return Ok();
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteNotification(long id)
        {
            var userId = _authorizeExtension.GetUserFromClaimToken().Id;
            var dto = await _notificationRepository.GetNotificationByIdAsync(id);
            if (dto == null) return NotFound();
            if (dto.UserId != userId) return Forbid();
            var deleted = await _notificationRepository.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearAll()
        {
            var userId = _authorizeExtension.GetUserFromClaimToken().Id;
            await _notificationRepository.DeleteAllAsync(userId);
            return Ok();
        }

        private async Task<BuildingBlocks.Pagination.Base.PaginatedResult<Notification>> FilterBlockedAsync(
            Guid currentUserId,
            BuildingBlocks.Pagination.Base.PaginatedResult<Notification> result)
        {
            if (!result.Data.Any()) return result;
            var blocked = await _blockedIdsProvider.GetBlockedUserIdsAsync(currentUserId);
            if (blocked.Count == 0) return result;
            var filtered = result.Data.Where(n => n.SenderId == null || !blocked.Contains(n.SenderId.Value)).ToList();
            var removed = result.Data.Count() - filtered.Count;
            return new BuildingBlocks.Pagination.Base.PaginatedResult<Notification>(
                result.PageIndex, result.PageSize, Math.Max(0, result.Count - removed), filtered);
        }
    }
}
