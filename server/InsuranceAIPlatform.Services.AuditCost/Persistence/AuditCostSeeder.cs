using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.AuditCost.Persistence;

/// <summary>
/// Idempotent seed for the AuditCost context.
/// CLM-1006: 6 audit events, 4 cost rows, 1 token usage row — exact values from InMemoryClaimReadService.
/// </summary>
public static class AuditCostSeeder
{
    public static async Task SeedAsync(AuditCostDbContext db, CancellationToken ct = default)
    {
        if (await db.AuditEvents.AnyAsync(ct))
            return;

        var events = new List<AuditEvent>
        {
            new() { ClaimId = SeedConstants.GoldenClaimId, At = "14:05:12", Source = "AI Pipeline",    Message = "Запуск аналізу CLM-1006",     Severity = "OK"    },
            new() { ClaimId = SeedConstants.GoldenClaimId, At = "14:05:14", Source = "Doc Classifier", Message = "Класифікація 6 документів",   Severity = "OK"    },
            new() { ClaimId = SeedConstants.GoldenClaimId, At = "14:05:19", Source = "Field Extractor",Message = "Витягнуто 47 полів",           Severity = "OK"    },
            new() { ClaimId = SeedConstants.GoldenClaimId, At = "14:05:25", Source = "Risk Engine",    Message = "Ризик 82/100 — Високий",       Severity = "WARN"  },
            new() { ClaimId = SeedConstants.GoldenClaimId, At = "14:05:30", Source = "Recommender",    Message = "Рекомендація: запросити фото", Severity = "OK"    },
            new() { ClaimId = SeedConstants.GoldenClaimId, At = "14:05:31", Source = "Governance",     Message = "Авто-погодження заблоковано",  Severity = "BLOCK" },
        };

        var costs = new List<CostTrace>
        {
            new() { ClaimId = SeedConstants.GoldenClaimId, Category = "Витягування",  Amount = 0.0072m },
            new() { ClaimId = SeedConstants.GoldenClaimId, Category = "RAG / докази", Amount = 0.0058m },
            new() { ClaimId = SeedConstants.GoldenClaimId, Category = "Ризик",        Amount = 0.0029m },
            new() { ClaimId = SeedConstants.GoldenClaimId, Category = "Рекомендація", Amount = 0.0028m },
        };

        var tokens = new List<TokenUsageTrace>
        {
            new() { ClaimId = SeedConstants.GoldenClaimId, Tokens = 4261, Cost = 0.0187m },
        };

        await db.AuditEvents.AddRangeAsync(events, ct);
        await db.CostTraces.AddRangeAsync(costs, ct);
        await db.TokenUsageTraces.AddRangeAsync(tokens, ct);
        await db.SaveChangesAsync(ct);
    }
}
