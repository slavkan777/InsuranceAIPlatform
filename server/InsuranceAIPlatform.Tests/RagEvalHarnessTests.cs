using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Evaluation harness over the seeded gold question set (corpus-agnostic — scales as the corpus grows).
/// Asserts aggregate recall@k of expected source chunks and that mustNotCite chunks never appear
/// (cross-claim leakage guard). This is the "ruler" that protects retrieval quality during refactors.
/// </summary>
public class RagEvalHarnessTests
{
    private static readonly DeterministicEmbeddingProvider Embed = new(256);
    private const int TopK = 4;
    private const double RecallThreshold = 0.6;

    private static AiAnalysisDbContext Build(string dbName) =>
        new(new DbContextOptionsBuilder<AiAnalysisDbContext>().UseInMemoryDatabase(dbName).Options);

    [Fact]
    public async Task Recall_at_k_meets_threshold_and_no_mustNotCite_leak()
    {
        await using var db = Build(nameof(Recall_at_k_meets_threshold_and_no_mustNotCite_leak));
        await RagSeeder.SeedAsync(db, Embed);

        var retrieval = new RagRetrievalService(Embed);
        var questions = await db.RagEvaluationQuestions.AsNoTracking().ToListAsync();
        Assert.NotEmpty(questions);

        int hit = 0, total = 0;
        foreach (var q in questions)
        {
            var claimChunks = await db.EvidenceChunks.AsNoTracking()
                .Where(c => c.ClaimId == q.ClaimId).ToListAsync();
            var retrievedIds = retrieval.Rank(q.Text, claimChunks, TopK)
                .Select(r => r.Chunk.ChunkId).ToHashSet();

            foreach (var expected in Split(q.ExpectedSourceChunkIdsCsv))
            {
                total++;
                if (retrievedIds.Contains(expected)) hit++;
            }

            // mustNotCite chunks (e.g. other-claim evidence) must never be retrieved.
            foreach (var forbidden in Split(q.MustNotCiteChunkIdsCsv))
                Assert.DoesNotContain(forbidden, retrievedIds);
        }

        double recall = total == 0 ? 1.0 : (double)hit / total;
        Assert.True(recall >= RecallThreshold, $"recall@{TopK} = {recall:P0} ({hit}/{total}) below {RecallThreshold:P0}");
    }

    private static string[] Split(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? Array.Empty<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
