using InsuranceAIPlatform.Api.Contracts.Claims;
using InsuranceAIPlatform.Services.Claims;

namespace InsuranceAIPlatform.Api.Services;

/// <summary>
/// Hybrid read service: keeps the rich in-memory data for the original 5 seed claims
/// (CLM-1006 .. CLM-1010) and falls back to the DB-backed <see cref="IClaimsService"/>
/// for any newly-created synthetic claims (CLM-1011+). Used by every BFF read endpoint.
///
/// For DB-only claims:
///   - <see cref="GetClaim"/> returns a bare ClaimDetailsDto built from the row.
///   - <see cref="GetDocuments"/>/<see cref="GetAiEvidence"/>/etc. return null when there is
///     no rich seed for the claim — controllers map that to 404 / "no data yet".
/// </summary>
public sealed class HybridClaimReadService : IClaimReadService
{
    private readonly InMemoryClaimReadService _inMemory;
    private readonly IClaimsService _claimsService;

    public HybridClaimReadService(
        InMemoryClaimReadService inMemory,
        IClaimsService claimsService)
    {
        _inMemory      = inMemory;
        _claimsService = claimsService;
    }

    public ClaimSummaryDto GetSummary() => _inMemory.GetSummary();

    public IReadOnlyList<ClaimListItemDto> GetClaims()
    {
        // Seed list (5 rich CLM-1006..1010) — kept as-is for shape parity with the prior
        // accepted gate. New DB-only claims are appended *after* the seed list.
        var seed = _inMemory.GetClaims();
        var seedIds = new HashSet<string>(seed.Select(s => s.Id), StringComparer.OrdinalIgnoreCase);

        // Pull DB rows synchronously via a blocking call — IClaimReadService is sync by contract.
        // For the local sandbox this is acceptable (single-digit row count expected).
        var dbRows = _claimsService.GetAllClaimsAsync().GetAwaiter().GetResult();
        var extras = dbRows
            .Where(r => !seedIds.Contains(r.ClaimId))
            .Select(r => new ClaimListItemDto(
                Id:             r.ClaimId,
                Customer:       r.Customer,
                Vehicle:        r.Vehicle,
                EventType:      r.EventType,
                Status:         r.Status,
                DocumentsCount: $"{r.DocumentsReceived}/{r.DocumentsTotal}",
                AiStatus:       "Очікує AI",
                Risk:           r.Risk,
                Sla:            FormatSla(r.SlaDeadline),
                NextAction:     "Зібрати документи",
                Updated:        DateTimeOffset.UtcNow));

        return seed.Concat(extras).ToList();
    }

    public ClaimDetailsDto? GetClaim(string claimId)
    {
        var seed = _inMemory.GetClaim(claimId);
        if (seed is not null) return seed;

        var row = _claimsService.GetClaimByIdAsync(claimId).GetAwaiter().GetResult();
        if (row is null) return null;

        // Build a bare details DTO for the newly-created claim. Fields the operator
        // hasn't set yet are zero / empty / placeholders. Documents / AI evidence
        // are populated by their own services as the operator adds them.
        //
        // Description IS persisted (PersistenceClaimsService passes it from the
        // CreateClaimRequest into the Claim row). Previously this DTO returned
        // `Description: string.Empty` which dropped the operator's input on the
        // floor — part of the PostManualV4 regression (see report).
        return new ClaimDetailsDto(
            Id:                  row.ClaimId,
            Customer:            row.Customer,
            CustomerId:          row.CustomerId,
            Vehicle:             row.Vehicle,
            VehicleVin:          row.VehicleVin,
            Policy:              row.Policy,
            PolicyId:            row.PolicyId,
            EventType:           row.EventType,
            EventDate:           row.EventDate,
            Location:            row.Location,
            Description:         row.Description,
            Status:              row.Status,
            Risk:                row.Risk,
            RiskScore:           row.RiskScore,
            Confidence:          0,
            SlaDeadline:         row.SlaDeadline,
            DocumentsReceived:   row.DocumentsReceived,
            DocumentsTotal:      row.DocumentsTotal,
            MissingDocument:     null,
            Estimate:            row.Estimate,
            ExpectedBenchmark:   0m,
            Deductible:          0m,
            RecommendedPayout:   row.RecommendedPayout,
            TraceId:             $"trc_{row.ClaimId.ToLowerInvariant()}",
            RunId:               string.Empty,
            Tokens:              0,
            Cost:                0m,
            DurationSec:         0);
    }

    public IReadOnlyList<ClaimDocumentDto>? GetDocuments(string claimId) =>
        _inMemory.GetDocuments(claimId);

    public AiEvidenceDto? GetAiEvidence(string claimId) =>
        _inMemory.GetAiEvidence(claimId);

    public RiskAssessmentDto? GetRisks(string claimId) =>
        _inMemory.GetRisks(claimId);

    public PolicyDto? GetPolicy(string claimId) =>
        _inMemory.GetPolicy(claimId);

    public CustomerVehicleContextDto? GetCustomerVehicle(string claimId) =>
        _inMemory.GetCustomerVehicle(claimId);

    public ApprovalDraftDto? GetApproval(string claimId) =>
        _inMemory.GetApproval(claimId);

    public AuditTraceDto? GetAudit(string claimId) =>
        _inMemory.GetAudit(claimId);

    public DemoScenarioDto GetDemoScenario() => _inMemory.GetDemoScenario();

    private static string FormatSla(DateTimeOffset deadline)
    {
        var remaining = deadline - DateTimeOffset.UtcNow;
        if (remaining.TotalHours < 0) return "Прострочено";
        if (remaining.TotalHours < 24) return $"{(int)remaining.TotalHours} год";
        return $"{(int)remaining.TotalDays} дн";
    }
}
