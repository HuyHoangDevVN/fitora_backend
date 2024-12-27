using AuthService.Domain.Events;
using BuildingBlocks.Abstractions.Entities;

namespace AuthService.Application.Services;

public class MessageQueueService : IMessageQueueService
{
    private readonly IMessageQueue _messageQueue;

    public MessageQueueService(IMessageQueue messageQueue)
    {
        _messageQueue = messageQueue;
    }

    public async Task PublishUserRegisteredEventAsync(Guid userId, string name, string email, DateTime registeredAt)
    {
        var userRegisteredEvent = new UserRegisteredEvent(userId, name, email, registeredAt);

        // Gửi sự kiện đến RabbitMQ
        await _messageQueue.PublishAsync("UserRegisteredQueue", userRegisteredEvent);
    }
}