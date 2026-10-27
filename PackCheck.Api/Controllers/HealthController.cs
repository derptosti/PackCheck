using Microsoft.AspNetCore.Mvc;

namespace PackCheck.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "PackCheck API is running"
        });
    }
}