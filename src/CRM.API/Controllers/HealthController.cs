using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Ok(new { status = "healthy", service = "CRM.API", timestamp = DateTime.UtcNow });
}
