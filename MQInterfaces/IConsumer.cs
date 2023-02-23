namespace Rmq.Interfaces;

public interface IConsumer<TMessage> 
    where TMessage : class
{
    void Consume(TMessage message);
}