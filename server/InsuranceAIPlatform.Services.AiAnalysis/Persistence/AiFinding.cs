namespace InsuranceAIPlatform.Services.AiAnalysis.Persistence;

public sealed class AiFinding
{
    public string Id { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;    // "ok" | "warn"

    public AiAnalysisRun? Run { get; set; }
}
