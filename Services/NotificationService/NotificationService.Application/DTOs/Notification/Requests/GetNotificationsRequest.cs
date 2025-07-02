using BuildingBlocks.Pagination.Base;

namespace NotificationService.Application.DTOs.Notification.Requests;

public record GetNotificationsRequest(Guid UserId, bool? IsRead, int? NotificationTypeId)  : PaginationRequest;