using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.Approval.Persistence;

/// <summary>
/// DB-backed implementation of <see cref="IApprovalService"/> (singleton-safe via IDbContextFactory).
/// Upserts approval drafts in <see cref="ApprovalDbContext"/>.
/// No payout, no customer messaging, no AI call.
/// </summary>
public sealed class PersistenceApprovalService : IApprovalService
{
    private readonly IDbContextFactory<ApprovalDbContext> _factory;
    private readonly IClock _clock;

    public PersistenceApprovalService(IDbContextFactory<ApprovalDbContext> factory, IClock clock)
    {
        _factory = factory;
        _clock   = clock;
    }

    public string ServiceName => ServiceNames.Approval;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.Approval,
        ServiceReadinessStatus.Ready,
        "persistence-v0.1",
        new[] { "approval-draft-read", "approval-draft-write", "human-decision" });

    // -----------------------------------------------------------------------
    // SaveDraftAsync — upsert draft; Submitted stays false
    // -----------------------------------------------------------------------

    public async Task<string> SaveDraftAsync(
        string claimId,
        string? currentDecision,
        string? notes,
        ActorContext actor,
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var draft = await db.ApprovalDrafts
            .FirstOrDefaultAsync(d => d.ClaimId == claimId, ct);

        if (draft is null)
        {
            draft = new ApprovalDraft
            {
                ClaimId          = claimId,
                Submitted        = false,
                AiRecommendation = string.Empty,
                RecommendedPayout= 0m,
            };
            db.ApprovalDrafts.Add(draft);
        }

        if (currentDecision is not null)
            draft.CurrentDecision = currentDecision;
        if (notes is not null)
            draft.Notes = notes;
        draft.SavedAt = _clock.UtcNow;
        // Submitted stays unchanged (must remain false for draft saves)

        await db.SaveChangesAsync(ct);
        return claimId;
    }

    // -----------------------------------------------------------------------
    // SubmitDecisionAsync — validates decision, sets Submitted=true
    // -----------------------------------------------------------------------

    public async Task<string> SubmitDecisionAsync(
        string claimId,
        string decision,
        string? notes,
        ActorContext actor,
        CancellationToken ct = default)
    {
        if (!HumanDecisions.IsAllowed(decision))
            throw new ArgumentException(
                $"Decision '{decision}' is not in the allowed human-decision set. " +
                $"Allowed: ApproveForReview, RejectForReview, NeedsMoreInformation, RequestDocuments.",
                nameof(decision));

        await using var db = await _factory.CreateDbContextAsync(ct);

        var draft = await db.ApprovalDrafts
            .FirstOrDefaultAsync(d => d.ClaimId == claimId, ct);

        if (draft is null)
        {
            draft = new ApprovalDraft
            {
                ClaimId          = claimId,
                AiRecommendation = string.Empty,
                RecommendedPayout= 0m,
            };
            db.ApprovalDrafts.Add(draft);
        }

        draft.CurrentDecision = decision;
        if (notes is not null)
            draft.Notes = notes;
        draft.Submitted    = true;
        draft.SubmittedAt  = _clock.UtcNow;
        draft.SavedAt      = _clock.UtcNow;

        await db.SaveChangesAsync(ct);
        return claimId;
    }
}
