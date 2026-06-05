using System.Diagnostics;
using System.Text.Json;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Runtime;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag;

/// <summary>
/// Local RAG orchestration. Retrieval-before-generation: fetch claim-scoped chunks, rank top-k,
/// generate a grounded advisory answer, persist an audit trace. Singleton + DbContextFactory
/// (same safe pattern as PersistenceAiAnalysisOrchestrator). No external/cloud call; synthetic only.
/// </summary>
public sealed class RagService : IRagService
{
    private readonly IRagChunkSource _chunks;
    private readonly IRagRetrievalService _retrieval;
    private readonly IGroundedAnswerGenerator _generator;
    private readonly IDbContextFactory<AiAnalysisDbContext> _factory;
    private readonly RagOptions _options;
    private readonly IEmbeddingProvider _embed;
    private readonly IRagRuntimeProbe? _probe;
    private readonly IVectorRetrievalRouter? _router;

    public RagService(
        IRagChunkSource chunks,
        IRagRetrievalService retrieval,
        IGroundedAnswerGenerator generator,
        IDbContextFactory<AiAnalysisDbContext> factory,
        RagOptions options,
        IEmbeddingProvider embed,
        IRagRuntimeProbe? probe = null,
        IVectorRetrievalRouter? router = null)
    {
        _chunks = chunks;
        _retrieval = retrieval;
        _generator = generator;
        _factory = factory;
        _options = options;
        _embed = embed;
        _probe = probe;
        _router = router;
    }

    /// <summary>
    /// Rank claim-scoped candidates via the vector router (Qdrant when serving, else the in-process
    /// index). When no router is wired (e.g. unit tests constructing the service directly) this degrades
    /// to the in-process deterministic index — identical behaviour to before the Qdrant seam existed.
    /// </summary>
    private async Task<IReadOnlyList<ScoredChunk>> RankAsync(
        string claimId, string query, IReadOnlyList<EvidenceChunk> candidates, int topK, CancellationToken ct)
    {
        if (_router is null)
            return _retrieval.Rank(query, candidates, topK);
        var outcome = await _router.RankAsync(claimId, query, candidates, topK, ct);
        return outcome.Hits;
    }

    public async Task<RagAnswer> AskAsync(string claimId, string question, string? useCase, string correlationId, CancellationToken ct = default)
    {
        string useCaseNorm = RagUseCases.Normalize(useCase);

        IReadOnlyList<EvidenceChunk> candidates = await _chunks.GetClaimChunksAsync(claimId, ct);

        var sw = Stopwatch.StartNew();
        IReadOnlyList<ScoredChunk> ranked = await RankAsync(claimId, question, candidates, _options.DefaultTopK, ct);
        sw.Stop();

        GroundedDraft draft = _generator.Generate(new GroundedRequest(claimId, useCaseNorm, question, ranked));

        var retrievedIds = ranked.Select(r => r.Chunk.ChunkId).ToList();
        string traceId = "ragtrc_" + Guid.NewGuid().ToString("N")[..8];
        DateTime now = DateTime.UtcNow;

        var trace = new RagAuditTrace
        {
            TraceId = traceId,
            ClaimId = claimId,
            UseCase = useCaseNorm,
            QueryText = question,
            RetrievedChunkIdsCsv = string.Join(",", retrievedIds),
            CitationsJson = JsonSerializer.Serialize(draft.Citations),
            AnswerText = draft.AnswerText,
            Confidence = draft.Confidence,
            ProviderMode = draft.ProviderMode,   // "LocalLlama" (live local model) or "Mock" (fallback) — never a cloud provider
            PromptTokens = draft.PromptTokens,
            CompletionTokens = draft.CompletionTokens,
            CostMicros = 0,                       // local/mock generation is free
            RetrievalMs = sw.ElapsedMilliseconds,
            AdvisoryOnly = true,
            CreatedAtUtc = now
        };

        await using (var db = await _factory.CreateDbContextAsync(ct))
        {
            db.RagAuditTraces.Add(trace);
            await db.SaveChangesAsync(ct);
        }

        return new RagAnswer(
            traceId, claimId, useCaseNorm, question,
            draft.AnswerText, draft.Confidence, draft.Citations, retrievedIds,
            draft.ProviderMode, draft.PromptTokens, draft.CompletionTokens,
            0, sw.ElapsedMilliseconds, true, now);
    }

