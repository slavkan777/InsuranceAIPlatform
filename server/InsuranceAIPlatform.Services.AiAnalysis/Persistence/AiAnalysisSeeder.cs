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
                new() { Id = "f1", RunId = "run_8f3d2a7e", Category = "Documents",       Text = "Rear bumper photo is missing. 6 of 7 documents provided.", Severity = "warn" },
                new() { Id = "f2", RunId = "run_8f3d2a7e", Category = "Damage estimate", Text = "Estimate $2,720 exceeds the $1,970 benchmark by 38%.",            Severity = "warn" },
                new() { Id = "f3", RunId = "run_8f3d2a7e", Category = "Coverage",        Text = "The road accident is covered by Auto Comprehensive. The $500 deductible applies.", Severity = "ok" },
            },
            EvidenceReferences = new List<AiEvidenceReference>
            {
                new() { Id = "e1", RunId = "run_8f3d2a7e", Source = "Police report",     Note = "Road accident on 18.05.2026 in Springfield confirmed.", Confidence = 95 },
                new() { Id = "e2", RunId = "run_8f3d2a7e", Source = "Repair invoice",    Note = "Total $2,720. Breakdown: bumper $980, paint $740, bodywork $1,000.", Confidence = 87 },
            },
            RiskSignals = new List<AiRiskSignal>
            {
                new() { Id = "rs1", RunId = "run_8f3d2a7e", Label = "Repair amount above the expected range",     Weight = 25 },
                new() { Id = "rs2", RunId = "run_8f3d2a7e", Label = "Damage photo missing",                       Weight = 22 },
                new() { Id = "rs3", RunId = "run_8f3d2a7e", Label = "Discrepancies between driver statements",    Weight = 18 },
                new() { Id = "rs4", RunId = "run_8f3d2a7e", Label = "Confidence below the 85% threshold",         Weight = 9  },
            }
        };

        await db.AiAnalysisRuns.AddAsync(run, ct);
        await db.SaveChangesAsync(ct);
    }
}
