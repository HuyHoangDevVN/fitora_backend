namespace UserService.Application.Services.IServices;

public interface IRabbitMqPublisher<T>
{
    Task PublishMessageAsync(T message, string queueName);
}