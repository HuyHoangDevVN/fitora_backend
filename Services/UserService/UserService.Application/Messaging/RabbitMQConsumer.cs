using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserService.Domain.Abstractions;

namespace UserService.Application.Messaging
{
    public sealed class RabbitMqConsumer<T> : IRabbitMqConsumer<T>, IAsyncDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task? _consumerTask;

        public RabbitMqConsumer(IOptions<RabbitMqSettings> settings)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _settings.ExchangeName, type: ExchangeType.Direct);
            _channel.QueueDeclare(queue: _settings.QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: _settings.QueueName, exchange: _settings.ExchangeName, routingKey: _settings.RoutingKey);

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartConsumingAsync(string queueName, Func<T, Task> messageHandler)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));
            if (messageHandler == null)
                throw new ArgumentNullException(nameof(messageHandler));

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                try
                {
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

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            _consumerTask = Task.Delay(Timeout.Infinite, _cancellationTokenSource.Token);
            await _consumerTask;
        }

        public async Task StopConsumingAsync()
        {
            _cancellationTokenSource.Cancel();

            if (_consumerTask != null)
            {
                try
                {
                    await _consumerTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation occurs, safe to ignore
                }
            }

            _channel?.Close();
            _connection?.Close();
        }

        public ValueTask DisposeAsync()
        {
            _channel?.Close();
            _connection?.Close();
            _cancellationTokenSource.Dispose();
            return default;
        }
    }
}