    public async Task<IReadOnlyList<RagEvidenceHit>> SearchEvidenceAsync(string claimId, string query, int topK, CancellationToken ct = default)
    {
        if (topK <= 0) topK = _options.DefaultTopK;
        var candidates = await _chunks.GetClaimChunksAsync(claimId, ct);
        var ranked = await RankAsync(claimId, query, candidates, topK, ct);
        return ranked
            .Select(s => new RagEvidenceHit(
                s.Chunk.ChunkId, s.Chunk.DocumentId, s.Chunk.Kind, Snippet(s.Chunk.Text), s.Score))
            .ToList();
    }

    public async Task<IReadOnlyList<RagEvalQuestionView>> GetEvaluationQuestionsAsync(string claimId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.RagEvaluationQuestions
            .AsNoTracking()
            .Where(q => q.ClaimId == claimId)
            .OrderBy(q => q.QuestionId)
            .Select(q => new RagEvalQuestionView(q.QuestionId, q.ClaimId, q.UseCase, q.Text, q.Language))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<RagAuditView>> GetAuditAsync(string claimId, int limit, CancellationToken ct = default)
    {
        if (limit <= 0) limit = 20;
        await using var db = await _factory.CreateDbContextAsync(ct);
        var rows = await db.RagAuditTraces
            .AsNoTracking()
            .Where(t => t.ClaimId == claimId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(ct);

        return rows
            .Select(t => new RagAuditView(
                t.TraceId, t.ClaimId, t.UseCase, t.QueryText, t.AnswerText, t.Confidence,
                SplitCsv(t.RetrievedChunkIdsCsv), t.CostMicros, t.CreatedAtUtc))
            .ToList();
    }

    public async Task<IReadOnlyList<SimilarClaim>> FindSimilarClaimsAsync(string claimId, int topK, CancellationToken ct = default)
    {
        if (topK <= 0) topK = _options.DefaultTopK;
        // Build claim-level centroids from ALL chunks; the ranker returns claim-level metadata ONLY
        // (no raw evidence text of other claims ever leaves this boundary).
        var allChunks = await _chunks.GetAllChunksAsync(ct);
        return SimilarClaimsRanker.Rank(claimId, allChunks, topK);
    }

    public async Task<RagInfrastructureStatus> GetInfrastructureStatusAsync(
        string claimId, string correlationId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        int policyClauses     = await db.PolicyClauses.CountAsync(ct);
        int evidenceChunks    = await db.EvidenceChunks.CountAsync(ct);
        int embeddedChunks    = await db.EvidenceChunks.CountAsync(c => c.EmbeddingJson != null, ct);
        int evalQuestions     = await db.RagEvaluationQuestions.CountAsync(ct);
        int auditTraces       = await db.RagAuditTraces.CountAsync(ct);

        // Counts specific to this claim for the index status
        int claimTotal    = await db.EvidenceChunks.CountAsync(c => c.ClaimId == claimId, ct);
        int claimEmbedded = await db.EvidenceChunks.CountAsync(c => c.ClaimId == claimId && c.EmbeddingJson != null, ct);

        var sqlStatus = new RagSqlStatus(
            Status: "healthy",
            PolicyClauses: policyClauses,
            EvidenceChunks: evidenceChunks,
            EvaluationQuestions: evalQuestions,
            AuditTraces: auditTraces);

        string indexStatusStr = claimTotal == 0
            ? "empty"
            : claimEmbedded == claimTotal ? "healthy" : "degraded";

        var indexStatus = new RagIndexStatus(
            Status: indexStatusStr,
            EmbeddedChunks: claimEmbedded,
            TotalChunks: claimTotal,
            EmbeddingModel: _embed.ModelName,
            Dimensions: _options.EmbeddingDimensions);

        // Local reasoning runtime (Ollama) — reachability is MECHANICALLY PROBED, never guessed.
        // Status: disabled (seam off) | live_local (enabled + reachable) | skipped_not_available (enabled + unreachable).
        bool llamaEnabled = _options.LocalLlamaEnabled;
        bool llamaEndpointConfigured = !string.IsNullOrWhiteSpace(_options.LocalLlamaEndpoint);
        bool llamaReachable = llamaEnabled && llamaEndpointConfigured && _probe is not null
            && await _probe.IsReachableAsync(_options.LocalLlamaEndpoint, null, ct);
        string runtimeStatusStr = !llamaEnabled
            ? "disabled"
            : (llamaReachable ? "live_local" : "skipped_not_available");
        var runtimeStatus = new RagRuntimeStatus(
            Status: runtimeStatusStr,
            Enabled: llamaEnabled,
            Model: _options.LocalLlamaModel ?? string.Empty,
            EndpointConfigured: llamaEndpointConfigured,
            Reachable: llamaReachable);

        // Vector runtime (Qdrant) — also mechanically probed. The in-process deterministic index is
        // the safe fallback when Qdrant is disabled or unreachable, so vectors always work.
        bool qdrantEnabled = _options.QdrantEnabled;
        bool qdrantEndpointConfigured = !string.IsNullOrWhiteSpace(_options.QdrantEndpoint);
        bool qdrantReachable = qdrantEnabled && qdrantEndpointConfigured && _probe is not null
            && await _probe.IsReachableAsync(_options.QdrantEndpoint, null, ct);
        string vectorStatusStr = !qdrantEnabled
            ? "disabled"
            : (qdrantReachable ? "live_local" : "skipped_not_available");

        // HONEST backend: "qdrant" ONLY when a real Qdrant retrieval round-trip serves this claim right
        // now (ensure + upsert + search via the router) — NOT merely because the port is probe-reachable.
        // Reachable-but-not-serving (no router/client wired, or a round-trip error) honestly reports
        // in-memory-hash. This closes the semantic gap flagged in the previous gate.
        string vectorBackend = VectorBackends.InMemoryHash;
        if (qdrantEnabled && qdrantReachable && _router is not null)
        {
            var vectorCandidates = await _chunks.GetClaimChunksAsync(claimId, ct);
            vectorBackend = await _router.ResolveServingBackendAsync(claimId, vectorCandidates, ct);
        }

        var vectorStatus = new RagVectorRuntimeStatus(
            Status: vectorStatusStr,
            Enabled: qdrantEnabled,
            Backend: vectorBackend,
            EndpointConfigured: qdrantEndpointConfigured,
            Reachable: qdrantReachable);

        return new RagInfrastructureStatus(
            ClaimId: claimId,
            SqlSourceOfTruth: sqlStatus,
            EvidenceMemoryIndex: indexStatus,
            VectorRuntime: vectorStatus,
            LocalReasoningRuntime: runtimeStatus,
            GeneratedAtUtc: DateTime.UtcNow,
            CorrelationId: correlationId);
    }

    public async Task<RagInfrastructureStatus> ReindexClaimAsync(
        string claimId, string correlationId, CancellationToken ct = default)
    {
        await using (var db = await _factory.CreateDbContextAsync(ct))
        {
            var chunks = await db.EvidenceChunks
                .Where(c => c.ClaimId == claimId)
                .ToListAsync(ct);

            foreach (var chunk in chunks)
            {
                var vector = _embed.Embed(chunk.Text);
                chunk.EmbeddingJson  = EmbeddingCodec.ToJson(vector);
                chunk.EmbeddingModel = _embed.ModelName;
                chunk.EmbeddingDim   = _embed.Dimensions;
            }

            await db.SaveChangesAsync(ct);
        }

        return await GetInfrastructureStatusAsync(claimId, correlationId, ct);
    }

    private static string Snippet(string text)
    {
        text = (text ?? string.Empty).Trim();
        return text.Length <= 200 ? text : text[..200] + "…";
    }

    private static IReadOnlyList<string> SplitCsv(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? Array.Empty<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
