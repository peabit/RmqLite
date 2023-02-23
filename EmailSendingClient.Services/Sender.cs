using EmailSendingClient.Interfaces;
using Rmq.Interfaces;

namespace EmailSendingClient.Services;

public sealed class Sender : ISender
{
    private readonly IPublisher _publisher;

    public Sender(IPublisher publisher)
        => _publisher = publisher;

    public void Send<TMessage>(TMessage message)
        => _publisher.Publish(message);
}