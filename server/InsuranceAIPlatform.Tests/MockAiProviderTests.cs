using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Providers;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Tests for MockAiProvider determinism.
/// Verifies: CLM-1006 golden shape; generic stub for other claims; determinism across calls.
/// </summary>
public class MockAiProviderTests
{
    private static readonly MockAiProvider Provider = new();

    // -----------------------------------------------------------------------
    // (P1) CLM-1006 returns golden deterministic output
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CLM1006_returns_golden_deterministic_output()
    {
        var request = new AiAnalysisRequest(SeedConstants.GoldenClaimId, "corr-1", "actor-1");
        var output  = await Provider.AnalyzeAsync(request);

        Assert.Equal("local-mock-v0.1", output.ModelName);
        Assert.Equal(78, output.ConfidenceScore);
        Assert.Equal(4261, output.Tokens);
        Assert.Equal(0.0187m, output.Cost);

        // 3 findings, 2 evidence refs, 4 risk signals (matching seeder)
        Assert.Equal(3, output.Findings.Count);
        Assert.Equal(2, output.Evidence.Count);
        Assert.Equal(4, output.Risks.Count);

        // Specific finding IDs from seeder
        Assert.Contains(output.Findings, f => f.Id == "f1" && f.Severity == "warn");
        Assert.Contains(output.Findings, f => f.Id == "f2" && f.Severity == "warn");
        Assert.Contains(output.Findings, f => f.Id == "f3" && f.Severity == "ok");

        // Evidence IDs
        Assert.Contains(output.Evidence, e => e.Id == "e1" && e.Confidence == 95);
        Assert.Contains(output.Evidence, e => e.Id == "e2" && e.Confidence == 87);

        // Risk IDs and weights
        Assert.Contains(output.Risks, r => r.Id == "rs1" && r.Weight == 25);
        Assert.Contains(output.Risks, r => r.Id == "rs2" && r.Weight == 22);
        Assert.Contains(output.Risks, r => r.Id == "rs3" && r.Weight == 18);
        Assert.Contains(output.Risks, r => r.Id == "rs4" && r.Weight == 9);
    }

    // -----------------------------------------------------------------------
    // (P2) Other claimId returns generic safe stub (1 finding, 0 evidence, 1 risk)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Other_claimId_returns_generic_safe_stub()
    {
        var request = new AiAnalysisRequest("CLM-9999", "corr-2", "actor-2");
        var output  = await Provider.AnalyzeAsync(request);

        Assert.Equal("local-mock-v0.1", output.ModelName);
        Assert.Equal(60, output.ConfidenceScore);
        Assert.Equal(1500, output.Tokens);
        Assert.Equal(0.0070m, output.Cost);

        Assert.Single(output.Findings);
        Assert.Empty(output.Evidence);
        Assert.Single(output.Risks);

        // Summary should not contain forbidden authority language
        Assert.DoesNotContain("approve payout", output.SummaryText.ToLowerInvariant());
        Assert.DoesNotContain("reject claim", output.SummaryText.ToLowerInvariant());
    }

    // -----------------------------------------------------------------------
    // (P3) Same claimId across two calls returns same shape (deterministic)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Same_claimId_across_two_calls_returns_same_shape()
    {
        var request1 = new AiAnalysisRequest(SeedConstants.GoldenClaimId, "corr-3a", "actor-3");
        var request2 = new AiAnalysisRequest(SeedConstants.GoldenClaimId, "corr-3b", "actor-3");

        var out1 = await Provider.AnalyzeAsync(request1);
        var out2 = await Provider.AnalyzeAsync(request2);

        Assert.Equal(out1.ModelName, out2.ModelName);
        Assert.Equal(out1.ConfidenceScore, out2.ConfidenceScore);
        Assert.Equal(out1.Tokens, out2.Tokens);
        Assert.Equal(out1.Cost, out2.Cost);
        Assert.Equal(out1.Findings.Count, out2.Findings.Count);
        Assert.Equal(out1.Evidence.Count, out2.Evidence.Count);
        Assert.Equal(out1.Risks.Count, out2.Risks.Count);
    }
}
