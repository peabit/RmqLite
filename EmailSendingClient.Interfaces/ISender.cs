namespace EmailSendingClient.Interfaces;

public interface ISender
{
    void Send<TMessage>(TMessage message);
}