namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;

/// <summary>Small, allocation-free vector helpers for local retrieval.</summary>
public static class VectorMath
{
    /// <summary>
    /// Cosine similarity in [-1, 1]. Safe for non-normalized inputs (divides by magnitudes);
    /// for L2-normalized vectors this equals the dot product. Returns 0 if either vector is zero
    /// or lengths differ.
    /// </summary>
    public static double Cosine(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length == 0 || a.Length != b.Length) return 0;

        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += (double)a[i] * b[i];
            na += (double)a[i] * a[i];
            nb += (double)b[i] * b[i];
        }
        if (na <= 0 || nb <= 0) return 0;
        return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
    }
}
