namespace Rmq.Interfaces;

public interface IPublisher
{
    void Publish<TMessage>(TMessage message);
}