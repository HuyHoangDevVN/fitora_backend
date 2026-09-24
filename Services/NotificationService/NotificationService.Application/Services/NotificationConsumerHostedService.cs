using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Application.Data;
using NotificationService.Application.DTOs.MessegeQueue.Notification;
using NotificationService.Domain.Models;

namespace NotificationService.Application.Services
{
    // Mã NotificationTypeId dùng chung giữa các service publish (Comment/Follow/FriendRequest...).
    // Không có bảng seed sẵn nên consumer tự tạo NotificationType nếu Id chưa tồn tại (FK bắt buộc phải có row).
    public static class WellKnownNotificationTypes
    {
        public static readonly Dictionary<int, (string Code, string Name)> Map = new()
        {
            [1] = ("comment", "Bình luận"),
            [2] = ("follow", "Theo dõi"),
            [3] = ("friend_request", "Lời mời kết bạn"),
            [4] = ("friend_request_accepted", "Chấp nhận kết bạn"),
        };
    }

    public class NotificationConsumerHostedService : BackgroundService
    {
        private readonly IRabbitMqConsumer<NotificationMessageDto> _rabbitMqConsumer;
        private readonly IRabbitMqPublisher<NotificationMessageDto> _rabbitMqPublisher;
        private readonly IServiceProvider _serviceProvider;

        public NotificationConsumerHostedService(
            IRabbitMqConsumer<NotificationMessageDto> rabbitMqConsumer,
            IRabbitMqPublisher<NotificationMessageDto> rabbitMqPublisher,
            IServiceProvider serviceProvider)
        {
            _rabbitMqConsumer = rabbitMqConsumer;
            _rabbitMqPublisher = rabbitMqPublisher;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("NotificationService is starting to consume...");

            try
            {
                await _rabbitMqConsumer.StartConsumingAsync("noti_queue", async message =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                    var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                    await EnsureNotificationTypeExistsAsync(db, message.NotificationTypeId);

                    var settingRepo = scope.ServiceProvider.GetRequiredService<INotificationSettingRepository>();
                    var enabled = await settingRepo.IsEnabledAsync(message.UserId, message.NotificationTypeId);
                    if (!enabled)
                    {
                        Console.WriteLine($"[NotificationService] Skipped type {message.NotificationTypeId} for user {message.UserId} (disabled)");
                        return;
                    }

                    var notification = new Notification
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
                    };

                    await notificationRepo.CreateAsync(notification);

                    await _rabbitMqPublisher.PublishMessageAsync(message, "noti_realtime_queue");

                    Console.WriteLine($"[NotificationService] Saved and pushed to realtime: {message.UserId}");
                });

                Console.WriteLine("Started consuming messages from 'noti_chanel' queue.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while setting up RabbitMQ Consumer: {ex.Message}");
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("RabbitMQ Consumer for NotificationService is stopping.");
            return base.StopAsync(cancellationToken);
        }

        private static async Task EnsureNotificationTypeExistsAsync(IApplicationDbContext db, int notificationTypeId)
        {
            var exists = await db.NotificationTypes.AnyAsync(nt => nt.Id == notificationTypeId);
            if (exists) return;

            var (code, name) = WellKnownNotificationTypes.Map.TryGetValue(notificationTypeId, out var known)
                ? known
                : ($"type_{notificationTypeId}", $"Loại thông báo #{notificationTypeId}");

            db.NotificationTypes.Add(new NotificationType { Id = notificationTypeId, Code = code, Name = name });
            try
            {
                await db.SaveChangesAsync(CancellationToken.None);
            }
            catch (DbUpdateException)
            {
                // Race: một consumer khác đã tạo row này trước — bỏ qua vì mục tiêu chỉ là đảm bảo tồn tại.
            }
        }
    }
}