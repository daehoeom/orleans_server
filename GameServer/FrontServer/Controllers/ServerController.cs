using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Controllers;

[ApiController]
[Route("api/v2/{controller}")]
public class ServerController(ILogger<ServerController> logger)
    : ControllerBase
{
    [HttpGet("heartbeat")]
    public IActionResult GetHeartBeat()
    {
        return Ok(DateTime.UtcNow);
    }
}