using Microsoft.AspNetCore.Mvc;
using RmqLite;

namespace RmqLiteExample;

[ApiController]
[Route("/send")]
public sealed class Controller : ControllerBase
{
    IPublisher _publisher;

    public Controller(IPublisher publisher)
        => _publisher = publisher;
    

    [HttpPost]
    public void Send(Message message)
        => _publisher.Publish(message);
}