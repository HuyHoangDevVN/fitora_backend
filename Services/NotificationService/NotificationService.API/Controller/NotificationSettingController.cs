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

        public NotificationSettingController(INotificationSettingRepository settingRepo)
        {
            _settingRepo = settingRepo;
        }

        [HttpGet("{userId:Guid}")]
        public async Task<IActionResult> GetSettings(Guid userId)
        {
            var result = await _settingRepo.GetSettingsAsync(userId);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSetting([FromBody] UpdateNotificationSettingRequest request)
        {
            var success = await _settingRepo.UpdateSettingAsync(request);
            return success ? Ok() : BadRequest();
        }
    }
}