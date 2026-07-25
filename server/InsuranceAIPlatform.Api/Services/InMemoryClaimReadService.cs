using InsuranceAIPlatform.Api.Contracts.Claims;
using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Api.Services;

/// <summary>
/// Singleton in-memory claim service. Seed data is deterministic, synthetic, and mirrors
/// the frontend mock at src/data/mock/claims.ts and src/data/mock/claim-1006.ts.
/// No EF Core, no DB, no external providers.
/// All AI outputs are advisory — human approval is always final.
/// </summary>
public sealed class InMemoryClaimReadService : IClaimReadService
{
    // -----------------------------------------------------------------------
    // Seed — CLM-1006 golden claim (mirrors frontend mock exactly)
    // -----------------------------------------------------------------------

    private static readonly ClaimDetailsDto Clm1006Details = new(
        Id: "CLM-1006",
        Customer: "Robert Johnson",
        CustomerId: "CUST-4421",
        Vehicle: "Toyota Camry 2021",
        VehicleVin: "VIN ****8842",
        Policy: "Auto Comprehensive",
        PolicyId: "POL-2025-AC-4421",
        EventType: "RoadAccident",
        EventDate: new DateOnly(2026, 5, 18),
        Location: "Springfield, Main Street 24",
        Description: "Collision at an intersection while performing a right-turn manoeuvre.",
        Status: ClaimContractCodes.Status.InProgress,
        Risk: ClaimContractCodes.Risk.High,
        RiskScore: 82,
        Confidence: 78,
        SlaDeadline: new DateTimeOffset(2026, 5, 27, 18, 0, 0, TimeSpan.Zero),
        DocumentsReceived: 6,
        DocumentsTotal: 7,
        MissingDocument: "Rear bumper damage photo",
        Estimate: 2720.00m,
        ExpectedBenchmark: 1970.00m,
        Deductible: 500.00m,
        RecommendedPayout: 1800.00m,
        TraceId: "trc_8f3d2a7e",
        RunId: "run_8f3d2a7e",
        Tokens: 4261,
        Cost: 0.0187m,
        DurationSec: 18.9);

    // -----------------------------------------------------------------------
    // Seed — claims list (mirrors src/data/mock/claims.ts claimRows)
    // -----------------------------------------------------------------------

    private static readonly IReadOnlyList<ClaimListItemDto> SeedClaimList = new List<ClaimListItemDto>
    {
        new("CLM-1006", "Robert Johnson", "Toyota Camry 2021", "RoadAccident",
            ClaimContractCodes.Status.InProgress, "6/7", ClaimContractCodes.AiStatus.AiVerified,
            ClaimContractCodes.Risk.High, "4h",
            "Request photo", new DateTimeOffset(2026, 5, 18, 10, 22, 0, TimeSpan.Zero)),
        new("CLM-1007", "Maria Coval", "VW Golf 2019", "Parking",
            ClaimContractCodes.Status.CollectingDocuments, "3/5", ClaimContractCodes.AiStatus.NeedsReview,
            ClaimContractCodes.Risk.Medium, "6h",
            "Request repair invoice", new DateTimeOffset(2026, 5, 26, 8, 15, 0, TimeSpan.Zero)),
        new("CLM-1008", "Ivan Petrenko", "Ford Focus 2020", "Collision",
            ClaimContractCodes.Status.Ready, "4/4", ClaimContractCodes.AiStatus.AiVerified,
            ClaimContractCodes.Risk.Low, "2h",
            "Approve", new DateTimeOffset(2026, 5, 26, 12, 0, 0, TimeSpan.Zero)),
        new("CLM-1009", "Elena Shevchenko", "Renault Megane 2018", "Damage",
            ClaimContractCodes.Status.CollectingDocuments, "2/6", ClaimContractCodes.AiStatus.AwaitingDocuments,
            ClaimContractCodes.Risk.Medium, "1d",
            "Request documents", new DateTimeOffset(2026, 5, 25, 9, 0, 0, TimeSpan.Zero)),
        new("CLM-1010", "David Wilson", "BMW X3 2022", "RoadAccident",
            ClaimContractCodes.Status.AiProcessing, "5/5", ClaimContractCodes.AiStatus.Processing,
            ClaimContractCodes.Risk.Low, "3h",
            "Await AI", new DateTimeOffset(2026, 5, 27, 7, 25, 0, TimeSpan.Zero)),
    };

    // -----------------------------------------------------------------------
    // Seed — documents for CLM-1006 (mirrors documentsChecklist + damagePhotos)
    // -----------------------------------------------------------------------

    private static readonly IReadOnlyList<ClaimDocumentDto> Clm1006Documents = new List<ClaimDocumentDto>
    {
        new("application",   "Customer statement",      "19.05.2026",        "ok",      "document", null),
        new("police",        "Police report",           "No. PR-2026/05/441","ok",      "document", null),
        new("photo-front",   "Photo — front",           "AI conf 92%",       "ok",      "photo",    92),
        new("photo-side",    "Photo — side",            "AI conf 87%",       "ok",      "photo",    87),
        new("invoice",       "Repair invoice",          "Amount +38%",       "warn",    "document", null),
        new("policy-terms",  "Policy terms",            "Auto Comprehensive","ok",      "document", null),
        new("photo-rear",    "Photo — rear bumper",     "MISSING",           "missing", "photo",    null),
    };

