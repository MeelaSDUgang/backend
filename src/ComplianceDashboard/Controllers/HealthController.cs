using Microsoft.AspNetCore.Mvc;

namespace ComplianceDashboard.Controllers;

[Route("api/health")]
[Tags("Health")]
public class HealthController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new { status = "ok" });
    }
}