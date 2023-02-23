namespace RmqLite.Interfaces;

internal interface ISubscriptionsProvider
{
    IEnumerable<Type> ConsumerTypes { get; }
    IEnumerable<string> MessageTypeNames { get; }
    Type GetConsumerType(string messageTypeName);
}