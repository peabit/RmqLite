using Microsoft.AspNetCore.Mvc;

namespace Sender;

[ApiController]
[Route("/send")]
public sealed class Controller : ControllerBase
{
    [HttpPost]
    public void Send()
    {

    }
}