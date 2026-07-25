using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.Approval.Persistence;

/// <summary>
/// Idempotent seed for the Approval context.
/// CLM-1006 draft with 4 decision options, exactly as in InMemoryClaimReadService.
/// </summary>
public static class ApprovalSeeder
{
    public static async Task SeedAsync(ApprovalDbContext db, CancellationToken ct = default)
    {
        if (await db.ApprovalDrafts.AnyAsync(ct))
            return;

        var draft = new ApprovalDraft
        {
            ClaimId = SeedConstants.GoldenClaimId,
            CurrentDecision = null,
            Notes = null,
            Submitted = false,
            SubmittedAt = null,
            SavedAt = null,
            AiRecommendation = "Request additional documents",
            RecommendedPayout = 1800.00m,
            Options = new List<ApprovalDecisionOption>
            {
                new() { ClaimId = SeedConstants.GoldenClaimId, Key = "request",  Label = "Request additional documents",        Recommended = true,  Rationale = "AI recommended — request the rear bumper photo" },
                new() { ClaimId = SeedConstants.GoldenClaimId, Key = "approve",  Label = "Approve payout",                      Recommended = false, Rationale = "If the risks are acceptable after review" },
                new() { ClaimId = SeedConstants.GoldenClaimId, Key = "reject",   Label = "Reject the claim",                    Recommended = false, Rationale = "With a written justification" },
                new() { ClaimId = SeedConstants.GoldenClaimId, Key = "escalate", Label = "Escalate to the investigation unit",  Recommended = false, Rationale = "Escalation for a detailed investigation" },
            }
        };

        await db.ApprovalDrafts.AddAsync(draft, ct);
        await db.SaveChangesAsync(ct);
    }
}
