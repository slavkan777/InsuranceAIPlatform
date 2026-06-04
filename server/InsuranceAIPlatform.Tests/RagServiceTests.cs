using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// End-to-end RAG service tests over EF InMemory (no SQL Server, no HTTP). Covers MECHANICAL
/// (endpoints' service layer responds), SEMANTIC (claim-scoped retrieval), PERSISTENCE (audit
/// trace survives a fresh read) and NEGATIVE (service-level cross-claim leakage guard).
/// </summary>
public class RagServiceTests
{
    private static readonly DeterministicEmbeddingProvider Embed = new(256);

    private static async Task<RagService> BuildSeededServiceAsync(string dbName)
    {
        var factory = new InMemoryAiAnalysisDbContextFactory(dbName);
        await using (var db = factory.CreateDbContext())
        {
            await RagSeeder.SeedAsync(db, Embed);
        }

        var retrieval = new RagRetrievalService(Embed);
        var generator = new MockGroundedAnswerGenerator();
        var source = new DbRagChunkSource(factory);
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, DefaultTopK = 4 };
        return new RagService(source, retrieval, generator, factory, options, Embed);
    }

    [Fact]
    public async Task Ask_returns_grounded_answer_and_persists_audit_trace()
    {
        var svc = await BuildSeededServiceAsync(nameof(Ask_returns_grounded_answer_and_persists_audit_trace));

        var answer = await svc.AskAsync("CLM-1006", "Яка франшиза за полісом?", RagUseCases.Coverage, "corr-1");

        Assert.NotEmpty(answer.RetrievedChunkIds);
        Assert.Equal("Mock", answer.ProviderMode);
        Assert.True(answer.AdvisoryOnly);
        Assert.All(answer.Citations, c => Assert.StartsWith("CLM-1006", c.ChunkId));

        // PERSISTENCE: the trace is readable after the write (fresh query through the service)
        var audit = await svc.GetAuditAsync("CLM-1006", 10);
        Assert.Contains(audit, t => t.TraceId == answer.TraceId);
    }

    [Fact]
    public async Task Ask_for_CLM1007_never_retrieves_CLM1006_evidence_service_level_leak_guard()
    {
        var svc = await BuildSeededServiceAsync(nameof(Ask_for_CLM1007_never_retrieves_CLM1006_evidence_service_level_leak_guard));

        var answer = await svc.AskAsync("CLM-1007", "Які пошкодження та документи?", RagUseCases.Summary, "corr-2");

        Assert.NotEmpty(answer.RetrievedChunkIds);
        Assert.All(answer.RetrievedChunkIds, id => Assert.StartsWith("CLM-1007", id));
        Assert.DoesNotContain(answer.RetrievedChunkIds, id => id.StartsWith("CLM-1006"));
        Assert.DoesNotContain(answer.Citations, c => c.ChunkId.StartsWith("CLM-1006"));
    }

    [Fact]
    public async Task EvidenceSearch_is_claim_scoped()
    {
        var svc = await BuildSeededServiceAsync(nameof(EvidenceSearch_is_claim_scoped));

        var hits = await svc.SearchEvidenceAsync("CLM-1006", "рахунок СТО бампер", 3);

        Assert.NotEmpty(hits);
        Assert.All(hits, h => Assert.StartsWith("CLM-1006", h.ChunkId));
    }

    [Fact]
    public async Task EvaluationQuestions_are_returned_for_claim()
    {
        var svc = await BuildSeededServiceAsync(nameof(EvaluationQuestions_are_returned_for_claim));

        var qs = await svc.GetEvaluationQuestionsAsync("CLM-1006");

        Assert.Equal(4, qs.Count); // Q-COVER/Q-MISS/Q-RISK/Q-SUMM for CLM-1006
        Assert.All(qs, q => Assert.Equal("CLM-1006", q.ClaimId));
    }
}

/// <summary>Minimal InMemory IDbContextFactory for tests (shares store by db name).</summary>
file sealed class InMemoryAiAnalysisDbContextFactory : IDbContextFactory<AiAnalysisDbContext>
{
    private readonly string _dbName;
    public InMemoryAiAnalysisDbContextFactory(string dbName) => _dbName = dbName;

    public AiAnalysisDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AiAnalysisDbContext>().UseInMemoryDatabase(_dbName).Options);
}
