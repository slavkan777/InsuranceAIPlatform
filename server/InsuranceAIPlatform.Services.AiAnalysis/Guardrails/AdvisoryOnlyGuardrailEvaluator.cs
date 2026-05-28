using InsuranceAIPlatform.Services.AiAnalysis.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Guardrails;

/// <summary>
/// Scans all text fields of the raw provider output for forbidden authority-language tokens.
/// If any forbidden token is found, the run is blocked (Status="blocked_unsafe").
/// GuardrailFlags are always advisory-only regardless of the blocked state.
/// No external calls; fully deterministic.
/// </summary>
public sealed class AdvisoryOnlyGuardrailEvaluator : IGuardrailEvaluator
{
    // Forbidden token-sets — any match (case-insensitive) blocks the run.
    private static readonly string[] ForbiddenPhrases =
    [
        "approve payout",
        "approve the payout",
        "payout approved",
        "claim rejected",
        "reject claim",
        "claim approved",
        "fraud confirmed",
        "final decision",
        "case closed",
        "send to customer",
        "email customer",
        "sms customer",
        "status changed",
        "set status",
    ];

    public GuardrailAssessment Evaluate(AiProviderRawOutput raw)
    {
        // Collect all text fields to scan
        var textCandidates = new List<string?>();
        textCandidates.Add(raw.SummaryText);
        textCandidates.Add(raw.RecommendedActionText);
        textCandidates.Add(raw.PolicyExplanationText);
        foreach (var f in raw.Findings)
        {
            textCandidates.Add(f.Text);
            textCandidates.Add(f.Category);
        }
        foreach (var r in raw.Risks)
        {
            textCandidates.Add(r.Label);
        }
        foreach (var e in raw.Evidence)
        {
            textCandidates.Add(e.Note);
            textCandidates.Add(e.Source);
        }

        foreach (var text in textCandidates)
        {
            if (string.IsNullOrEmpty(text)) continue;
            var lower = text.ToLowerInvariant();
            foreach (var phrase in ForbiddenPhrases)
            {
                if (lower.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                {
                    return new GuardrailAssessment(
                        Blocked: true,
                        ReasonCode: "unsafe_authority_language",
                        OffendingPhrase: phrase,
                        Flags: GuardrailFlags.Advisory);
                }
            }
        }

        return new GuardrailAssessment(
            Blocked: false,
            ReasonCode: null,
            OffendingPhrase: null,
            Flags: GuardrailFlags.Advisory);
    }
}
