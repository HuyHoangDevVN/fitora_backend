namespace AuthService.Application.Services.IServices;

public interface IRabbitMQPublisher<T>
{
    Task PublishMessageAsync(T message, string queueName);
}