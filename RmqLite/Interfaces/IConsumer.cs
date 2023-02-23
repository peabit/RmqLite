namespace RmqLite;

public interface IConsumer<TMessage> 
    where TMessage : class
{
    Task Consume(TMessage message);
}