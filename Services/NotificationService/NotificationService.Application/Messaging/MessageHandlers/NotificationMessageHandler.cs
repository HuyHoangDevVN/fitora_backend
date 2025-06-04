using NotificationService.Application.DTOs.MessegeQueue.Notification;
using NotificationService.Application.Messaging.MessageHandlers.IHandlers;

namespace NotificationService.Application.Messaging.MessageHandlers;

public class NotificationMessageHandler : IMessageHandler<NotificationMessageDto>
{
    private readonly INotificationRepository _notificationRepo;

    public NotificationMessageHandler(INotificationRepository notificationRepo)
    {
        _notificationRepo = notificationRepo;
    }

    public async Task HandleAsync(NotificationMessageDto message)
    {
        Console.WriteLine("NotificationMessageHandler");
        await _notificationRepo.CreateAsync(new Notification
        {
            UserId = message.UserId,
            SenderId = message.SenderId,
            NotificationTypeId = message.NotificationTypeId,
            ObjectId = message.ObjectId,
            Title = message.Title,
            Content = message.Content,
            IsRead = false,
            IsDelivered = false,
            Channel = message.Channel,
            ReadAt = null
        });
    }
}
