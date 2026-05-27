using InsuranceAIPlatform.Api.Contracts.Claims;

namespace InsuranceAIPlatform.Api.Services;

/// <summary>
/// Read-only boundary for all claim data. Returns null for unknown claimIds — controllers map null to 404.
/// In-memory implementation is the single source of truth for the P0 demo gate.
/// </summary>
public interface IClaimReadService
{
    ClaimSummaryDto GetSummary();
    IReadOnlyList<ClaimListItemDto> GetClaims();
    ClaimDetailsDto? GetClaim(string claimId);
    IReadOnlyList<ClaimDocumentDto>? GetDocuments(string claimId);
    AiEvidenceDto? GetAiEvidence(string claimId);
    RiskAssessmentDto? GetRisks(string claimId);
    PolicyDto? GetPolicy(string claimId);
    CustomerVehicleContextDto? GetCustomerVehicle(string claimId);
    ApprovalDraftDto? GetApproval(string claimId);
    AuditTraceDto? GetAudit(string claimId);
    DemoScenarioDto GetDemoScenario();
}
