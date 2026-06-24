using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Packet.Http;

namespace ApiServer.Controllers;

[ApiController]
[Route("api/v2/{controller}")]
public class SessionController(ILogger<SessionController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] Login req)
    {
        return Ok(new LoginRes());
    }
}