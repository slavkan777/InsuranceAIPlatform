using InsuranceAIPlatform.Services.AiAnalysis.Rag;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Runtime;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Tests for the Qdrant retrieval adapter (QDRANT_RETRIEVAL_ADAPTER_V0.1). Proves:
///  - enabled + a serving client → retrieval is served by Qdrant (backend=qdrant);
///  - disabled OR a Qdrant outage → silent fallback to the in-process index (backend=in-memory-hash);
///  - the claim filter + map-back is a real cross-claim leakage guard;
///  - the infrastructure-status backend label is honest — never "qdrant" unless Qdrant actually served.
/// The HTTP boundary is faked; the REAL <see cref="VectorRetrievalRouter"/> logic is under test.
/// </summary>
public class VectorRetrievalRouterTests
{
    private static readonly DeterministicEmbeddingProvider Embed = new(256);

    private static EvidenceChunk Chunk(string chunkId, string claimId, string text) =>
        new() { ChunkId = chunkId, ClaimId = claimId, DocumentId = "DOC", Kind = "statement", Text = text };

    private static IReadOnlyList<EvidenceChunk> SampleCandidates() => new[]
    {
        Chunk("CLM-1006#0", "CLM-1006", "water damage to the kitchen ceiling after a burst pipe"),
        Chunk("CLM-1006#1", "CLM-1006", "invoice for plumbing repair and drywall replacement"),
        Chunk("CLM-1006#2", "CLM-1006", "policy clause covering accidental water damage"),
    };

    // ── pure router unit tests ───────────────────────────────────────────────

    [Fact]
    public async Task Disabled_seam_uses_in_memory_and_never_touches_qdrant()
    {
        var fake = new FakeQdrantVectorClient();
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, QdrantEnabled = false };
        var router = new VectorRetrievalRouter(new RagRetrievalService(Embed), Embed, options, fake);

        var outcome = await router.RankAsync("CLM-1006", "burst pipe water damage", SampleCandidates(), 3);

