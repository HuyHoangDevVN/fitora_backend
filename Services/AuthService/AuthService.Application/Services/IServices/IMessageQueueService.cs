namespace AuthService.Application.Services.IServices;

public interface IMessageQueueService
{
    Task PublishUserRegisteredEventAsync(Guid userId, string name, string email, DateTime registeredAt);
}