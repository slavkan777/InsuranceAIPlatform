using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Approval;

/// <summary>
/// Skeleton implementation of <see cref="IApprovalService"/>. Reports readiness only; holds no
/// drafts and performs no decisions yet. Write methods are no-ops — the DB-backed
/// <see cref="Persistence.PersistenceApprovalService"/> handles real writes.
/// </summary>
public sealed class ApprovalService : IApprovalService
{
    public string ServiceName => ServiceNames.Approval;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.Approval,
        ServiceReadinessStatus.Stub,
        "skeleton-v0.1",
        new[] { "approval-draft-read", "human-decision" });

    public Task<string> SaveDraftAsync(
        string claimId, string? currentDecision, string? notes,
        ActorContext actor, CancellationToken ct = default)
        => Task.FromResult(claimId); // no-op in skeleton

    public Task<string> SubmitDecisionAsync(
        string claimId, string decision, string? notes,
        ActorContext actor, CancellationToken ct = default)
    {
        if (!HumanDecisions.IsAllowed(decision))
            throw new ArgumentException($"Decision '{decision}' is not in the allowed human-decision set.", nameof(decision));
        return Task.FromResult(claimId); // no-op in skeleton
    }

    public Task<int> CreatePayoutSimulationAsync(
        string claimId, decimal amount, decimal deductible, string currency,
        string decisionSource, string? sourceAiRunId, string? notes,
        ActorContext actor, string correlationId, CancellationToken ct = default)
        => Task.FromResult(0); // no-op in skeleton — DB-backed service handles real writes

    public Task<string?> ConfirmPayoutSimulationAsync(
        int simulationId, ActorContext actor, CancellationToken ct = default)
        => Task.FromResult<string?>(null); // no-op in skeleton

    public Task<IReadOnlyList<PayoutSimulationSummary>> GetPayoutSimulationsAsync(
        string claimId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<PayoutSimulationSummary>>(Array.Empty<PayoutSimulationSummary>());
}
