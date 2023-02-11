using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Polly.Retry;
using System.Net.Sockets;
using RabbitMQ.Client.Exceptions;
using Polly;
using Rmq.Interfaces;

namespace Rmq.Services;

public sealed class PersistentConnection : IPersistentConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger _logger;
    private readonly int _retryCount;
    private IConnection? _connection;
    private bool _disposed;
    private readonly object _lock = new object();

    public PersistentConnection(IConnectionFactory connectionFactory, ILogger<PersistentConnection> logger, int retryCount = 5)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _retryCount = retryCount;
    }

    public bool IsConnected
        => _connection is not null && _connection.IsOpen && !_disposed;

    public IModel CreateModel()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("No RMQ connections are available to perform this action");
        }

        return _connection.CreateModel();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            _connection.Dispose();
        }
        catch (IOException ex)
        {
            _logger.LogCritical(ex.ToString());
        }
    }

    public bool TryConnect()
    {
        _logger.LogInformation("RMQ client is trying to connect");

        lock (_lock)
        {
            var policy = RetryPolicy
                .Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(
                    retryCount: _retryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (ex, time) => _logger.LogWarning(
                        ex, "RMQ client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message
                    )
                );

            policy.Execute(() =>
                _connection = _connectionFactory.CreateConnection()
            );

            if (IsConnected)
            {
                _connection.ConnectionShutdown += (s, e) => TryConnect("A RMQ connection is shutdown");
                _connection.CallbackException += (s, e) => TryConnect("A RMQ connection is shutdown");
                _connection.ConnectionBlocked += (s, e) => TryConnect("A RMQ connection is on shutdown");

                _logger.LogInformation(
                    "RMQ client acquired a persistent connection to '{HostName}' and is subscribed to failure events", _connection.Endpoint.HostName
                );

                return true;
            }
            else
            {
                _logger.LogCritical("RMQ connections could not be created and opened");
                return false;
            }
        }
    }

    private void TryConnect(string logMessage)
    {
        if (_disposed)
        {
            return;
        }

        _logger.LogWarning(logMessage + " Trying to re-connect...");

        TryConnect();
    }
}