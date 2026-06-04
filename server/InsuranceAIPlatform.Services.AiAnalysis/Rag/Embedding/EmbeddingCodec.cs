using System.Text.Json;

namespace InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;

/// <summary>Serializes embedding vectors to/from the JSON cache column on EvidenceChunk.</summary>
public static class EmbeddingCodec
{
    public static string ToJson(float[] vector) => JsonSerializer.Serialize(vector);

    public static float[]? FromJson(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<float[]>(json!);
}
