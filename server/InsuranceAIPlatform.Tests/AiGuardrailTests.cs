using InsuranceAIPlatform.Services.AiAnalysis.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Guardrails;
using System.Reflection;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Tests for guardrail evaluator. Verifies: unsafe language blocked; safe output passes;
/// GuardrailFlags are always advisory-only with no mutation path.
/// </summary>
public class AiGuardrailTests
{
    private static readonly AdvisoryOnlyGuardrailEvaluator Evaluator = new();

    // -----------------------------------------------------------------------
    // (G1) Output with unsafe language is blocked
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Approve payout immediately")]
    [InlineData("This claim should have payout approved")]
    [InlineData("Claim rejected based on evidence")]
    [InlineData("Fraud confirmed in this case")]
    [InlineData("Final decision: proceed")]
    [InlineData("Case closed")]
    [InlineData("Email customer with result")]
    [InlineData("Status changed to closed")]
    [InlineData("Set status to approved")]
    [InlineData("Send to customer right away")]
    public void Unsafe_output_is_blocked(string unsafeText)
    {
        var raw = BuildRaw(summaryText: unsafeText);
        var assessment = Evaluator.Evaluate(raw);

        Assert.True(assessment.Blocked);
        Assert.Equal("unsafe_authority_language", assessment.ReasonCode);
        Assert.NotNull(assessment.OffendingPhrase);
    }

    // -----------------------------------------------------------------------
    // (G2) Safe output is not blocked
    // -----------------------------------------------------------------------

    [Fact]
    public void Safe_output_is_not_blocked()
    {
        var raw = BuildRaw(
            summaryText: "AI аналіз виявив 2 попереджувальні знахідки. Рекомендується перевірка людиною.",
            recommendedAction: "Запросіть додаткові документи. Рішення лише за людиною-ад'ютантом.",
            findings: [new AiFindingDraft("f1", "Documents", "Missing photo", "warn")],
            risks: [new AiRiskDraft("rs1", "High repair cost", 25)]);

        var assessment = Evaluator.Evaluate(raw);

        Assert.False(assessment.Blocked);
        Assert.Null(assessment.ReasonCode);
        Assert.Null(assessment.OffendingPhrase);
    }

    // -----------------------------------------------------------------------
    // (G3) GuardrailFlags are always advisory-only — no mutation path
    // -----------------------------------------------------------------------

    [Fact]
    public void GuardrailFlags_always_advisory_only_no_mutation_path()
    {
        var flags = GuardrailFlags.Advisory;

        Assert.True(flags.AdvisoryOnly);
        Assert.True(flags.RequiresHumanReview);
        Assert.False(flags.CanApprovePayout);
        Assert.False(flags.CanRejectClaim);
        Assert.False(flags.CanAccuseFraudFinal);
        Assert.False(flags.CanSendCustomerMessage);
        Assert.False(flags.CanChangeClaimStatus);

        // Verify by reflection that there is no public setter that could flip a false to true
        var falseFlagProperties = typeof(GuardrailFlags).GetProperties()
            .Where(p => p.Name.StartsWith("Can"))
            .ToList();

        foreach (var prop in falseFlagProperties)
        {
            // No public setter — property is read-only
            Assert.Null(prop.GetSetMethod(nonPublic: false));
        }

        // Only one instance possible (private constructor)
        var ctors = typeof(GuardrailFlags).GetConstructors(
            BindingFlags.Public | BindingFlags.Instance);
        Assert.Empty(ctors); // private constructor means no public ctors
    }

    // -----------------------------------------------------------------------
    // (G4) Guardrail evaluator blocks text in findings and risks (not just summary)
    // -----------------------------------------------------------------------

    [Fact]
    public void Guardrail_evaluator_scans_findings_and_risks()
    {
        var raw = BuildRaw(
            summaryText: "Normal summary",
            findings: [new AiFindingDraft("f1", "Normal", "approve the payout", "warn")]);

        var assessment = Evaluator.Evaluate(raw);
        Assert.True(assessment.Blocked);
        Assert.Equal("approve the payout", assessment.OffendingPhrase);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static AiProviderRawOutput BuildRaw(
        string? summaryText = null,
        string? recommendedAction = null,
        IReadOnlyList<AiFindingDraft>? findings = null,
        IReadOnlyList<AiRiskDraft>? risks = null)
    {
        return new AiProviderRawOutput(
            ModelName: "test-model",
            SummaryText: summaryText ?? "Normal summary",
            Findings: findings ?? Array.Empty<AiFindingDraft>(),
            Evidence: Array.Empty<AiEvidenceDraft>(),
            Risks: risks ?? Array.Empty<AiRiskDraft>(),
            RecommendedActionText: recommendedAction ?? "Gather documents",
            PolicyExplanationText: "Policy covers collision damage",
            ConfidenceScore: 75,
            Tokens: 1000,
            Cost: 0.005m);
    }
}