        Assert.Equal(VectorBackends.InMemoryHash, outcome.Backend);
        Assert.NotEmpty(outcome.Hits);
        Assert.Equal(0, fake.EnsureCalls);   // disabled seam must not contact Qdrant at all
        Assert.Equal(0, fake.SearchCalls);
    }

    [Fact]
    public async Task Enabled_with_serving_client_uses_qdrant_backend()
    {
        var fake = new FakeQdrantVectorClient();
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, QdrantEnabled = true };
        var router = new VectorRetrievalRouter(new RagRetrievalService(Embed), Embed, options, fake);

        var candidates = SampleCandidates();
        var outcome = await router.RankAsync("CLM-1006", "burst pipe water damage", candidates, 3);

        Assert.Equal(VectorBackends.Qdrant, outcome.Backend);
        Assert.True(fake.UpsertedPoints >= candidates.Count); // claim chunks were upserted into Qdrant
        Assert.Equal(1, fake.SearchCalls);
        Assert.NotEmpty(outcome.Hits);
        // Every hit maps back to a candidate (no phantom chunks).
        Assert.All(outcome.Hits, h => Assert.Contains(candidates, c => c.ChunkId == h.Chunk.ChunkId));
    }

    [Fact]
    public async Task Enabled_but_qdrant_outage_falls_back_to_in_memory_hash()
    {
        var fake = new FakeQdrantVectorClient(outage: true);
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, QdrantEnabled = true };
        var router = new VectorRetrievalRouter(new RagRetrievalService(Embed), Embed, options, fake);

        var outcome = await router.RankAsync("CLM-1006", "burst pipe water damage", SampleCandidates(), 3);

        // The outage must NOT bubble up — retrieval still works via the in-process index.
        Assert.Equal(VectorBackends.InMemoryHash, outcome.Backend);
        Assert.NotEmpty(outcome.Hits);
        Assert.True(fake.EnsureCalls >= 1); // it tried Qdrant first…
    }

    [Fact]
    public async Task No_client_registered_falls_back_to_in_memory_hash()
    {
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, QdrantEnabled = true };
        var router = new VectorRetrievalRouter(new RagRetrievalService(Embed), Embed, options, qdrant: null);

        var outcome = await router.RankAsync("CLM-1006", "burst pipe water damage", SampleCandidates(), 3);

        Assert.Equal(VectorBackends.InMemoryHash, outcome.Backend);
        Assert.NotEmpty(outcome.Hits);
    }

    [Fact]
    public async Task Qdrant_hit_for_a_foreign_chunk_is_dropped_by_the_map_back_leakage_guard()
    {
        // Simulate Qdrant returning a stale/foreign chunk id that is NOT in this claim's candidate set.
        var forced = new List<QdrantSearchHit>
        {
            new("CLM-1006#0", 0.99),
            new("CLM-9999#0", 0.98), // foreign — must be dropped
        };
        var fake = new FakeQdrantVectorClient(forcedHits: forced);
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, QdrantEnabled = true };
        var router = new VectorRetrievalRouter(new RagRetrievalService(Embed), Embed, options, fake);

        var outcome = await router.RankAsync("CLM-1006", "anything", SampleCandidates(), 5);

        Assert.Equal(VectorBackends.Qdrant, outcome.Backend);
        Assert.Contains(outcome.Hits, h => h.Chunk.ChunkId == "CLM-1006#0");
        Assert.DoesNotContain(outcome.Hits, h => h.Chunk.ChunkId == "CLM-9999#0"); // leakage guard #2
    }

    [Fact]
    public async Task ResolveServingBackend_reports_qdrant_only_when_round_trip_serves()
    {
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, QdrantEnabled = true };

        var serving = new VectorRetrievalRouter(new RagRetrievalService(Embed), Embed, options, new FakeQdrantVectorClient());
        Assert.Equal(VectorBackends.Qdrant, await serving.ResolveServingBackendAsync("CLM-1006", SampleCandidates()));

        var outage = new VectorRetrievalRouter(new RagRetrievalService(Embed), Embed, options, new FakeQdrantVectorClient(outage: true));
        Assert.Equal(VectorBackends.InMemoryHash, await outage.ResolveServingBackendAsync("CLM-1006", SampleCandidates()));

        var disabled = new VectorRetrievalRouter(new RagRetrievalService(Embed), Embed,
            new RagOptions { EmbeddingDimensions = Embed.Dimensions, QdrantEnabled = false }, new FakeQdrantVectorClient());
        Assert.Equal(VectorBackends.InMemoryHash, await disabled.ResolveServingBackendAsync("CLM-1006", SampleCandidates()));
    }

    // ── RagService integration: infrastructure-status backend honesty ─────────

    [Fact]
    public async Task Infrastructure_with_serving_qdrant_adapter_reports_qdrant_backend()
    {
        var status = await BuildStatusAsync(
            nameof(Infrastructure_with_serving_qdrant_adapter_reports_qdrant_backend),
            new FakeQdrantVectorClient());

        Assert.Equal("live_local", status.VectorRuntime.Status);
        Assert.True(status.VectorRuntime.Reachable);
        Assert.Equal(VectorBackends.Qdrant, status.VectorRuntime.Backend); // legitimately serving → qdrant
    }

    [Fact]
    public async Task Infrastructure_with_qdrant_outage_reports_in_memory_hash_not_a_fake_qdrant_label()
    {
        var status = await BuildStatusAsync(
            nameof(Infrastructure_with_qdrant_outage_reports_in_memory_hash_not_a_fake_qdrant_label),
            new FakeQdrantVectorClient(outage: true));

        // Reachable (probe true) but the retrieval round-trip failed → must NOT claim qdrant.
        Assert.Equal("live_local", status.VectorRuntime.Status);
        Assert.True(status.VectorRuntime.Reachable);
        Assert.Equal(VectorBackends.InMemoryHash, status.VectorRuntime.Backend);
    }

    [Fact]
    public async Task AskAsync_with_serving_qdrant_adapter_drives_retrieval_through_qdrant()
    {
        var factory = RagInfrastructureTestHelper.MakeFactory(nameof(AskAsync_with_serving_qdrant_adapter_drives_retrieval_through_qdrant));
        await using (var db = factory.CreateDbContext())
            await RagSeeder.SeedAsync(db, Embed);

        var fake = new FakeQdrantVectorClient();
        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, DefaultTopK = 4, QdrantEnabled = true };
        var router = new VectorRetrievalRouter(new RagRetrievalService(Embed), Embed, options, fake);
        var svc = new RagService(new DbRagChunkSource(factory), new RagRetrievalService(Embed),
            new MockGroundedAnswerGenerator(), factory, options, Embed, new AlwaysReachableProbe(), router);

        var answer = await svc.AskAsync("CLM-1006", "is the water damage covered?", "coverage", "corr-ask-qdrant");

        Assert.NotNull(answer);
        Assert.True(fake.UpsertedPoints > 0); // the claim's chunks were upserted into Qdrant for retrieval
        Assert.True(fake.SearchCalls > 0);    // and retrieval queried Qdrant, not just the in-process index
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static async Task<RagInfrastructureStatus> BuildStatusAsync(string dbName, IQdrantVectorClient client)
    {
        var factory = RagInfrastructureTestHelper.MakeFactory(dbName);
        await using (var db = factory.CreateDbContext())
            await RagSeeder.SeedAsync(db, Embed);

        var options = new RagOptions { EmbeddingDimensions = Embed.Dimensions, DefaultTopK = 4, QdrantEnabled = true };
        var router = new VectorRetrievalRouter(new RagRetrievalService(Embed), Embed, options, client);
        var svc = new RagService(new DbRagChunkSource(factory), new RagRetrievalService(Embed),
            new MockGroundedAnswerGenerator(), factory, options, Embed, new AlwaysReachableProbe(), router);

        return await svc.GetInfrastructureStatusAsync("CLM-1006", "corr-int");
    }

    private sealed class AlwaysReachableProbe : IRagRuntimeProbe
    {
        public Task<bool> IsReachableAsync(string endpoint, string? healthPath, CancellationToken ct = default) =>
            Task.FromResult(true);
    }
}

