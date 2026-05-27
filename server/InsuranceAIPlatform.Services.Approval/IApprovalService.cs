using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Approval;

/// <summary>
/// Approval service boundary. Owner of approval drafts and the human-controlled decision workflow.
/// AI is never the final authority; submit is human-only.
/// No payout, no messaging, no binary upload in any implementation.
/// </summary>
public interface IApprovalService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.Approval"/>).</summary>
    string ServiceName { get; }

    /// <summary>
    /// Upserts an approval draft for <paramref name="claimId"/>.
    /// Sets CurrentDecision and/or Notes; SavedAt = now; Submitted stays false.
    /// Returns the draft's ClaimId on success.
    /// </summary>
    Task<string> SaveDraftAsync(
        string claimId,
        string? currentDecision,
        string? notes,
        ActorContext actor,
        CancellationToken ct = default);

    /// <summary>
    /// Submits the human decision for <paramref name="claimId"/>.
    /// <paramref name="decision"/> MUST be in <see cref="HumanDecisions"/>; throws <see cref="ArgumentException"/> otherwise.
    /// Sets CurrentDecision=decision, Submitted=true, SubmittedAt=now.
    /// No payout, no message, no claim-row mutation.
    /// </summary>
    Task<string> SubmitDecisionAsync(
        string claimId,
        string decision,
        string? notes,
        ActorContext actor,
        CancellationToken ct = default);
}
