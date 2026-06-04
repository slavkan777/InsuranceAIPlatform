using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;

/// <summary>
/// Local vector-like retrieval: embed the query, cosine-compare against each candidate's cached
/// embedding (rebuilt from text if the cache is empty), return the top-k. No external service.
/// </summary>
public sealed class RagRetrievalService : IRagRetrievalService
{
    private readonly IEmbeddingProvider _embed;

    public RagRetrievalService(IEmbeddingProvider embed) => _embed = embed;

    public IReadOnlyList<ScoredChunk> Rank(string query, IReadOnlyList<EvidenceChunk> candidates, int topK)
    {
        if (candidates is null || candidates.Count == 0 || topK <= 0)
            return Array.Empty<ScoredChunk>();

        float[] q = _embed.Embed(query ?? string.Empty);

        var scored = new List<ScoredChunk>(candidates.Count);
        foreach (var c in candidates)
        {
            float[] vec = EmbeddingCodec.FromJson(c.EmbeddingJson) ?? _embed.Embed(c.Text);
            double score = VectorMath.Cosine(q, vec);
            scored.Add(new ScoredChunk(c, score));
        }

        return scored
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Chunk.ChunkId, StringComparer.Ordinal) // deterministic tie-break
            .Take(topK)
            .ToList();
    }
}
