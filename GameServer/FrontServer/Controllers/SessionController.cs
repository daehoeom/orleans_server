using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Packet.Http;

namespace GameServer.Controllers;

[ApiController]
[Route("api/v2/{controller}")]
public class SessionController(ILogger<SessionController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginReq request)
    {
        
        
        return Ok();
    }
}