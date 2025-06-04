namespace NotificationService.Application.Messaging.MessageHandlers.IHandlers;

public interface IMessageHandler<T>
{
    Task HandleAsync(T message);
}
