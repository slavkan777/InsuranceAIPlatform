using System.Globalization;
using System.Text;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;

/// <summary>
/// Deterministic, local, dependency-free text embedding using signed feature hashing
/// (the "hashing trick"). Tokens are hashed with FNV-1a (a STABLE hash — not
/// <c>string.GetHashCode()</c>, which is randomized per process) into a fixed-width vector,
/// accumulated with a per-token sign, then L2-normalized so cosine similarity == dot product.
///
/// Why this and not a real embedding model:
///  - zero external calls, zero API key, zero cost (RAG_LOCAL_FOUNDATION_MEGA_V0.1 boundary);
///  - fully deterministic → seeded vectors and CI tests are reproducible;
///  - language-agnostic (works for the corpus's Ukrainian + English text);
///  - good enough to demonstrate real lexical/semantic-ish retrieval over a ~150-400 chunk corpus.
/// A real embedding model can later implement <see cref="IEmbeddingProvider"/> with no other change.
/// </summary>
public sealed class DeterministicEmbeddingProvider : IEmbeddingProvider
{
    public const string Model = "local-hash-embed-v0.1";

    public DeterministicEmbeddingProvider(int dimensions = 256)
    {
        if (dimensions < 16) throw new ArgumentOutOfRangeException(nameof(dimensions), "Embedding dimension must be >= 16.");
        Dimensions = dimensions;
    }

    public string ModelName => Model;

    public int Dimensions { get; }

    public float[] Embed(string text)
    {
        var vec = new float[Dimensions];
        if (string.IsNullOrWhiteSpace(text)) return vec;

        foreach (var token in Tokenize(text))
        {
            uint bucketHash = Fnv1a(token);
            uint signHash = Fnv1a("s:" + token);
            int idx = (int)(bucketHash % (uint)Dimensions);
            float sign = (signHash & 1u) == 0u ? 1f : -1f;
            vec[idx] += sign;
        }

        L2Normalize(vec);
        return vec;
    }

    /// <summary>Lowercase, split on non-letter/digit (keeps Cyrillic + Latin), drop 1-char tokens.</summary>
    internal static IEnumerable<string> Tokenize(string text)
    {
        var sb = new StringBuilder();
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
            else if (sb.Length > 0)
            {
                if (sb.Length >= 2) yield return sb.ToString();
                sb.Clear();
            }
        }
        if (sb.Length >= 2) yield return sb.ToString();
    }

    /// <summary>FNV-1a 32-bit over UTF-8 bytes. Stable across processes/runs (unlike String.GetHashCode).</summary>
    internal static uint Fnv1a(string s)
    {
        const uint offset = 2166136261;
        const uint prime = 16777619;
        uint hash = offset;
        foreach (var b in Encoding.UTF8.GetBytes(s))
        {
            hash ^= b;
            hash *= prime;
        }
        return hash;
    }

    private static void L2Normalize(float[] v)
    {
        double sum = 0;
        for (int i = 0; i < v.Length; i++) sum += (double)v[i] * v[i];
        if (sum <= 0) return;
        float inv = (float)(1.0 / Math.Sqrt(sum));
        for (int i = 0; i < v.Length; i++) v[i] *= inv;
    }
}
