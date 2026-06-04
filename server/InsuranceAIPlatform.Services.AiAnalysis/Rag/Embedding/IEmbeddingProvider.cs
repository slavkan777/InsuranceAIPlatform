namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;

/// <summary>
/// Local, deterministic text embedding boundary. No external service, no API key, no network call.
/// Implementations must be deterministic so CI/tests and seeded embeddings are reproducible and
/// the vector cache is rebuildable from source text (SQL stays the source of truth).
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>Stable model identifier persisted alongside cached vectors (re-embed gate).</summary>
    string ModelName { get; }

    /// <summary>Fixed embedding dimensionality.</summary>
    int Dimensions { get; }

    /// <summary>Produces an L2-normalized embedding for the given text. Deterministic for identical input.</summary>
    float[] Embed(string text);
}
