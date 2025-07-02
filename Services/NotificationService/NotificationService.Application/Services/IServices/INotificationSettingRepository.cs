using NotificationService.Application.DTOs.NotificationSetting.Requests;
using NotificationService.Application.DTOs.NotificationSetting.Responses;

namespace NotificationService.Application.Services.IServices;

public interface INotificationSettingRepository
{
    Task<IEnumerable<NotificationSettingDto>> GetSettingsAsync(Guid userId);

    Task<bool> UpdateSettingAsync(UpdateNotificationSettingRequest request);

    Task<bool> UpdateSettingsAsync(Guid userId, IEnumerable<UpdateNotificationSettingRequest> requests);
}