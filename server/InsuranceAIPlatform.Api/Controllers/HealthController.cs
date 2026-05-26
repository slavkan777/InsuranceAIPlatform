using InsuranceAIPlatform.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

[ApiController]
[Tags("System")]
public sealed class HealthController(IWebHostEnvironment environment) : ControllerBase
{
    /// <summary>Liveness probe — confirms the skeleton is running locally.</summary>
    [HttpGet("/health")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> GetHealth() =>
        Ok(new HealthResponse(
            Status: "Healthy",
            Service: "InsuranceAIPlatform.Api",
            Environment: environment.EnvironmentName,
            TimestampUtc: DateTimeOffset.UtcNow));
}
