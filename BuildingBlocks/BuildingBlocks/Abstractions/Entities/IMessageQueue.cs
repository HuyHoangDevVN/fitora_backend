namespace BuildingBlocks.Abstractions.Entities;

public interface IMessageQueue
{
    Task PublishAsync<T>(string queueName, T message);
}