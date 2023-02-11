using Microsoft.AspNetCore.Mvc;
using Rmq.Interfaces;

namespace Sender;

[ApiController]
[Route("/send")]
public sealed class Controller : ControllerBase
{
    private readonly IPublisher _publisher;

    public Controller(IPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public void Send()
    {
        _publisher.Publish("Hello!");
    }
}