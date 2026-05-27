using InsuranceAIPlatform.Api.Contracts;
using InsuranceAIPlatform.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

/// <summary>
/// BFF / API Gateway identity endpoints — Stage-1 skeleton.
/// These endpoints expose the BFF layer itself; they do not own any claim data.
/// All responses are synthetic. No secrets, no DB, no AI provider.
/// </summary>
[ApiController]
[Route("api/bff")]
[Tags("BFF Gateway")]
public sealed class BffController(IWebHostEnvironment environment) : ControllerBase
{
    // -----------------------------------------------------------------------
    // GET /api/bff/health
    // -----------------------------------------------------------------------

    /// <summary>
    /// BFF identity health probe.
    /// Returns a small synthetic status document identifying this layer as the
    /// Stage-1 skeleton BFF / API Gateway.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(BffHealthResponse), StatusCodes.Status200OK)]
    public ActionResult<BffHealthResponse> GetBffHealth()
    {
        var correlationId = HttpContext.Items[CorrelationIdMiddleware.CorrelationIdKey]?.ToString()
            ?? HttpContext.TraceIdentifier;

        return Ok(new BffHealthResponse(
            Service: "bff-api-gateway",
            Status: "healthy",
            Stage: "skeleton-v0.1",
            Upstream: "in-memory-read-service",
            Environment: environment.EnvironmentName,
            CorrelationId: correlationId,
            TimestampUtc: DateTimeOffset.UtcNow));
    }

    // -----------------------------------------------------------------------
    // GET /api/bff/demo-status
    // -----------------------------------------------------------------------

    /// <summary>
    /// BFF passthrough to the system demo-status.
    /// Delegates to the same synthetic payload as /api/system/demo-status.
    /// Synthetic data only; no real insurer or customer represented.
    /// </summary>
    [HttpGet("demo-status")]
    [ProducesResponseType(typeof(DemoStatusResponse), StatusCodes.Status200OK)]
    public ActionResult<DemoStatusResponse> GetDemoStatus() =>
        Ok(new DemoStatusResponse(
            Project: "InsuranceAIPlatform",
            Mode: "LocalDemo",
            Data: "Synthetic",
            Backend: "BffGatewaySkeleton",
            Database: "NotConnected",
            AiProvider: "NotConnected",
            ClaimFlow: "Planned",
            HumanApprovalRequired: true,
            Message: "BFF gateway skeleton is running (Stage-1). Claims API, database, and AI provider are planned future gates.",
            TimestampUtc: DateTimeOffset.UtcNow));
}
