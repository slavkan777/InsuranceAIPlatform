using InsuranceAIPlatform.Services.AiAnalysis.Rag.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;

/// <summary>
/// Cross-claim similarity over claim-level centroids (the mean of a claim's cached chunk embeddings).
/// SAFETY: this returns ONLY claim id / score / reason / matching categories — it never returns
/// another claim's raw evidence text. Per-claim ask/evidence retrieval stays strictly claim-scoped;
/// this is the one deliberately cross-claim path, and it exposes only claim-level metadata.
/// Pure and deterministic (operates on cached embeddings) → unit-testable without a DB.
/// </summary>
public static class SimilarClaimsRanker
{
    public static IReadOnlyList<SimilarClaim> Rank(string targetClaimId, IReadOnlyList<EvidenceChunk> allChunks, int topK)
    {
        if (string.IsNullOrWhiteSpace(targetClaimId) || allChunks is null || allChunks.Count == 0 || topK <= 0)
            return Array.Empty<SimilarClaim>();

        // Group by claim; build a normalized centroid + the set of evidence kinds per claim.
        var byClaim = allChunks
            .GroupBy(c => c.ClaimId)
            .ToDictionary(g => g.Key, g => new ClaimProfile(
                Centroid: Centroid(g),
                Kinds: g.Select(c => c.Kind).Where(k => !string.IsNullOrWhiteSpace(k)).Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase)));

        if (!byClaim.TryGetValue(targetClaimId, out var target) || target.Centroid.Length == 0)
            return Array.Empty<SimilarClaim>();

        var results = new List<SimilarClaim>();
        foreach (var (claimId, profile) in byClaim)
        {
            if (string.Equals(claimId, targetClaimId, StringComparison.OrdinalIgnoreCase)) continue; // never self
            if (profile.Centroid.Length == 0) continue;

            double score = VectorMath.Cosine(target.Centroid, profile.Centroid);
            var shared = target.Kinds.Intersect(profile.Kinds, StringComparer.OrdinalIgnoreCase)
                .OrderBy(k => k, StringComparer.Ordinal).ToList();

            string reason = shared.Count > 0
                ? $"Спільні категорії доказів: {string.Join(", ", shared)}; семантична близькість {score:P0}."
                : $"Семантична близькість {score:P0} за профілем справи.";

            results.Add(new SimilarClaim(claimId, score, reason, shared));
        }

        return results
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.ClaimId, StringComparer.Ordinal)
            .Take(topK)
            .ToList();
    }

    private static float[] Centroid(IEnumerable<EvidenceChunk> chunks)
    {
        float[]? acc = null;
        int n = 0;
        foreach (var c in chunks)
        {
            var v = EmbeddingCodec.FromJson(c.EmbeddingJson);
            if (v is null || v.Length == 0) continue;
            acc ??= new float[v.Length];
            if (v.Length != acc.Length) continue; // dimension guard
            for (int i = 0; i < v.Length; i++) acc[i] += v[i];
            n++;
        }
        if (acc is null || n == 0) return Array.Empty<float>();

        for (int i = 0; i < acc.Length; i++) acc[i] /= n;

        // L2-normalize so cosine == dot.
        double sum = 0;
        for (int i = 0; i < acc.Length; i++) sum += (double)acc[i] * acc[i];
        if (sum > 0)
        {
            float inv = (float)(1.0 / Math.Sqrt(sum));
            for (int i = 0; i < acc.Length; i++) acc[i] *= inv;
        }
        return acc;
    }

    private readonly record struct ClaimProfile(float[] Centroid, HashSet<string> Kinds);
}
