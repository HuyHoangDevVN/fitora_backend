using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserService.Application.Messaging.MessageHandlers.IHandlers;

namespace UserService.Application.Services;

public class RabbitMqConsumerHostedService : BackgroundService
{
    private readonly IRabbitMqConsumer<UserRegisteredMessageDto> _rabbitMqConsumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;

    public RabbitMqConsumerHostedService(
        IRabbitMqConsumer<UserRegisteredMessageDto> rabbitMqConsumer,
        IServiceProvider serviceProvider,
        IMapper mapper)
    {
        _rabbitMqConsumer = rabbitMqConsumer;
        _serviceProvider = serviceProvider;
        _mapper = mapper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("RabbitMQ Consumer is starting.");

        try
        {
            // Truyền hàm xử lý thông điệp vào StartConsumingAsync
            await _rabbitMqConsumer.StartConsumingAsync("user_registration_queue", async message =>
            {
                using var scope = _serviceProvider.CreateScope();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var userModel = new User
                {
                    Id = message.UserId,
                    Email = message.Email,
                    Username = message.FullName,
                };
                await userRepository.CreateUserAsync(userModel);
                Console.WriteLine($"User created: {message.UserId}");
            });

            Console.WriteLine("Started consuming messages from 'user_registration_queue'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while setting up RabbitMQ Consumer: {ex.Message}");
        }

        // Đảm bảo dịch vụ tiếp tục chạy
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("RabbitMQ Consumer is stopping.");
        return base.StopAsync(cancellationToken);
    }
}
