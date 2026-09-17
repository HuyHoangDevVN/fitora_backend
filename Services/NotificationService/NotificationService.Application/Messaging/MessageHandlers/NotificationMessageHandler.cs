using NotificationService.Application.DTOs.MessegeQueue.Notification;
using NotificationService.Application.Messaging.MessageHandlers.IHandlers;
using NotificationService.Application.Services.IServices;

namespace NotificationService.Application.Messaging.MessageHandlers;

public class NotificationMessageHandler : IMessageHandler<NotificationMessageDto>
{
    private readonly INotificationRepository _notificationRepo;
    private readonly INotificationSettingRepository _settingRepo;

    public NotificationMessageHandler(INotificationRepository notificationRepo, INotificationSettingRepository settingRepo)
    {
        _notificationRepo = notificationRepo;
        _settingRepo = settingRepo;
    }

    public async Task HandleAsync(NotificationMessageDto message)
    {
        // Check recipient's notification setting — skip if disabled for this type
        var enabled = await _settingRepo.IsEnabledAsync(message.UserId, message.NotificationTypeId);
        if (!enabled)
        {
            Console.WriteLine($"[NotificationMessageHandler] Skipped notification type {message.NotificationTypeId} for user {message.UserId} (disabled)");
            return;
        }
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
