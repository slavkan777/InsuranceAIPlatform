using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Runtime;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Infrastructure status tests: GetInfrastructureStatusAsync and ReindexClaimAsync.
/// Uses the same EF InMemory + RagSeeder pattern as RagServiceTests / RagSeederTests.
/// </summary>
public class RagInfrastructureTests
{
    private static readonly DeterministicEmbeddingProvider Embed = new(256);

    private static async Task<RagService> BuildSeededServiceAsync(string dbName)
    {
        var factory = RagInfrastructureTestHelper.MakeFactory(dbName);
        await using (var db = factory.CreateDbContext())
        {
            await RagSeeder.SeedAsync(db, Embed);
        }

        var retrieval  = new RagRetrievalService(Embed);
        var generator  = new MockGroundedAnswerGenerator();
        var source     = new DbRagChunkSource(factory);
        var options    = new RagOptions { EmbeddingDimensions = Embed.Dimensions, DefaultTopK = 4 };
        return new RagService(source, retrieval, generator, factory, options, Embed);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Test 1: GetInfrastructureStatus_reports_healthy_counts
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetInfrastructureStatus_reports_healthy_counts()
    {
        var svc = await BuildSeededServiceAsync(
            nameof(GetInfrastructureStatus_reports_healthy_counts));

        var status = await svc.GetInfrastructureStatusAsync("CLM-1006", "corr-infra-1");

        // SQL counts match the seeder's known totals
        Assert.Equal("healthy",  status.SqlSourceOfTruth.Status);
        Assert.Equal(8,          status.SqlSourceOfTruth.PolicyClauses);
        Assert.Equal(50,         status.SqlSourceOfTruth.EvidenceChunks);
        Assert.Equal(21,         status.SqlSourceOfTruth.EvaluationQuestions);

        // Index: all 13 CLM-1006 chunks are embedded after seeding
        Assert.Equal("healthy",  status.EvidenceMemoryIndex.Status);
        Assert.Equal(13,         status.EvidenceMemoryIndex.TotalChunks);
        Assert.Equal(13,         status.EvidenceMemoryIndex.EmbeddedChunks);
        Assert.Equal(13,         status.EvidenceMemoryIndex.EmbeddedChunks); // == TotalChunks

        // Runtime: disabled by default (LocalLlamaEnabled = false), reachability mechanically false
        Assert.Equal("disabled", status.LocalReasoningRuntime.Status);
        Assert.False(status.LocalReasoningRuntime.Enabled);
        Assert.False(status.LocalReasoningRuntime.Reachable);

        // Vector runtime: Qdrant disabled by default → in-process hash index is the backend
        Assert.Equal("disabled", status.VectorRuntime.Status);
        Assert.False(status.VectorRuntime.Enabled);
        Assert.Equal("in-memory-hash", status.VectorRuntime.Backend);
        Assert.False(status.VectorRuntime.Reachable);

        // Claim and correlation round-trip
        Assert.Equal("CLM-1006", status.ClaimId);
        Assert.Equal("corr-infra-1", status.CorrelationId);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Test 2: Reindex_is_idempotent_and_keeps_index_healthy
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Reindex_is_idempotent_and_keeps_index_healthy()
    {
        var dbName = nameof(Reindex_is_idempotent_and_keeps_index_healthy);
        var factory = RagInfrastructureTestHelper.MakeFactory(dbName);
        await using (var seedDb = factory.CreateDbContext())
        {
            await RagSeeder.SeedAsync(seedDb, Embed);
        }
        var retrieval  = new RagRetrievalService(Embed);
        var generator  = new MockGroundedAnswerGenerator();
        var source     = new DbRagChunkSource(factory);
        var options    = new RagOptions { EmbeddingDimensions = Embed.Dimensions, DefaultTopK = 4 };
        var svc        = new RagService(source, retrieval, generator, factory, options, Embed);

        // Capture original embedding for a known chunk
        string targetChunkId = "CLM-1006-police#0";
        string? originalEmbeddingJson;
        await using (var db = factory.CreateDbContext())
        {
            var chunk = await db.EvidenceChunks
                .AsNoTracking()
                .FirstAsync(c => c.ChunkId == targetChunkId);
            originalEmbeddingJson = chunk.EmbeddingJson;
        }

        Assert.NotNull(originalEmbeddingJson); // seeder must have embedded it

        // Run reindex (should be idempotent — deterministic embedding, same source text)
        var after = await svc.ReindexClaimAsync("CLM-1006", "corr-reindex-1");

        // Chunk count must not change
        Assert.Equal(13, after.EvidenceMemoryIndex.TotalChunks);
        Assert.Equal(13, after.EvidenceMemoryIndex.EmbeddedChunks);
        Assert.Equal("healthy", after.EvidenceMemoryIndex.Status);

        // Embedding must be byte-identical (deterministic)
        await using (var db = factory.CreateDbContext())
        {
            var chunk = await db.EvidenceChunks
                .AsNoTracking()
                .FirstAsync(c => c.ChunkId == targetChunkId);
            Assert.Equal(originalEmbeddingJson, chunk.EmbeddingJson);
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Test 3-7: runtime reachability is MECHANICALLY PROBED, not guessed from the flag
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>Deterministic in-memory probe — no network — for honest-status tests.</summary>
    private sealed class StubRuntimeProbe : IRagRuntimeProbe
    {
        private readonly bool _reachable;
        public StubRuntimeProbe(bool reachable) => _reachable = reachable;
        public Task<bool> IsReachableAsync(string endpoint, string? healthPath, CancellationToken ct = default) =>
            Task.FromResult(_reachable);
    }

    private static async Task<RagService> BuildSeededServiceAsync(string dbName, RagOptions options, IRagRuntimeProbe? probe)
    {
        var factory = RagInfrastructureTestHelper.MakeFactory(dbName);
        await using (var db = factory.CreateDbContext())
        {
            await RagSeeder.SeedAsync(db, Embed);
        }
        var retrieval = new RagRetrievalService(Embed);
        var generator = new MockGroundedAnswerGenerator();
        var source    = new DbRagChunkSource(factory);
        return new RagService(source, retrieval, generator, factory, options, Embed, probe);
    }

    [Fact]
    public async Task LlamaRuntime_enabled_and_reachable_reports_live_local()
    {
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, LocalLlamaEnabled = true };
        var svc = await BuildSeededServiceAsync(
            nameof(LlamaRuntime_enabled_and_reachable_reports_live_local), options, new StubRuntimeProbe(true));

        var status = await svc.GetInfrastructureStatusAsync("CLM-1006", "corr-live");

        Assert.Equal("live_local", status.LocalReasoningRuntime.Status);
        Assert.True(status.LocalReasoningRuntime.Enabled);
        Assert.True(status.LocalReasoningRuntime.Reachable);
    }

    [Fact]
    public async Task LlamaRuntime_enabled_but_unreachable_reports_skipped_not_available()
    {
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, LocalLlamaEnabled = true };
        var svc = await BuildSeededServiceAsync(
            nameof(LlamaRuntime_enabled_but_unreachable_reports_skipped_not_available), options, new StubRuntimeProbe(false));

        var status = await svc.GetInfrastructureStatusAsync("CLM-1006", "corr-skip");

        Assert.Equal("skipped_not_available", status.LocalReasoningRuntime.Status);
        Assert.True(status.LocalReasoningRuntime.Enabled);
        Assert.False(status.LocalReasoningRuntime.Reachable);
    }

    [Fact]
    public async Task QdrantVectorRuntime_reachable_without_serving_adapter_reports_in_memory_hash()
    {
        // Honesty invariant (QDRANT_RETRIEVAL_ADAPTER_V0.1): being probe-reachable is NOT enough to claim
        // backend=qdrant. With no router/client wired (this service is built without one), Qdrant is up but
        // does not actually serve retrieval — so the backend must honestly report in-memory-hash while the
        // runtime status still reflects reachability (live_local). See VectorRetrievalRouterTests for the
        // serving-adapter case that legitimately reports backend=qdrant.
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, QdrantEnabled = true };
        var svc = await BuildSeededServiceAsync(
            nameof(QdrantVectorRuntime_reachable_without_serving_adapter_reports_in_memory_hash), options, new StubRuntimeProbe(true));

        var status = await svc.GetInfrastructureStatusAsync("CLM-1006", "corr-qlive");

        Assert.Equal("live_local", status.VectorRuntime.Status);   // reachable
        Assert.True(status.VectorRuntime.Enabled);
        Assert.True(status.VectorRuntime.Reachable);
        Assert.Equal("in-memory-hash", status.VectorRuntime.Backend); // but NOT actually serving → honest fallback label
    }

    [Fact]
    public async Task QdrantVectorRuntime_enabled_but_unreachable_falls_back_to_in_memory_hash()
    {
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, QdrantEnabled = true };
        var svc = await BuildSeededServiceAsync(
            nameof(QdrantVectorRuntime_enabled_but_unreachable_falls_back_to_in_memory_hash), options, new StubRuntimeProbe(false));

        var status = await svc.GetInfrastructureStatusAsync("CLM-1006", "corr-qskip");

        Assert.Equal("skipped_not_available", status.VectorRuntime.Status);
        Assert.True(status.VectorRuntime.Enabled);
        Assert.False(status.VectorRuntime.Reachable);
        Assert.Equal("in-memory-hash", status.VectorRuntime.Backend);
    }

