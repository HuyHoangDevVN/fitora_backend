using BuildingBlocks.Pagination.Base;
using NotificationService.Application.DTOs.NotificationType.Requests;

namespace NotificationService.Application.Services.IServices;

public interface INotificationTypeRepository
{
    Task<bool> CreateAsync(NotificationType notificationType);
    Task<NotificationType> GetByIdAsync(int id);
    Task<PaginatedResult<NotificationType>> GetListAsync(GetListNotificationTypesRequest request);
    Task<bool> UpdateAsync(NotificationType notificationType);
    Task<bool> DeleteAsync(int id);
}