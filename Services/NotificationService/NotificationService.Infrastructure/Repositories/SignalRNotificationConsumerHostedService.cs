using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using NotificationService.Application.DTOs.MessegeQueue.Notification;
using NotificationService.Application.Services.IServices;
using NotificationService.Infrastructure.Hubs;

namespace NotificationService.Infrastructure.Repositories;

public class SignalRNotificationConsumerHostedService : BackgroundService
{
    private readonly IRabbitMqConsumer<NotificationMessageDto> _rabbitMqConsumer;
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationConsumerHostedService(
        IRabbitMqConsumer<NotificationMessageDto> rabbitMqConsumer,
        IHubContext<NotificationHub> hubContext)
    {
        _rabbitMqConsumer = rabbitMqConsumer;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("RealtimeService is starting to listen for notifications...");

        await _rabbitMqConsumer.StartConsumingAsync("noti_realtime_queue", async message =>
        {
            // Gửi notification tới đúng user qua SignalR
            await _hubContext.Clients.User(message.UserId.ToString())
                .SendAsync("ReceiveNotification", message);
            
            // await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);


            Console.WriteLine($"[RealtimeService] Sent realtime notification to user {message.UserId}");
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}