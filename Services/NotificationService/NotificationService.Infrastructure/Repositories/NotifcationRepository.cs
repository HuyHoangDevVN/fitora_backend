using AutoMapper;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.DTOs.Notification.Requests;
using NotificationService.Application.DTOs.Notification.Responses;
using NotificationService.Application.Services.IServices;
using NotificationService.Domain.Models;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IRepositoryBase<Notification> _notificationRepo;
    private readonly DbSet<Notification> _notificationDbSet;
    private readonly IMapper _mapper;

    public NotificationRepository(
        IRepositoryBase<Notification> notificationRepo,
        ApplicationDbContext dbContext,
        IMapper mapper)
    {
        _notificationRepo = notificationRepo;
        _notificationDbSet = dbContext.Set<Notification>();
        _mapper = mapper;
    }

    public async Task<bool> CreateAsync(Notification notification)
    {
        await _notificationRepo.AddAsync(notification);
        return await _notificationRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Notification notification)
    {
        await _notificationRepo.UpdateAsync(n => n.Id == notification.Id, notification);
        return await _notificationRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        await _notificationRepo.DeleteAsync(n => n.Id == id);
        return await _notificationRepo.SaveChangesAsync() > 0;
    }

    public async Task<PaginatedResult<Notification>> GetNotificationsAsync(GetNotificationsRequest request)
    {
        return await _notificationRepo.GetPageAsync(
            new PaginationRequest(request.PageIndex, request.PageSize),
            CancellationToken.None,
            n => n.UserId == request.UserId &&
                 (request.IsRead == null || n.IsRead == request.IsRead) &&
                 (request.NotificationTypeId == null || n.NotificationTypeId == request.NotificationTypeId)
        );
    }

    public async Task<PaginatedResult<Notification>> GetUnreadNotificationsAsync(GetUnreadNotificationsRequest request)
    {
        return await _notificationRepo.GetPageAsync(
            new PaginationRequest(request.PageIndex, request.PageSize),
            CancellationToken.None,
            n => n.UserId == request.UserId && !n.IsRead
        );
    }

    public async Task<NotificationDto?> GetNotificationByIdAsync(long id)
    {
        var notification = await _notificationRepo.GetAsync(n => n.Id == id);
        return notification == null ? null : _mapper.Map<NotificationDto>(notification);
    }

    public async Task<NotificationDto> CreateAndReturnAsync(CreateNotificationRequest request)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            SenderId = request.SenderId,
            NotificationTypeId = request.NotificationTypeId,
            ObjectId = request.ObjectId,
            Title = request.Title,
            Content = request.Content,
            Channel = request.Channel,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _notificationRepo.AddAsync(notification);
        await _notificationRepo.SaveChangesAsync();
        return _mapper.Map<NotificationDto>(notification);
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _notificationDbSet.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in notifications)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _notificationRepo.SaveChangesAsync();
    }
}