    [Fact]
    public async Task Disabled_runtime_does_not_probe_and_reports_disabled()
    {
        // Enabled=false → even a probe that WOULD return true must not flip status to live.
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, LocalLlamaEnabled = false, QdrantEnabled = false };
        var svc = await BuildSeededServiceAsync(
            nameof(Disabled_runtime_does_not_probe_and_reports_disabled), options, new StubRuntimeProbe(true));

        var status = await svc.GetInfrastructureStatusAsync("CLM-1006", "corr-disabled");

        Assert.Equal("disabled", status.LocalReasoningRuntime.Status);
        Assert.False(status.LocalReasoningRuntime.Reachable);
        Assert.Equal("disabled", status.VectorRuntime.Status);
        Assert.Equal("in-memory-hash", status.VectorRuntime.Backend);
        Assert.False(status.VectorRuntime.Reachable);
    }
}

/// <summary>
/// Factory helper for infrastructure tests. Uses a different class name from the file-scoped
/// InMemoryAiAnalysisDbContextFactory in RagServiceTests.cs to avoid any collision.
/// </summary>
internal static class RagInfrastructureTestHelper
{
    public static IDbContextFactory<AiAnalysisDbContext> MakeFactory(string dbName) =>
        new RagInfraDbContextFactory(dbName);

    private sealed class RagInfraDbContextFactory : IDbContextFactory<AiAnalysisDbContext>
    {
        private readonly string _dbName;
        public RagInfraDbContextFactory(string dbName) => _dbName = dbName;

        public AiAnalysisDbContext CreateDbContext() =>
            new(new DbContextOptionsBuilder<AiAnalysisDbContext>().UseInMemoryDatabase(_dbName).Options);
    }
}
