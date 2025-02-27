using System.Text;
using AuthService.Domain.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuthService.Application.Messaging
{
    public class RabbitMqConsumer<T> : IRabbitMqConsumer<T>, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly RabbitMqSettings _settings;
        private readonly CancellationTokenSource _cts;
        private readonly AsyncEventingBasicConsumer _consumer;

        public RabbitMqConsumer(IOptions<RabbitMqSettings> settings)
        {
            _settings = settings.Value;
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _settings.ExchangeName, type: ExchangeType.Direct);
            _channel.QueueDeclare(queue: _settings.QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: _settings.QueueName, exchange: _settings.ExchangeName, routingKey: _settings.RoutingKey);
            _cts = new CancellationTokenSource();
            _consumer = new AsyncEventingBasicConsumer(_channel);
        }

        public Task StartConsumingAsync(Func<T, Task> messageHandler)
        {
            _consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var message = JsonConvert.DeserializeObject<T>(messageJson);
                    if (message != null)
                    {
                        await messageHandler(message);
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };
            _channel.BasicConsume(queue: _settings.QueueName, autoAck: false, consumer: _consumer);
            return Task.CompletedTask; // Không cần chờ vô hạn
        }

        public async Task StopConsumingAsync()
        {
            _cts.Cancel();
            await Task.CompletedTask; // Đơn giản hóa, không cần chờ task vô hạn
        }

        public async ValueTask DisposeAsync()
        {
            _channel?.Close();
            _connection?.Close();
            _cts.Dispose();
            await Task.CompletedTask;
        }
    }
}