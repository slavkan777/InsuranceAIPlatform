using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InsuranceAIPlatform.Services.AiAnalysis.Rag;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Retrieval;

namespace InsuranceAIPlatform.Api.Rag;

/// <summary>
/// HTTP implementation of <see cref="IQdrantVectorClient"/> against a LOCAL Qdrant instance
/// (default http://localhost:6333). Uses the raw REST API (no SDK / NuGet dependency) verified against
/// Qdrant 1.18.x:
///   - create:  PUT  /collections/{c}            {"vectors":{"size":N,"distance":"Cosine"}}
///   - upsert:  PUT  /collections/{c}/points?wait=true
///   - search:  POST /collections/{c}/points/search   (with a claimId payload filter)
///
/// Safety posture: local-only endpoint, no secret, no cloud call. A short timeout (RuntimeProbeTimeoutMs)
/// bounds every call so a missing/slow local runtime can never stall the request — the router catches any
/// exception and falls back to the in-process index.
/// </summary>
public sealed class HttpQdrantVectorClient : IQdrantVectorClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _endpoint;
    private readonly string _collection;
    private readonly int _timeoutMs;

    public HttpQdrantVectorClient(IHttpClientFactory httpClientFactory, RagOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _endpoint = (string.IsNullOrWhiteSpace(options.QdrantEndpoint) ? "http://localhost:6333" : options.QdrantEndpoint).TrimEnd('/');
        _collection = string.IsNullOrWhiteSpace(options.QdrantCollection) ? "insurance_evidence" : options.QdrantCollection;
        _timeoutMs = options.RuntimeProbeTimeoutMs > 0 ? options.RuntimeProbeTimeoutMs : 1500;
    }

    public async Task EnsureCollectionAsync(int dimensions, CancellationToken ct = default)
    {
        using var client = _httpClientFactory.CreateClient("qdrant");
        using var cts = Linked(ct);

        // Already present? GET returns 200 when the collection exists.
        using (var getResp = await client.GetAsync(
            $"{_endpoint}/collections/{_collection}", HttpCompletionOption.ResponseHeadersRead, cts.Token))
        {
            if (getResp.IsSuccessStatusCode) return;
        }

        var body = $"{{\"vectors\":{{\"size\":{dimensions.ToString(CultureInfo.InvariantCulture)},\"distance\":\"Cosine\"}}}}";
        using var putResp = await client.PutAsync(
            $"{_endpoint}/collections/{_collection}",
            new StringContent(body, Encoding.UTF8, "application/json"), cts.Token);
        putResp.EnsureSuccessStatusCode();
    }

    public async Task UpsertAsync(IReadOnlyList<QdrantUpsertPoint> points, CancellationToken ct = default)
    {
        if (points is null || points.Count == 0) return;

        using var client = _httpClientFactory.CreateClient("qdrant");
        using var cts = Linked(ct);

        var sb = new StringBuilder();
        sb.Append("{\"points\":[");
        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            if (i > 0) sb.Append(',');
            sb.Append("{\"id\":\"").Append(PointId(p.ChunkId)).Append("\",\"vector\":");
            AppendVector(sb, p.Vector);
            sb.Append(",\"payload\":{\"chunkId\":").Append(JsonSerializer.Serialize(p.ChunkId))
              .Append(",\"claimId\":").Append(JsonSerializer.Serialize(p.ClaimId))
              .Append("}}");
        }
        sb.Append("]}");

        using var resp = await client.PutAsync(
            $"{_endpoint}/collections/{_collection}/points?wait=true",
            new StringContent(sb.ToString(), Encoding.UTF8, "application/json"), cts.Token);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<QdrantSearchHit>> SearchAsync(
        string claimId, IReadOnlyList<float> queryVector, int topK, CancellationToken ct = default)
    {
        using var client = _httpClientFactory.CreateClient("qdrant");
        using var cts = Linked(ct);

        var sb = new StringBuilder();
        sb.Append("{\"vector\":");
        AppendVector(sb, queryVector);
        sb.Append(",\"limit\":").Append(topK.ToString(CultureInfo.InvariantCulture))
          .Append(",\"with_payload\":true,\"filter\":{\"must\":[{\"key\":\"claimId\",\"match\":{\"value\":")
          .Append(JsonSerializer.Serialize(claimId))
          .Append("}}]}}");

        using var resp = await client.PostAsync(
            $"{_endpoint}/collections/{_collection}/points/search",
            new StringContent(sb.ToString(), Encoding.UTF8, "application/json"), cts.Token);
        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(cts.Token);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cts.Token);

        var hits = new List<QdrantSearchHit>();
        if (doc.RootElement.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in result.EnumerateArray())
            {
                double score = item.TryGetProperty("score", out var s) && s.TryGetDouble(out var sv) ? sv : 0d;
                string chunkId =
                    item.TryGetProperty("payload", out var payload) &&
                    payload.ValueKind == JsonValueKind.Object &&
                    payload.TryGetProperty("chunkId", out var cid) &&
                    cid.ValueKind == JsonValueKind.String
                        ? cid.GetString()!
                        : string.Empty;

                if (!string.IsNullOrEmpty(chunkId))
                    hits.Add(new QdrantSearchHit(chunkId, score));
            }
        }
        return hits;
    }

    private CancellationTokenSource Linked(CancellationToken ct)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeoutMs);
        return cts;
    }

    private static void AppendVector(StringBuilder sb, IReadOnlyList<float> vector)
    {
        sb.Append('[');
        for (int j = 0; j < vector.Count; j++)
        {
            if (j > 0) sb.Append(',');
            sb.Append(vector[j].ToString("R", CultureInfo.InvariantCulture));
        }
        sb.Append(']');
    }

    /// <summary>
    /// Deterministic Qdrant point id derived from the chunk id (MD5 → GUID). Stable across runs so a
    /// re-upsert overwrites the same point (idempotent). MD5 is used as a non-cryptographic id hash only.
    /// </summary>
    private static string PointId(string chunkId)
    {
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(chunkId ?? string.Empty));
        return new Guid(hash).ToString();
    }
}
