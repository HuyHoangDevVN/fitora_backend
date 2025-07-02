using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using NotificationService.Application.DTOs.NotificationType.Requests;
using NotificationService.Application.Services.IServices;
using NotificationService.Domain.Models;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationTypeRepository : INotificationTypeRepository
{
    private readonly IRepositoryBase<NotificationType> _notificationTypeRepo;

    public NotificationTypeRepository(IRepositoryBase<NotificationType> notificationTypeRepo)
    {
        _notificationTypeRepo = notificationTypeRepo;
    }


    public async Task<bool> CreateAsync(NotificationType notificationType)
    {
        await _notificationTypeRepo.AddAsync(notificationType);
        return await _notificationTypeRepo.SaveChangesAsync() > 0;
    }

    public async Task<NotificationType> GetByIdAsync(int id)
    {
        return await _notificationTypeRepo.GetAsync(nt => nt.Id == id)
               ?? throw new KeyNotFoundException($"NotificationType with id {id} not found.");
        ;
    }

    public async Task<PaginatedResult<NotificationType>> GetListAsync(GetListNotificationTypesRequest request)
    {
        var results = await _notificationTypeRepo.GetPageAsync(request, CancellationToken.None,
            nt => string.IsNullOrEmpty(request.KeySearch) || nt.Name.Contains(request.KeySearch)
                                                          || nt.Code.Contains(request.KeySearch));
        var totalCount = results.Data.Count();
        return new PaginatedResult<NotificationType>(
            results.PageIndex, request.PageSize, totalCount, results.Data
        );
    }

    public async Task<bool> UpdateAsync(NotificationType notificationType)
    {
        await _notificationTypeRepo.UpdateAsync(nt => nt.Id == notificationType.Id, notificationType);
        return await _notificationTypeRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _notificationTypeRepo.DeleteAsync(nt => nt.Id == id);
        return await _notificationTypeRepo.SaveChangesAsync() > 0;
    }
}