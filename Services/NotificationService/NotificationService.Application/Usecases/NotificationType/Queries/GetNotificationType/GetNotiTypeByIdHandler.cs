namespace NotificationService.Application.Usecases.NotificationType.Queries.GetNotificationType;

public class GetNotiTypeByIdHandler(INotificationTypeRepository notificationTypeRepo) : IQueryHandler<GetNotiTypeByIdQuery,Domain.Models.NotificationType>
{
    public async Task<Domain.Models.NotificationType> Handle(GetNotiTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var notificationType = await notificationTypeRepo.GetByIdAsync(request.Id);
        return notificationType;
    }
}