    // -----------------------------------------------------------------------
    // Seed — AI evidence for CLM-1006
    // -----------------------------------------------------------------------

    private static readonly AiEvidenceDto Clm1006AiEvidence = new(
        RunId: "run_8f3d2a7e",
        ModelConfidence: 78,
        Findings: new AiFindingDto[]
        {
            new("f1", "Documents",      "Rear bumper photo is missing. 6 of 7 documents provided.", "warn"),
            new("f2", "Damage estimate","Estimate $2,720 exceeds the $1,970 benchmark by 38%.",     "warn"),
            new("f3", "Coverage",       "The road accident is covered by Auto Comprehensive. The $500 deductible applies.", "ok"),
        },
        Evidence: new EvidenceSourceDto[]
        {
            new("e1", "Police report",     "Road accident on 18.05.2026 in Springfield confirmed.",                 95),
            new("e2", "Repair invoice",    "Total $2,720. Breakdown: bumper $980, paint $740, bodywork $1,000.",    87),
        },
        ExtractedEntities: new ExtractedEntityDto[]
        {
            new("Accident date","18.05.2026",       "Police report",     99),
            new("Vehicle",     "Toyota Camry 2021", "Policy",            98),
            new("Amount",      "$2,720",            "Repair invoice",    94),
            new("Policy",      "POL-2025-AC-4421",  "Policy",            100),
            new("Claimant",    "Robert Johnson",    "Statement",         100),
            new("Location",    "Springfield, Main Street 24", "Report", 95),
        },
        ModelConfidenceBreakdown: new ConfidenceBreakdownItemDto[]
        {
            new("Extraction",     95),
            new("Coverage",       92),
            new("Damage",         71),
            new("Recommendation", 78),
        });

    // -----------------------------------------------------------------------
    // Seed — risk assessment for CLM-1006
    // -----------------------------------------------------------------------

    private static readonly RiskAssessmentDto Clm1006Risks = new(
        Score: 82,
        Threshold: 60,
        Level: ClaimContractCodes.Risk.High,
        Factors: new RiskFactorDto[]
        {
            new("amount",        "Repair amount above the expected range",     25),
            new("mismatch",      "Discrepancies between driver statements",    18),
            new("missing-photo", "Damage photo missing",                       22),
            new("prior",         "Customer prior claims",                       8),
            new("confidence",    "Confidence below the 85% threshold",          9),
        },
        Pipeline: new PipelineStageDto[]
        {
            new("Document classifier",  "OK"),
            new("Field extraction",     "OK"),
            new("Risk engine",          "WARN"),
            new("Recommender",          "OK"),
            new("Governance",           "BLOCK"),
        });

    // -----------------------------------------------------------------------
    // Seed — policy for CLM-1006
    // -----------------------------------------------------------------------

    private static readonly PolicyDto Clm1006Policy = new(
        PolicyId: "POL-2025-AC-4421",
        ProductName: "Auto Comprehensive",
        CoverageBlocks: new PolicyCoverageDto[]
        {
            new("cov-collision",  "Collision",        "$50,000",  "$500",    true,  null),
            new("cov-liability",  "Liability",        "$100,000", "$0",      false, null),
            new("cov-glass",      "Glass",            "$1,500",   "$100",    false, null),
            new("cov-theft",      "Theft",            "Market",   "$1,000",  false, null),
            new("cov-roadside",   "Roadside assist",  "24/7",     "$0",      false, null),
        },
        Validation: new PolicyCheckResultDto(
            Covered: true,
            CoverageType: "Collision",
            ValidationNotes: new[]
            {
                "Coverage confirmed",
                "Accident date within the policy period",
                "No lapse detected",
                "Collision is covered",
                "$500 deductible applies",
                "No exclusions found",
            },
            ExclusionTriggered: false));

    // -----------------------------------------------------------------------
    // Seed — customer & vehicle for CLM-1006
    // -----------------------------------------------------------------------

    private static readonly CustomerVehicleContextDto Clm1006CustomerVehicle = new(
        Customer: new CustomerDto(
            CustomerId: "CUST-4421",
            FullName: "Robert Johnson",
            PreviousClaimsCount: 2,
            CustomerSince: new DateOnly(2021, 3, 15),
            CommunicationHistory: new CommunicationEntryDto[]
            {
                new(new DateOnly(2026, 5, 19), "Email",    "Rear bumper photo request"),
                new(new DateOnly(2026, 5, 19), "Chat",     "Repair invoice provided"),
                new(new DateOnly(2026, 5, 18), "Phone",    "Policy verification"),
                new(new DateOnly(2026, 5, 18), "Web",      "Accident notification"),
            }),
        Vehicle: new VehicleDto(
            Make: "Toyota",
            Model: "Camry",
            Year: 2021,
            Vin: "VIN ****8842",
            Color: "Silver",
            Mileage: 42300));

