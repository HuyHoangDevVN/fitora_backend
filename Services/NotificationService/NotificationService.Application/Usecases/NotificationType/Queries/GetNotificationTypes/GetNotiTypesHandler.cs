using BuildingBlocks.Pagination.Base;

namespace NotificationService.Application.Usecases.NotificationType.Queries.GetNotificationTypes;

public class GetNotiTypesHandler(INotificationTypeRepository notificationTypeRepo) : IQueryHandler<GetNotiTypesQuery, PaginatedResult<Domain.Models.NotificationType>>
{
    public async Task<PaginatedResult<Domain.Models.NotificationType>> Handle(GetNotiTypesQuery request, CancellationToken cancellationToken)
    {
        var result = await notificationTypeRepo.GetListAsync(request.Request);
        return result;
    }
}