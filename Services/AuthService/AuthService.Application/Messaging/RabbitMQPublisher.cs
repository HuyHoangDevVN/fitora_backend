using System.Text;
using AuthService.Domain.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace AuthService.Application.Messaging
{
    public class RabbitMQPublisher<T> : IRabbitMQPublisher<T>
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMQPublisher(IOptions<RabbitMqSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task PublishMessageAsync(T message, string queueName)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            
            var messageJson = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(messageJson);

            await Task.Run(() => { channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);});
        }
    }
}
