using InsuranceAIPlatform.Api.Contracts.Claims;
using InsuranceAIPlatform.Api.Contracts.Common;
using InsuranceAIPlatform.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceAIPlatform.Api.Controllers;

/// <summary>
/// Read-only claims API endpoints (P0 slice).
/// All AI outputs are advisory — human approval is always final.
/// Synthetic demo data only; no real PII, no real insurer or customer represented.
/// </summary>
[Route("api/claims")]
[Tags("Claims")]
public sealed class ClaimsController(IClaimReadService claims) : ClaimsControllerBase
{
    // -----------------------------------------------------------------------
    // GET /api/claims/summary
    // -----------------------------------------------------------------------

    /// <summary>Dashboard aggregate counters across all active claims.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ClaimSummaryDto), StatusCodes.Status200OK)]
    public ActionResult<ClaimSummaryDto> GetSummary() =>
        Ok(claims.GetSummary());

    // -----------------------------------------------------------------------
    // GET /api/claims
    // -----------------------------------------------------------------------

    /// <summary>Claims queue list — all seeded claims.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClaimListItemDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<ClaimListItemDto>> GetClaims() =>
        Ok(claims.GetClaims());

    // -----------------------------------------------------------------------
    // GET /api/claims/{claimId}
    // -----------------------------------------------------------------------

    /// <summary>Full claim detail for a given claim identifier.</summary>
    [HttpGet("{claimId}")]
    [ProducesResponseType(typeof(ClaimDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<ClaimDetailsDto> GetClaim(string claimId)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        var result = claims.GetClaim(claimId);
        return result is null ? ClaimNotFound(claimId) : Ok(result);
    }

    // -----------------------------------------------------------------------
    // GET /api/claims/{claimId}/documents
    // -----------------------------------------------------------------------

    /// <summary>Merged document + photo checklist for a claim.</summary>
    [HttpGet("{claimId}/documents")]
    [ProducesResponseType(typeof(IReadOnlyList<ClaimDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<IReadOnlyList<ClaimDocumentDto>> GetDocuments(string claimId)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        var result = claims.GetDocuments(claimId);
        return result is null ? ClaimNotFound(claimId) : Ok(result);
    }

    // -----------------------------------------------------------------------
    // GET /api/claims/{claimId}/ai-evidence
    // -----------------------------------------------------------------------

    /// <summary>AI advisory analysis result. Advisory only — not a binding decision.</summary>
    [HttpGet("{claimId}/ai-evidence")]
    [ProducesResponseType(typeof(AiEvidenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<AiEvidenceDto> GetAiEvidence(string claimId)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        var result = claims.GetAiEvidence(claimId);
        return result is null ? ClaimNotFound(claimId) : Ok(result);
    }

    // -----------------------------------------------------------------------
    // GET /api/claims/{claimId}/risks
    // -----------------------------------------------------------------------

    /// <summary>Risk assessment: score, factors, and pipeline status.</summary>
    [HttpGet("{claimId}/risks")]
    [ProducesResponseType(typeof(RiskAssessmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<RiskAssessmentDto> GetRisks(string claimId)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        var result = claims.GetRisks(claimId);
        return result is null ? ClaimNotFound(claimId) : Ok(result);
    }

    // -----------------------------------------------------------------------
    // GET /api/claims/{claimId}/policy
    // -----------------------------------------------------------------------

    /// <summary>Policy coverage blocks and validation result for the claim event.</summary>
    [HttpGet("{claimId}/policy")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<PolicyDto> GetPolicy(string claimId)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        var result = claims.GetPolicy(claimId);
        return result is null ? ClaimNotFound(claimId) : Ok(result);
    }

    // -----------------------------------------------------------------------
    // GET /api/claims/{claimId}/customer-vehicle
    // -----------------------------------------------------------------------

    /// <summary>Customer identity and vehicle details for the insured asset.</summary>
    [HttpGet("{claimId}/customer-vehicle")]
    [ProducesResponseType(typeof(CustomerVehicleContextDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<CustomerVehicleContextDto> GetCustomerVehicle(string claimId)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        var result = claims.GetCustomerVehicle(claimId);
        return result is null ? ClaimNotFound(claimId) : Ok(result);
    }

    // -----------------------------------------------------------------------
    // GET /api/claims/{claimId}/approval
    // -----------------------------------------------------------------------

    /// <summary>Current approval draft state and available human decision options.</summary>
    [HttpGet("{claimId}/approval")]
    [ProducesResponseType(typeof(ApprovalDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<ApprovalDraftDto> GetApproval(string claimId)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        var result = claims.GetApproval(claimId);
        return result is null ? ClaimNotFound(claimId) : Ok(result);
    }

    // -----------------------------------------------------------------------
    // GET /api/claims/{claimId}/audit
    // -----------------------------------------------------------------------

    /// <summary>Full audit trace for a claim's AI processing run.</summary>
    [HttpGet("{claimId}/audit")]
    [ProducesResponseType(typeof(AuditTraceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<AuditTraceDto> GetAudit(string claimId)
    {
        if (ValidateClaimId(claimId) is { } bad) return bad;
        var result = claims.GetAudit(claimId);
        return result is null ? ClaimNotFound(claimId) : Ok(result);
    }
}
