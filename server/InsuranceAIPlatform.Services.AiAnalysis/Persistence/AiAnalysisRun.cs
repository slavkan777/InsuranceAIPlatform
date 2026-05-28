namespace InsuranceAIPlatform.Services.AiAnalysis.Persistence;

/// <summary>
/// AI analysis run for a claim. ProviderMode MUST be "Disabled" or "Mock" — never a real provider name.
/// ClaimId is a cross-context string reference.
/// New nullable structured fields added by gate AddAiAnalysisRunStructuredFields.
/// </summary>
public sealed class AiAnalysisRun
{
    public string RunId { get; set; } = string.Empty;       // e.g. "run_8f3d2a7e"
    public string ClaimId { get; set; } = string.Empty;
    public string ProviderMode { get; set; } = "Disabled";  // MUST be "Disabled" or "Mock"
    public int ModelConfidence { get; set; }
    public int Tokens { get; set; }
    public decimal Cost { get; set; }

    // -----------------------------------------------------------------------
    // Nullable structured fields added by AddAiAnalysisRunStructuredFields migration.
    // Existing seeded rows have these as null (safe; backfill on new runs only).
    // -----------------------------------------------------------------------

    /// <summary>AI model name (e.g. "local-mock-v0.1"). Nullable for backward compat with seeded rows.</summary>
    public string? ModelName { get; set; }

    /// <summary>"succeeded" | "blocked_unsafe" | "claim_not_found"</summary>
    public string? Status { get; set; }

    /// <summary>AI-generated summary text. Advisory only.</summary>
    public string? SummaryText { get; set; }

    /// <summary>JSON-serialized RecommendedActionOut record. Advisory only.</summary>
    public string? RecommendedActionJson { get; set; }

    /// <summary>Policy coverage explanation text. Advisory only.</summary>
    public string? PolicyExplanationText { get; set; }

    /// <summary>JSON-serialized GuardrailFlags. Always advisory-only.</summary>
    public string? GuardrailFlagsJson { get; set; }

    /// <summary>"low" | "moderate" | "high" — derived from risk signal weight sum.</summary>
    public string? RiskLevel { get; set; }

    /// <summary>UTC timestamp when this run was created.</summary>
    public DateTime? CreatedAtUtc { get; set; }

    /// <summary>Correlation ID from the triggering request.</summary>
    public string? CorrelationId { get; set; }

    public ICollection<AiFinding> Findings { get; set; } = new List<AiFinding>();
    public ICollection<AiEvidenceReference> EvidenceReferences { get; set; } = new List<AiEvidenceReference>();
    public ICollection<AiRiskSignal> RiskSignals { get; set; } = new List<AiRiskSignal>();
}
