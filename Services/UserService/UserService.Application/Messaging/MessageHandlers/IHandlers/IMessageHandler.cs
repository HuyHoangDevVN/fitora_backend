namespace UserService.Application.Messaging.MessageHandlers.IHandlers;

public interface IMessageHandler<T>
{
    Task HandleAsync(T message);
}
