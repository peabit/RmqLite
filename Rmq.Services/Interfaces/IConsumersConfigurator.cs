namespace RmqLite.Interfaces;

public interface ISubscriptionsConfigurator
{
    void Subscribe<TConsumer, TMessage>()
        where TConsumer : IConsumer<TMessage>
        where TMessage : class;
}