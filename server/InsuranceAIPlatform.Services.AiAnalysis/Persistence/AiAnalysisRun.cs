namespace InsuranceAIPlatform.Services.AiAnalysis.Persistence;

/// <summary>
/// AI analysis run for a claim. ProviderMode MUST be "Disabled" or "Mock" — never a real provider name.
/// ClaimId is a cross-context string reference.
/// </summary>
public sealed class AiAnalysisRun
{
    public string RunId { get; set; } = string.Empty;       // e.g. "run_8f3d2a7e"
    public string ClaimId { get; set; } = string.Empty;
    public string ProviderMode { get; set; } = "Disabled";  // MUST be "Disabled" or "Mock"
    public int ModelConfidence { get; set; }
    public int Tokens { get; set; }
    public decimal Cost { get; set; }

    public ICollection<AiFinding> Findings { get; set; } = new List<AiFinding>();
    public ICollection<AiEvidenceReference> EvidenceReferences { get; set; } = new List<AiEvidenceReference>();
    public ICollection<AiRiskSignal> RiskSignals { get; set; } = new List<AiRiskSignal>();
}
