using System.Text.RegularExpressions;
using InsuranceAIPlatform.Api.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

/// <summary>
/// Shared validation helpers used by ClaimsController and DemoController.
/// Keeps validation logic in one place; avoids global exception middleware (deferred).
/// </summary>
[ApiController]
public abstract class ClaimsControllerBase : ControllerBase
{
    private static readonly Regex ClaimIdPattern = new(@"^CLM-\d{4}$", RegexOptions.Compiled);

    /// <summary>
    /// Validates claimId format. Returns BadRequest action result if invalid, null if valid.
    /// Call at the top of every endpoint that accepts a claimId path param.
    /// </summary>
    protected ActionResult? ValidateClaimId(string claimId)
    {
        if (!ClaimIdPattern.IsMatch(claimId))
        {
            return BadRequest(new ApiErrorResponse(
                Code: "INVALID_CLAIM_ID",
                Message: "ClaimId must match pattern CLM-####.",
                TraceId: HttpContext.TraceIdentifier));
        }
        return null;
    }

    /// <summary>Returns a 404 ApiErrorResponse for a well-formed but unknown claimId.</summary>
    protected ActionResult ClaimNotFound(string claimId) =>
        NotFound(new ApiErrorResponse(
            Code: "CLAIM_NOT_FOUND",
            Message: $"Claim '{claimId}' was not found.",
            TraceId: HttpContext.TraceIdentifier));
}
