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
            AiRecommendation = "Запросити додаткові документи",
            RecommendedPayout = 1800.00m,
            Options = new List<ApprovalDecisionOption>
            {
                new() { ClaimId = SeedConstants.GoldenClaimId, Key = "request",  Label = "Запросити додаткові документи",       Recommended = true,  Rationale = "Рекомендовано AI — запросити фото заднього бампера" },
                new() { ClaimId = SeedConstants.GoldenClaimId, Key = "approve",  Label = "Затвердити виплату",                  Recommended = false, Rationale = "Якщо ризики прийнятні після перевірки" },
                new() { ClaimId = SeedConstants.GoldenClaimId, Key = "reject",   Label = "Відхилити заявку",                    Recommended = false, Rationale = "З обґрунтуванням відмови" },
                new() { ClaimId = SeedConstants.GoldenClaimId, Key = "escalate", Label = "Передати до відділу розслідування",   Recommended = false, Rationale = "Ескалація для детального розслідування" },
            }
        };

        await db.ApprovalDrafts.AddAsync(draft, ct);
        await db.SaveChangesAsync(ct);
    }
}
