using BuildingBlocks.Pagination.Base;
using NotificationService.Application.DTOs.Notification.Requests;
using NotificationService.Application.DTOs.Notification.Responses;

namespace NotificationService.Application.Services.IServices;

public interface INotificationRepository
{
    Task<bool> CreateAsync(Notification notification);
    Task<bool> UpdateAsync(Notification notification);
    Task<bool> DeleteAsync(long id);
    Task<PaginatedResult<Notification>> GetNotificationsAsync(GetNotificationsRequest request);
    Task<PaginatedResult<Notification>> GetUnreadNotificationsAsync(GetUnreadNotificationsRequest request);
    Task<NotificationDto?> GetNotificationByIdAsync(long id);
    Task<NotificationDto> CreateAndReturnAsync(CreateNotificationRequest request);
    Task MarkAllAsReadAsync(Guid userId);
}