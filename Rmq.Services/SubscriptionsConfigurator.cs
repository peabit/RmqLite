using RmqLite.Interfaces;

namespace RmqLite;

internal class SubscriptionsConfigurator : ISubscriptionsConfigurator
{
    private readonly Dictionary<string, Type> _subscriptions = new();

    public void Subscribe<TConsumer, TMessage>()
        where TConsumer : IConsumer<TMessage>
        where TMessage : class
    {
        _subscriptions.Add(typeof(TMessage).Name, typeof(TConsumer));
    }

    public ISubscriptionsProvider GetProvider()
        => new SubscriptionsProvider(_subscriptions);
}