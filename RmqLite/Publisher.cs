using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace RmqLite;

public sealed class Publisher : IPublisher
{
    private readonly IPersistentConnection _connection;
    private readonly ILogger<Publisher> _logger;
    private readonly int _retryCount;

    public Publisher(IPersistentConnection connection, ILogger<Publisher> logger, int retryCount = 5)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _retryCount = retryCount;
    }

    public void Publish<TMessage>(TMessage message)
    {
        if (!_connection.IsConnected)
        {
            _connection.TryConnect();
        }

        var messageType = typeof(TMessage).Name;

        var policy = RetryPolicy
            .Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(
                retryCount: _retryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (ex, time) => _logger.LogWarning(
                    ex, "Could not publish message: {MessageType} after {Timeout}s ({ExceptionMessage})",
                    messageType, $"{time.TotalSeconds:n1}", ex.Message
                ));

        _logger.LogTrace("Creating RMQ channel to publish message {MessageType}", messageType);

        using (var channel = _connection.CreateModel())
        {
            var messageString = JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageString);

            policy.Execute(() =>
            {
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                _logger.LogTrace("Publishing message {MessageType} to RMQ", messageType);

                channel.ExchangeDeclare(
                    exchange: messageType, 
                    type: "fanout" 
                );

                channel.QueueDeclare(
                    queue: messageType,
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                channel.QueueBind(
                    queue: messageType, 
                    exchange: messageType, 
                    routingKey: string.Empty);

                channel.BasicPublish(
                    exchange: messageType,
                    routingKey: string.Empty,
                    basicProperties: properties,
                    body: messageBytes
                );
            });
        }
    }
}