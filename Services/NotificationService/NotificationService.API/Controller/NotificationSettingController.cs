using BuildingBlocks.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs.NotificationSetting.Requests;
using NotificationService.Application.Services.IServices;

namespace NotificationService.API.Controller
{
    [ApiController]
    [Route("api/noti/notification-settings")]
    [Authorize]
    public class NotificationSettingController : ControllerBase
    {
        private readonly INotificationSettingRepository _settingRepo;
        private readonly IAuthorizeExtension _auth;

        public NotificationSettingController(INotificationSettingRepository settingRepo, IAuthorizeExtension auth)
        {
            _settingRepo = settingRepo;
            _auth = auth;
        }

        // GET /api/noti/notification-settings  -> current user's settings
        [HttpGet]
        public async Task<IActionResult> GetMySettings()
        {
            var userId = _auth.GetUserFromClaimToken().Id;
            var result = await _settingRepo.GetSettingsAsync(userId);
            return Ok(result);
        }

        [HttpGet("{userId:Guid}")]
        public async Task<IActionResult> GetSettings(Guid userId)
        {
            var callerId = _auth.GetUserFromClaimToken().Id;
            if (userId != callerId) return Forbid();
            var result = await _settingRepo.GetSettingsAsync(userId);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSetting([FromBody] UpdateNotificationSettingRequest request)
        {
            var currentUserId = _auth.GetUserFromClaimToken().Id;
            // Enforce: user can only update their own settings
            var req = request.UserId == Guid.Empty || request.UserId != currentUserId
                ? request with { UserId = currentUserId }
                : request;
            var success = await _settingRepo.UpdateSettingAsync(req);
            // If setting didn't exist, try bulk upsert path
            if (!success)
            {
                success = await _settingRepo.UpdateSettingsAsync(currentUserId, new[] { req });
            }
            return success ? Ok() : BadRequest();
        }

        // PUT /api/noti/notification-settings/bulk  -> update multiple at once
        [HttpPut("bulk")]
        public async Task<IActionResult> UpdateBulk([FromBody] IEnumerable<UpdateNotificationSettingRequest> requests)
        {
            var currentUserId = _auth.GetUserFromClaimToken().Id;
            var normalized = requests.Select(r => r.UserId == Guid.Empty || r.UserId != currentUserId ? r with { UserId = currentUserId } : r);
            var success = await _settingRepo.UpdateSettingsAsync(currentUserId, normalized);
            return success ? Ok() : BadRequest();
        }
    }
}