using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Cross-claim similar-claims search tests. Verifies it ranks OTHER claims, excludes the target
/// itself, surfaces matching evidence categories, and (by type) exposes no raw evidence text.
/// </summary>
public class RagSimilarClaimsTests
{
    private static readonly DeterministicEmbeddingProvider Embed = new(256);

    private static async Task<List<EvidenceChunk>> SeedAndReadAllAsync(string dbName)
    {
        var options = new DbContextOptionsBuilder<AiAnalysisDbContext>().UseInMemoryDatabase(dbName).Options;
        await using var db = new AiAnalysisDbContext(options);
        await RagSeeder.SeedAsync(db, Embed);
        return await db.EvidenceChunks.AsNoTracking().ToListAsync();
    }

    [Fact]
    public async Task SimilarClaims_excludes_self_and_returns_other_claims()
    {
        var all = await SeedAndReadAllAsync(nameof(SimilarClaims_excludes_self_and_returns_other_claims));

        var similar = SimilarClaimsRanker.Rank("CLM-1006", all, 5);

        Assert.NotEmpty(similar);
        Assert.DoesNotContain(similar, s => s.ClaimId == "CLM-1006");          // never self
        Assert.All(similar, s => Assert.InRange(s.Score, -1.0, 1.0));
        // results are ordered by score descending
        for (int i = 1; i < similar.Count; i++)
            Assert.True(similar[i - 1].Score >= similar[i].Score);
    }

    [Fact]
    public async Task SimilarClaims_reports_shared_evidence_categories()
    {
        var all = await SeedAndReadAllAsync(nameof(SimilarClaims_reports_shared_evidence_categories));

        var similar = SimilarClaimsRanker.Rank("CLM-1006", all, 5);

        // CLM-1006 and CLM-1008 both have application + police + invoice kinds → shared categories present.
        var clm1008 = similar.FirstOrDefault(s => s.ClaimId == "CLM-1008");
        Assert.NotNull(clm1008);
        Assert.Contains("invoice", clm1008!.MatchingCategories);
        Assert.False(string.IsNullOrWhiteSpace(clm1008.Reason));
    }

    [Fact]
    public async Task SimilarClaims_unknown_target_returns_empty()
    {
        var all = await SeedAndReadAllAsync(nameof(SimilarClaims_unknown_target_returns_empty));
        var similar = SimilarClaimsRanker.Rank("CLM-9999", all, 5);
        Assert.Empty(similar);
    }
}
