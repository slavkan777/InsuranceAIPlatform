using InsuranceAIPlatform.Api.Contracts.Claims;
using InsuranceAIPlatform.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

/// <summary>Demo walkthrough scenario endpoint.</summary>
[ApiController]
[Route("api/demo")]
[Tags("Demo & System")]
public sealed class DemoController(IClaimReadService claims) : ControllerBase
{
    /// <summary>Returns the structured demo walkthrough steps for the guided tour.</summary>
    [HttpGet("scenario")]
    [ProducesResponseType(typeof(DemoScenarioDto), StatusCodes.Status200OK)]
    public ActionResult<DemoScenarioDto> GetScenario() =>
        Ok(claims.GetDemoScenario());
}
