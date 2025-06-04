using BuildingBlocks.Pagination.Base;

namespace NotificationService.Application.DTOs.Notification.Requests;

public record GetUnreadNotificationsRequest(Guid UserId) : PaginationRequest;