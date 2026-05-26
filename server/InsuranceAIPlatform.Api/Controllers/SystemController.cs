using InsuranceAIPlatform.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

[ApiController]
[Route("api/system")]
[Tags("System")]
public sealed class SystemController : ControllerBase
{
    /// <summary>Synthetic demo-stage status. No real backend/DB/AI is connected yet.</summary>
    [HttpGet("demo-status")]
    [ProducesResponseType(typeof(DemoStatusResponse), StatusCodes.Status200OK)]
    public ActionResult<DemoStatusResponse> GetDemoStatus() =>
        Ok(new DemoStatusResponse(
            Project: "InsuranceAIPlatform",
            Mode: "LocalDemo",
            Data: "Synthetic",
            Backend: "Skeleton",
            Database: "NotConnected",
            AiProvider: "NotConnected",
            ClaimFlow: "Planned",
            HumanApprovalRequired: true,
            Message: "Backend skeleton is running. Claims API, database, and AI provider are planned future gates.",
            TimestampUtc: DateTimeOffset.UtcNow));
}
