using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UserService.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Messaging.MessageHandlers.IHandlers;

namespace UserService.Application.Messaging
{
    public class RabbitMqConsumer<T> : IRabbitMqConsumer<T>
    {
        private readonly RabbitMqSettings _settings;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task _consumerTask;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqConsumer(IOptions<RabbitMqSettings> settings, IServiceProvider serviceProvider)
        {
            _settings = settings.Value;
            _serviceProvider = serviceProvider;

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
            _channel.QueueBind(queue: _settings.QueueName, exchange: _settings.ExchangeName,
                routingKey: _settings.RoutingKey);

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartConsumingAsync(string queueName, Func<T, Task> messageHandler)
        {
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
                        await messageHandler(message); // Gọi hàm xử lý message được truyền vào
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
                await _consumerTask;
            }

            if (_channel != null)
            {
                await Task.Run(() => _channel.Close());
            }

            if (_connection != null)
            {
                await Task.Run(() => _connection.Close());
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await Task.Run(() => _channel.Close());
            }

            if (_connection != null)
            {
                await Task.Run(() => _connection.Close());
            }

            _cancellationTokenSource.Dispose();
        }
    }
}