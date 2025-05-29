using System.Text;
using InteractService.Domain.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace InteractService.Application.Messaging
{
    public class RabbitMqPublisher<T> : IRabbitMqPublisher<T>, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly RabbitMqSettings _settings;

        public RabbitMqPublisher(IOptions<RabbitMqSettings> settings)
        {
            _settings = settings.Value;
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true // Tự động khôi phục kết nối khi lỗi
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _settings.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public async Task PublishMessageAsync(T message, string queueName)
        {
            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            await policy.ExecuteAsync(async () =>
            {
                var messageJson = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageJson);
                _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                await Task.CompletedTask; // Đảm bảo async hoàn chỉnh
            });
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}