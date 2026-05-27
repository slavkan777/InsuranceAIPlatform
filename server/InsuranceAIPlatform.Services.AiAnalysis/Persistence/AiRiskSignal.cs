namespace InsuranceAIPlatform.Services.AiAnalysis.Persistence;

public sealed class AiRiskSignal
{
    public string Id { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Weight { get; set; }

    public AiAnalysisRun? Run { get; set; }
}
