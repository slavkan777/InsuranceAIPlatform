using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// RAG seed tests on the EF InMemory provider (no SQL Server). Covers PERSISTENCE (seed survives a
/// fresh read), idempotency, and the data-level cross-claim leakage guard (NEGATIVE).
/// </summary>
public class RagSeederTests
{
    private static readonly DeterministicEmbeddingProvider Embed = new(256);

    private static AiAnalysisDbContext Build(string dbName) =>
        new(new DbContextOptionsBuilder<AiAnalysisDbContext>().UseInMemoryDatabase(dbName).Options);

    [Fact]
    public async Task Seed_produces_expected_counts()
    {
        await using var db = Build(nameof(Seed_produces_expected_counts));
        await RagSeeder.SeedAsync(db, Embed);

        Assert.Equal(8, await db.PolicyClauses.CountAsync());
        Assert.Equal(50, await db.EvidenceChunks.CountAsync());          // 13 (CLM-1006) + 6 (CLM-1007) + 6 (CLM-1008) + 7 (CLM-1009) + 10 (CLM-1010) + 8 (CLM-1011)
        Assert.Equal(13, await db.EvidenceChunks.CountAsync(c => c.ClaimId == "CLM-1006"));
        Assert.Equal(21, await db.RagEvaluationQuestions.CountAsync());
    }

    [Fact]
    public async Task Seed_is_idempotent()
    {
        await using var db = Build(nameof(Seed_is_idempotent));
        await RagSeeder.SeedAsync(db, Embed);
        await RagSeeder.SeedAsync(db, Embed); // second call must not duplicate

        Assert.Equal(50, await db.EvidenceChunks.CountAsync());
        Assert.Equal(8, await db.PolicyClauses.CountAsync());
    }

    [Fact]
    public async Task Every_chunk_has_a_cached_embedding()
    {
        await using var db = Build(nameof(Every_chunk_has_a_cached_embedding));
        await RagSeeder.SeedAsync(db, Embed);

        var missing = await db.EvidenceChunks
            .CountAsync(c => c.EmbeddingJson == null || c.EmbeddingModel == null);
        Assert.Equal(0, missing);
    }

    [Fact]
    public async Task CLM1007_chunks_never_include_CLM1006_evidence_data_level_leak_guard()
    {
        await using var db = Build(nameof(CLM1007_chunks_never_include_CLM1006_evidence_data_level_leak_guard));
        await RagSeeder.SeedAsync(db, Embed);

        var clm1007 = await db.EvidenceChunks.Where(c => c.ClaimId == "CLM-1007").ToListAsync();

        Assert.NotEmpty(clm1007);
        Assert.All(clm1007, c => Assert.StartsWith("CLM-1007", c.ChunkId));
        Assert.DoesNotContain(clm1007, c => c.ClaimId == "CLM-1006");
    }

    [Fact]
    public async Task Every_eval_question_expected_source_chunk_exists()
    {
        await using var db = Build(nameof(Every_eval_question_expected_source_chunk_exists));
        await RagSeeder.SeedAsync(db, Embed);

        var chunkIds = (await db.EvidenceChunks.Select(c => c.ChunkId).ToListAsync()).ToHashSet();
        var questions = await db.RagEvaluationQuestions.ToListAsync();

        foreach (var q in questions)
        {
            foreach (var expected in q.ExpectedSourceChunkIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                Assert.Contains(expected, chunkIds);
        }
    }
}
