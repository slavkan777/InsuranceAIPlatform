using InsuranceAIPlatform.Api.Contracts.Claims;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.Claims;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<HybridClaimReadService> _logger;

    public HybridClaimReadService(
        InMemoryClaimReadService inMemory,
        IClaimsService claimsService,
        ILogger<HybridClaimReadService> logger)
    {
        _inMemory      = inMemory;
        _claimsService = claimsService;
        _logger        = logger;
    }

    /// <summary>
    /// Pulls operator-created claims from the DB. The DB is an ADDITIVE enrichment on top of
    /// the in-memory seed list, so when it is unreachable this degrades to "seed list only"
    /// instead of failing the whole read. The deployed demo runs with no SQL server, where an
    /// unguarded call turns every list request into a 500 and the demo renders nothing.
    /// The failure is logged at Warning so an outage is observable rather than silent.
    /// </summary>
    private IReadOnlyList<SyntheticClaimSummary> TryGetDbClaims()
    {
        try
        {
            return _claimsService.GetAllClaimsAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Claims database unavailable; serving the in-memory seed list only. " +
                "Operator-created claims will not appear until the database is reachable.");
            return Array.Empty<SyntheticClaimSummary>();
        }
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
        // Best-effort: an unreachable DB yields an empty enrichment, never a failed read.
        var dbRows = TryGetDbClaims();
        var extras = dbRows
            .Where(r => !seedIds.Contains(r.ClaimId))
            .Select(r => new ClaimListItemDto(
                Id:             r.ClaimId,
                Customer:       r.Customer,
                Vehicle:        r.Vehicle,
                EventType:      NormalizeEventType(r.EventType),
                // Persisted rows may still hold legacy Ukrainian values — normalize at the
                // API boundary so the wire contract is always code-shaped. Unknown values
                // pass through verbatim (never coerced into a real state).
                Status:         ClaimContractCodes.NormalizeStatus(r.Status),
                DocumentsCount: $"{r.DocumentsReceived}/{r.DocumentsTotal}",
                AiStatus:       ClaimContractCodes.AiStatus.AwaitingAi,
                Risk:           ClaimContractCodes.NormalizeRisk(r.Risk),
                Sla:            FormatSla(r.SlaDeadline),
                NextAction:     "Collect documents",
                Updated:        DateTimeOffset.UtcNow));

        return seed.Concat(extras).ToList();
    }

    public ClaimDetailsDto? GetClaim(string claimId)
    {
        var seed = _inMemory.GetClaim(claimId);
        if (seed is not null) return seed;

        // Same best-effort contract as GetClaims(): a seed claim is served from memory above,
        // and an unreachable DB means "no such claim here" (→ 404) rather than a 500.
        SyntheticClaimSummary? row;
        try
        {
            row = _claimsService.GetClaimByIdAsync(claimId).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Claims database unavailable while resolving {ClaimId}; reporting it as not found.",
                claimId);
            return null;
        }

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
            EventType:           NormalizeEventType(row.EventType),
            EventDate:           row.EventDate,
            Location:            row.Location,
            Description:         row.Description,
            // Same boundary normalization as GetClaims() — legacy persisted values in,
            // stable codes out.
            Status:              ClaimContractCodes.NormalizeStatus(row.Status),
            Risk:                ClaimContractCodes.NormalizeRisk(row.Risk),
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

    /// <summary>
    /// Canonical token for a breached SLA. The frontend matches this exact value
    /// (`SLA_OVERDUE` in `utils/claimContract.ts`) instead of a translated string.
    /// </summary>
    private const string SlaOverdue = "Overdue";

    private static string FormatSla(DateTimeOffset deadline)
    {
        var remaining = deadline - DateTimeOffset.UtcNow;
        if (remaining.TotalHours < 0) return SlaOverdue;
        if (remaining.TotalHours < 24) return $"{(int)remaining.TotalHours}h";
        return $"{(int)remaining.TotalDays}d";
    }

    // Event-type compatibility map. Rows seeded before the English-only migration still
    // hold Ukrainian event types (the claims seeder is out of scope for this lease), so
    // they are normalized to codes HERE, server-side. Keeping the mapping on the server
    // means no Cyrillic is ever shipped to the browser. Unknown values pass through
    // verbatim and are never coerced into a real state.
    private static readonly Dictionary<string, string> EventTypeLegacy = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ДТП"] = "RoadAccident",
        ["Паркування"] = "Parking",
        ["Зіткнення"] = "Collision",
        ["Пошкодження"] = "Damage",
        ["Скло"] = "Glass",
        ["Угон"] = "Theft",
        ["Викрадення"] = "Theft",
    };

    private static string NormalizeEventType(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw ?? string.Empty;
        var trimmed = raw.Trim();
        return EventTypeLegacy.TryGetValue(trimmed, out var code) ? code : trimmed;
    }
}
