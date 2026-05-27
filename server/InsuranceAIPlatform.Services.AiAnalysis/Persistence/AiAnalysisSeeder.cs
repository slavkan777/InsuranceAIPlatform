using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.AiAnalysis.Persistence;

/// <summary>
/// Idempotent seed for the AiAnalysis context.
/// CLM-1006 run with ProviderMode="Disabled" — no real AI provider call.
/// Values match InMemoryClaimReadService exactly.
/// </summary>
public static class AiAnalysisSeeder
{
    public static async Task SeedAsync(AiAnalysisDbContext db, CancellationToken ct = default)
    {
        if (await db.AiAnalysisRuns.AnyAsync(ct))
            return;

        var run = new AiAnalysisRun
        {
            RunId = "run_8f3d2a7e",
            ClaimId = SeedConstants.GoldenClaimId,
            ProviderMode = "Disabled",  // NEVER a real provider
            ModelConfidence = 78,
            Tokens = 4261,
            Cost = 0.0187m,
            Findings = new List<AiFinding>
            {
                new() { Id = "f1", RunId = "run_8f3d2a7e", Category = "Документи",     Text = "Відсутнє фото заднього бампера. 6 з 7 документів надано.", Severity = "warn" },
                new() { Id = "f2", RunId = "run_8f3d2a7e", Category = "Оцінка збитку", Text = "Оцінка $2720 перевищує бенчмарк $1970 на 38%.",            Severity = "warn" },
                new() { Id = "f3", RunId = "run_8f3d2a7e", Category = "Покриття",      Text = "Подія ДТП підпадає під Auto Comprehensive. Франшиза $500 застосовна.", Severity = "ok" },
            },
            EvidenceReferences = new List<AiEvidenceReference>
            {
                new() { Id = "e1", RunId = "run_8f3d2a7e", Source = "Поліцейський звіт", Note = "Підтверджено факт ДТП 18.05.2026, Бориспіль.", Confidence = 95 },
                new() { Id = "e2", RunId = "run_8f3d2a7e", Source = "Рахунок СТО",       Note = "Загальна сума $2720. Деталізація: бампер $980, лак $740, кузов $1000.", Confidence = 87 },
            },
            RiskSignals = new List<AiRiskSignal>
            {
                new() { Id = "rs1", RunId = "run_8f3d2a7e", Label = "Сума ремонту вище очікуваного діапазону",   Weight = 25 },
                new() { Id = "rs2", RunId = "run_8f3d2a7e", Label = "Відсутнє фото пошкодження",                 Weight = 22 },
                new() { Id = "rs3", RunId = "run_8f3d2a7e", Label = "Розбіжності у поясненнях водіїв",           Weight = 18 },
                new() { Id = "rs4", RunId = "run_8f3d2a7e", Label = "Confidence нижче порогу 85%",               Weight = 9  },
            }
        };

        await db.AiAnalysisRuns.AddAsync(run, ct);
        await db.SaveChangesAsync(ct);
    }
}
