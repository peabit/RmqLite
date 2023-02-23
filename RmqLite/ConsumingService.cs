using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using RmqLite.Interfaces;

namespace RmqLite;

internal class ConsumingService : IHostedService, IDisposable
{
    private readonly IPersistentConnection _connection;
    private readonly ILogger<ConsumingService> _logger;
    private IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISubscriptionsProvider _subsProvider;

    public ConsumingService(
        IPersistentConnection connection,
        ILogger<ConsumingService> logger,
        IServiceProvider serviceProvider,
        ISubscriptionsProvider subscriptionsProvider
    )
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
        _subsProvider = subscriptionsProvider;
        _logger = logger;

        InitChannel();
        StartConsume();
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    private void InitChannel()
    {
        if (!_connection.IsConnected)
        {
            _connection.TryConnect();
        }

        _logger.LogTrace("Creating RMQ consumer channel");

        _channel = _connection.CreateModel();

        _channel.BasicQos(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false
        );

        _channel.CallbackException += (s, e) =>
        {
            _logger.LogWarning(e.Exception, "Recreating RMQ consumer channel");

            _channel?.Dispose();
            InitChannel();
            StartConsume();
        };
    }

    private void StartConsume()
    {
        _logger.LogTrace("Starting RMQ consume");

        if (_channel is null)
        {
            _logger.LogError("Consume can`t start, because channel wasn`t initialize");
        }

        foreach (var messageTypeName in _subsProvider.MessageTypeNames)
        {
            _channel.ExchangeDeclare(
                exchange: messageTypeName,
                type: "fanout"
            );

            _channel.QueueDeclare(
                queue: messageTypeName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueBind(
                queue: messageTypeName,
                exchange: messageTypeName,
                routingKey: String.Empty
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;

            _channel.BasicConsume(
                queue: messageTypeName,
                autoAck: false,
                consumer: consumer
            );
        }
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
    {
        var messageJson = Encoding.UTF8.GetString(e.Body.ToArray());
        var messageTypeName = e.Exchange;

        await Consume(messageJson, messageTypeName);

        _channel.BasicAck(e.DeliveryTag, multiple: false);
    }

    private async Task Consume(string messageJson, string messageTypeName)
    {
        var consumerType = _subsProvider.GetConsumerType(messageTypeName);

        if (consumerType is null)
        {
            throw new InvalidOperationException($"Customer for {messageTypeName} was not found");
        }

        var consumer = _serviceProvider.GetService(consumerType);

        if (consumer is null)
        {
            throw new InvalidOperationException($"Customer for {messageTypeName} was not found");
        }

        var messageType = consumerType
            .GetInterface(typeof(IConsumer<>).Name)
            !.GenericTypeArguments.First();

        var message = JsonSerializer.Deserialize(messageJson, messageType);

        if (message is null)
        {
            throw new InvalidOperationException($"Invalid message format {messageTypeName}");
        }

        await Task.Yield();
        await (Task) consumerType.GetMethod("Consume").Invoke(consumer, new object[] { message });
    }

    public void Dispose()
        => _channel?.Dispose();
}