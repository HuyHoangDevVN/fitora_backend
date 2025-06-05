namespace NotificationService.Application.Usecases.NotificationType.Queries.GetNotificationType;

public record GetNotiTypeByIdQuery(int Id) : IQuery<Domain.Models.NotificationType>;