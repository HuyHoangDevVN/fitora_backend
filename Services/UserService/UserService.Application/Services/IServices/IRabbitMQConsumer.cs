namespace UserService.Application.Services.IServices;

public interface IRabbitMQConsumer<T>
{
    Task StartConsumingAsync(Func<T, Task> messageHandler);
    Task StopConsumingAsync();
}