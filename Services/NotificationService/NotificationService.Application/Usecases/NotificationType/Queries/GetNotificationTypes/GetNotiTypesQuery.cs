using BuildingBlocks.Pagination.Base;
using NotificationService.Application.DTOs.NotificationType.Requests;

namespace NotificationService.Application.Usecases.NotificationType.Queries.GetNotificationTypes;

public record GetNotiTypesQuery(GetListNotificationTypesRequest Request) : IQuery<PaginatedResult<Domain.Models.NotificationType>>;