using InsuranceAIPlatform.Services.AiAnalysis.Contracts;

namespace InsuranceAIPlatform.Services.AiAnalysis.Orchestration;

/// <summary>
/// AI analysis orchestrator boundary. RunAsync persists a new run + audit + outbox.
/// GetLatestAsync reads the most recent run for a claim.
/// AI output is advisory only — no method here can approve, reject, accuse, send messages, or change status.
/// </summary>
public interface IAiAnalysisOrchestrator
{
    /// <summary>
    /// Runs AI analysis for the given claim. Persists the run, audit event, and outbox message.
    /// If the claim is not found, returns AiAnalysisResult with Status="claim_not_found" (not persisted).
    /// </summary>
    Task<AiAnalysisResult> RunAsync(
        string claimId,
        string correlationId,
        string actor,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the latest AI analysis run for the given claim, or null if none exists.
    /// </summary>
    Task<AiAnalysisResult?> GetLatestAsync(string claimId, CancellationToken ct = default);
}
