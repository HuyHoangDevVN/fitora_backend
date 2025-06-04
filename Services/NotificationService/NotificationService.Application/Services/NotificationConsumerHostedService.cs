using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Application.DTOs.MessegeQueue.Notification;

namespace NotificationService.Application.Services
{
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

                    await _rabbitMqPublisher.PublishMessageAsync(message, "noti_realtime_channel");

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
    }
}