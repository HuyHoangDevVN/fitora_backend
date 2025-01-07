namespace UserService.Domain.Abstractions;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public string? UserName { get; set; } = "guest";
    public string? Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "my_exchange";
    public string QueueName { get; set; } = "my_queue";
    
    public string RoutingKey { get; set; } = "my_routingKey";
}