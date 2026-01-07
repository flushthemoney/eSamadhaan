using Microsoft.AspNetCore.Mvc;

namespace eSamadhaan.API.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint for container orchestration
    /// </summary>
    [HttpGet("health")]
    [HttpGet("api/health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "eSamadhaan API"
        });
    }
}

