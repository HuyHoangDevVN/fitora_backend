namespace NotificationService.Application.Services.IServices;

public interface IRabbitMqConsumer<T>
{
    Task StartConsumingAsync(string userRegisterQueue, Func<T, Task> messageHandler);
    Task StopConsumingAsync();
}