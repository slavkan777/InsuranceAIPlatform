using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Providers;

/// <summary>
/// Deterministic mock AI provider. Returns the golden CLM-1006 shape for that claim,
/// and a safe generic stub for all other claims.
/// No HTTP, no external service, no key read, no network call of any kind.
/// </summary>
public sealed class MockAiProvider : IAiProvider
{
    public AiProviderMode Mode => AiProviderMode.Mock;

    public Task<AiProviderRawOutput> AnalyzeAsync(AiAnalysisRequest request, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        AiProviderRawOutput output = request.ClaimId == SeedConstants.GoldenClaimId
            ? BuildGoldenOutput()
            : BuildGenericStub(request.ClaimId);

        return Task.FromResult(output);
    }

    // -----------------------------------------------------------------------
    // CLM-1006 deterministic golden output — matches AiAnalysisSeeder exactly.
    // -----------------------------------------------------------------------

    private static AiProviderRawOutput BuildGoldenOutput() => new(
        ModelName: "local-mock-v0.1",
        SummaryText:
            "AI analysis produced 2 warning findings and 1 neutral finding. " +
            "The damage estimate exceeds the benchmark and some photos are missing. " +
            "Coverage is confirmed. Human review is recommended.",
        Findings:
        [
            new AiFindingDraft("f1", "Documents",       "Rear bumper photo is missing. 6 of 7 documents provided.",                       "warn"),
            new AiFindingDraft("f2", "Damage estimate", "Estimate $2,720 exceeds the $1,970 benchmark by 38%.",                           "warn"),
            new AiFindingDraft("f3", "Coverage",        "The road accident is covered by Auto Comprehensive. The $500 deductible applies.", "ok"),
        ],
        Evidence:
        [
            new AiEvidenceDraft("e1", "Police report",  "Road accident on 18.05.2026 in Springfield confirmed.",                          95),
            new AiEvidenceDraft("e2", "Repair invoice", "Total $2,720. Breakdown: bumper $980, paint $740, bodywork $1,000.",             87),
        ],
        Risks:
        [
            new AiRiskDraft("rs1", "Repair amount above the expected range",     25),
            new AiRiskDraft("rs2", "Damage photo missing",                       22),
            new AiRiskDraft("rs3", "Discrepancies between driver statements",    18),
            new AiRiskDraft("rs4", "Confidence below the 85% threshold",          9),
        ],
        RecommendedActionText: "Request the missing bumper photo. Review the repair invoice. The decision rests solely with the human adjuster.",
        PolicyExplanationText: "Policy Auto Comprehensive POL-2025-AC-4421 covers road-accident damage after the $500 deductible is applied.",
        ConfidenceScore: 78,
        Tokens: 4261,
        Cost: 0.0187m);

    // -----------------------------------------------------------------------
    // Generic stub for all other claims.
    // -----------------------------------------------------------------------

    private static AiProviderRawOutput BuildGenericStub(string claimId) => new(
        ModelName: "local-mock-v0.1",
        SummaryText: $"AI analysis for {claimId} — awaiting data.",
        Findings:
        [
            new AiFindingDraft("f1", "General", "AI analysis pending — insufficient claim data for detailed analysis.", "ok"),
        ],
        Evidence: Array.Empty<AiEvidenceDraft>(),
        Risks:
        [
            new AiRiskDraft("rs1", "Insufficient data", 15),
        ],
        RecommendedActionText: "Gather all required documents before proceeding. Human adjuster review required.",
        PolicyExplanationText: "Policy coverage analysis requires complete claim data.",
        ConfidenceScore: 60,
        Tokens: 1500,
        Cost: 0.0070m);
}
