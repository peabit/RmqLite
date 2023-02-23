namespace RmqLite;

public interface IPublisher
{
    void Publish<TMessage>(TMessage message);
}