/// <summary>
/// In-memory fake of <see cref="IQdrantVectorClient"/> for tests. Models a claim-filtered vector store,
/// an outage (every call throws), and a "forced hits" mode used to verify the router's map-back guard.
/// </summary>
internal sealed class FakeQdrantVectorClient : IQdrantVectorClient
{
    private readonly bool _outage;
    private readonly IReadOnlyList<QdrantSearchHit>? _forcedHits;
    private readonly List<QdrantUpsertPoint> _store = new();

    public int EnsureCalls { get; private set; }
    public int UpsertedPoints { get; private set; }
    public int SearchCalls { get; private set; }

    public FakeQdrantVectorClient(bool outage = false, IReadOnlyList<QdrantSearchHit>? forcedHits = null)
    {
        _outage = outage;
        _forcedHits = forcedHits;
    }

    public Task EnsureCollectionAsync(int dimensions, CancellationToken ct = default)
    {
        EnsureCalls++;
        if (_outage) throw new InvalidOperationException("simulated Qdrant outage (EnsureCollection)");
        return Task.CompletedTask;
    }

    public Task UpsertAsync(IReadOnlyList<QdrantUpsertPoint> points, CancellationToken ct = default)
    {
        if (_outage) throw new InvalidOperationException("simulated Qdrant outage (Upsert)");
        UpsertedPoints += points.Count;
        _store.AddRange(points);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<QdrantSearchHit>> SearchAsync(string claimId, IReadOnlyList<float> queryVector, int topK, CancellationToken ct = default)
    {
        SearchCalls++;
        if (_outage) throw new InvalidOperationException("simulated Qdrant outage (Search)");
        if (_forcedHits is not null)
            return Task.FromResult<IReadOnlyList<QdrantSearchHit>>(_forcedHits.Take(topK).ToList());

        // Simulate Qdrant's claim-filtered search: only this claim's stored points, score by insertion order.
        var hits = _store
            .Where(p => p.ClaimId == claimId)
            .Select((p, i) => new QdrantSearchHit(p.ChunkId, 1.0 - 0.001 * i))
            .Take(topK)
            .ToList();
        return Task.FromResult<IReadOnlyList<QdrantSearchHit>>(hits);
    }
}
