using RabbitMQ.Client;

namespace RmqLite;

public interface IPersistentConnection : IDisposable
{
    bool IsConnected { get; }
    bool TryConnect();
    IModel CreateModel();
}