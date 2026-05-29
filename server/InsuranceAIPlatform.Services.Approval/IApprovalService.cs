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

    /// <summary>
    /// Persists a DB-only payout simulation record for <paramref name="claimId"/>.
    /// NEVER performs a real money transfer; NEVER sends external messages.
    /// SimulationOnly=true is hard-set at row construction. Status starts at
    /// "DraftSimulated" and may be advanced to "Simulated" later by a follow-up
    /// confirmation call. Returns the new simulation's Id.
    /// </summary>
    Task<int> CreatePayoutSimulationAsync(
        string claimId,
        decimal amount,
        decimal deductible,
        string currency,
        string decisionSource,
        string? sourceAiRunId,
        string? notes,
        ActorContext actor,
        string correlationId,
        CancellationToken ct = default);

    /// <summary>
    /// Advances an existing payout simulation from "DraftSimulated" to "Simulated".
    /// Still DB-only; NO real transfer. Returns the updated row's current status,
    /// or null if the id does not exist.
    /// </summary>
    Task<string?> ConfirmPayoutSimulationAsync(
        int simulationId,
        ActorContext actor,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the payout simulations for a claim, ordered by CreatedAtUtc DESC.
    /// Used by the BFF GET endpoint and tests.
    /// </summary>
    Task<IReadOnlyList<PayoutSimulationSummary>> GetPayoutSimulationsAsync(
        string claimId,
        CancellationToken ct = default);
}

/// <summary>Read-only DTO surface for payout simulations (no schema leak to controllers/tests).</summary>
public sealed record PayoutSimulationSummary(
    int Id,
    string ClaimId,
    string Status,
    decimal Amount,
    decimal Deductible,
    decimal NetPayoutAmount,
    string Currency,
    string DecisionSource,
    string DecisionActor,
    string? SourceAiRunId,
    string? Notes,
    string CorrelationId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ConfirmedAtUtc,
    bool SimulationOnly);
