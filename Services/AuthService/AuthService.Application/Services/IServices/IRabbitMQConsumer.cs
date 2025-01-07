namespace AuthService.Application.Services.IServices;

public interface IRabbitMqConsumer<T>
{
    Task StartConsumingAsync(Func<T, Task> messageHandler);
    Task StopConsumingAsync();
}