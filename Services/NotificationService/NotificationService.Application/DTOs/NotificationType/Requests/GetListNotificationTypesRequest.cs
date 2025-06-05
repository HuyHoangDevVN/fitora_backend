using BuildingBlocks.Pagination.Base;

namespace NotificationService.Application.DTOs.NotificationType.Requests;

public record GetListNotificationTypesRequest(string? KeySearch): PaginationRequest;