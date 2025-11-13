using Microsoft.AspNetCore.Mvc;

namespace TwitterCloneApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow,
        service = "TwitterCloneApi",
        version = "1.0.0"
    });
}
