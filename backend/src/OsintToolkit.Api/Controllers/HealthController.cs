using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OsintToolkit.Api.Contracts.Responses;

namespace OsintToolkit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse(
            Status: "Healthy",
            Service: "OSINT Toolkit Local API",
            TimestampUtc: DateTimeOffset.UtcNow));
    }
}
