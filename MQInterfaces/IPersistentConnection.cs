using RabbitMQ.Client;

namespace Rmq.Interfaces;

public interface IPersistentConnection : IDisposable
{
    bool IsConnected { get; }
    bool TryConnect();
    IModel CreateModel();
}