using NotificationService.Application.DTOs.NotificationSetting.Requests;
using NotificationService.Application.DTOs.NotificationSetting.Responses;
using NotificationService.Application.Services.IServices;
using BuildingBlocks.RepositoryBase.EntityFramework;
using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Models;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories
{
    public class NotificationSettingRepository : INotificationSettingRepository
    {
        private readonly IRepositoryBase<NotificationSetting> _settingRepo;
        private readonly DbSet<NotificationSetting> _settingDbSet;

        public NotificationSettingRepository(IRepositoryBase<NotificationSetting> settingRepo, ApplicationDbContext dbContext)
        {
            _settingRepo = settingRepo;
            _settingDbSet = dbContext.Set<NotificationSetting>();
        }

        public async Task<IEnumerable<NotificationSettingDto>> GetSettingsAsync(Guid userId)
        {
            var entities = await _settingRepo.FindAsync(s => s.UserId == userId);
            return entities.Select(s => new NotificationSettingDto
            {
                Id = s.Id,
                UserId = s.UserId,
                NotificationTypeId = s.NotificationTypeId,
                TypeCode = s.NotificationType.Code,
                IsEnabled = s.IsEnabled,
            });
        }

        public async Task<bool> UpdateSettingAsync(UpdateNotificationSettingRequest request)
        {
            var setting = await _settingRepo.GetAsync(s => s.UserId == request.UserId && s.NotificationTypeId == request.NotificationTypeId);
            if (setting == null)
                return false;
            setting.IsEnabled = request.IsEnabled;
            await _settingRepo.UpdateAsync(s => s.Id == setting.Id, setting);
            return await _settingRepo.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateSettingsAsync(Guid userId, IEnumerable<UpdateNotificationSettingRequest> requests)
        {
            bool result = true;
            foreach (var req in requests)
            {
                var setting = await _settingRepo.GetAsync(s => s.UserId == userId && s.NotificationTypeId == req.NotificationTypeId);
                if (setting != null)
                {
                    setting.IsEnabled = req.IsEnabled;
                    await _settingRepo.UpdateAsync(s => s.Id == setting.Id, setting);
                }
                else
                {
                    var newSetting = new NotificationSetting
                    {
                        UserId = userId,
                        NotificationTypeId = req.NotificationTypeId,
                        IsEnabled = req.IsEnabled,
                    };
                    await _settingRepo.AddAsync(newSetting);
                }
            }
            return await _settingRepo.SaveChangesAsync() > 0 && result;
        }
    }
}