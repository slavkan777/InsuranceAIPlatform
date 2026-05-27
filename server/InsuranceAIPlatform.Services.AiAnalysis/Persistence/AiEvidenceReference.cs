namespace InsuranceAIPlatform.Services.AiAnalysis.Persistence;

public sealed class AiEvidenceReference
{
    public string Id { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public int Confidence { get; set; }

    public AiAnalysisRun? Run { get; set; }
}