    // -----------------------------------------------------------------------
    // Seed — approval draft for CLM-1006
    // -----------------------------------------------------------------------

    private static readonly ApprovalDraftDto Clm1006Approval = new(
        ClaimId: "CLM-1006",
        CurrentDecision: null,
        Notes: null,
        SavedAt: null,
        Submitted: false,
        SubmittedAt: null,
        AvailableOptions: new HumanDecisionOptionDto[]
        {
            new("request",  "Request additional documents", true,  "AI recommended — request the rear bumper photo"),
            new("approve",  "Approve payout",               false, "If the risks are acceptable after review"),
            new("reject",   "Reject the claim",             false, "With a written justification"),
            new("escalate", "Escalate to the investigation unit", false, "Escalation for a detailed investigation"),
        },
        AiRecommendation: "Request additional documents",
        RecommendedPayout: 1800.00m);

    // -----------------------------------------------------------------------
    // Seed — audit trace for CLM-1006 (mirrors auditTrail + costDistribution)
    // -----------------------------------------------------------------------

    private static readonly AuditTraceDto Clm1006Audit = new(
        RunId: "run_8f3d2a7e",
        TraceId: "trc_8f3d2a7e",
        Model: "Azure OpenAI (mock)",
        Tokens: 4261,
        Cost: 0.0187m,
        DurationSec: 18.9,
        Events: new AuditEventDto[]
        {
            new("14:05:12", "AI Pipeline",    "Analysis started for CLM-1006", "OK"),
            new("14:05:14", "Doc Classifier", "Classified 6 documents",        "OK"),
            new("14:05:19", "Field Extractor","Extracted 47 fields",           "OK"),
            new("14:05:25", "Risk Engine",    "Risk 82/100 — High",            "WARN"),
            new("14:05:30", "Recommender",    "Recommendation: request photo", "OK"),
            new("14:05:31", "Governance",     "Auto-approval blocked",         "BLOCK"),
        },
        CostDistribution: new CostDistributionItemDto[]
        {
            new("Extraction",     0.0072m),
            new("RAG / evidence", 0.0058m),
            new("Risk",           0.0029m),
            new("Recommendation", 0.0028m),
        });

    // -----------------------------------------------------------------------
    // Seed — demo scenario (mirrors demoSteps from claim-1006.ts)
    // -----------------------------------------------------------------------

    private static readonly DemoScenarioDto SeedDemoScenario = new(
        Steps: new DemoStepDto[]
        {
            new(1, "Overview",       "Claims queue status",     "Board 01", "/"),
            new(2, "Select CLM-1006","Toyota Camry",            "Board 03", "/claims/CLM-1006"),
            new(3, "Documents and photos","6/7 + one missing",  "Board 04", "/claims/CLM-1006/documents"),
            new(4, "AI evidence",    "4 findings + RAG",        "Board 05", "/claims/CLM-1006/ai-evidence"),
            new(5, "Risk assessment","82/100 High",             "Board 06", "/claims/CLM-1006/risks"),
            new(6, "Human decision", "Expert decides",          "Board 07", "/claims/CLM-1006/approval"),
            new(7, "Audit & Cost",   "Trace + governance",      "Board 08", "/claims/CLM-1006/audit"),
        },
        GoldenClaimId: "CLM-1006");

    // -----------------------------------------------------------------------
    // Index for O(1) lookup by claimId
    // -----------------------------------------------------------------------

    private static readonly Dictionary<string, ClaimDetailsDto> ClaimIndex =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["CLM-1006"] = Clm1006Details,
        };

    // -----------------------------------------------------------------------
    // IClaimReadService implementation
    // -----------------------------------------------------------------------

    public ClaimSummaryDto GetSummary() => new(
        TotalActive: 47,
        PendingReview: 12,
        HighRisk: 8,
        AvgSlaRemainingHours: 14.3,
        ProcessedToday: 6,
        AiAnalysisRunning: 2);

    public IReadOnlyList<ClaimListItemDto> GetClaims() => SeedClaimList;

    public ClaimDetailsDto? GetClaim(string claimId) =>
        ClaimIndex.TryGetValue(claimId, out var claim) ? claim : null;

    public IReadOnlyList<ClaimDocumentDto>? GetDocuments(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006Documents
            : null;

    public AiEvidenceDto? GetAiEvidence(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006AiEvidence
            : null;

    public RiskAssessmentDto? GetRisks(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006Risks
            : null;

    public PolicyDto? GetPolicy(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006Policy
            : null;

    public CustomerVehicleContextDto? GetCustomerVehicle(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006CustomerVehicle
            : null;

    public ApprovalDraftDto? GetApproval(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006Approval
            : null;

    public AuditTraceDto? GetAudit(string claimId) =>
        claimId.Equals("CLM-1006", StringComparison.OrdinalIgnoreCase)
            ? Clm1006Audit
            : null;

    public DemoScenarioDto GetDemoScenario() => SeedDemoScenario;
}
