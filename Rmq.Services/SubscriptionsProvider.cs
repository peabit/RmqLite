using RmqLite.Interfaces;

namespace RmqLite;

internal class SubscriptionsProvider : ISubscriptionsProvider
{
    private readonly IReadOnlyDictionary<string, Type> _subscriptions;

    public SubscriptionsProvider(IReadOnlyDictionary<string, Type> consumers)
        => _subscriptions = consumers;

    public IEnumerable<Type> ConsumerTypes
        => _subscriptions.Values;

    public IEnumerable<string> MessageTypeNames
        => _subscriptions.Keys;

    public Type GetConsumerType(string messageTypeName)
        => _subscriptions[messageTypeName];
}