using System.Net.Http.Json;
using System.Text.Json;

namespace InsuranceAIPlatform.Api.Rag;

// ---- DTOs (camelCase on the wire to match the FastAPI/pydantic sidecar) ----

public sealed record AdvancedEvidenceItem(string ChunkId, string Kind, string Text);

public sealed record AdvancedReviewRequest(
    string ClaimId, string? CustomerName, string? Vehicle, string? EventType,
    string? Description, string? Question, IReadOnlyList<AdvancedEvidenceItem> Evidence);

public sealed record AdvancedReviewCitation(string ChunkId, string Kind);

public sealed record AdvancedReviewResult(
    string ClaimId, string Summary, string CoverageAssessment, string EvidenceStrength,
    IReadOnlyList<string> Anomalies, IReadOnlyList<string> MissingItems, string RecommendedNextAction,
    IReadOnlyList<AdvancedReviewCitation> Citations, int Confidence, bool AdvisoryOnly,
    string ProviderMode, string Framework);

/// <summary>
/// Typed client for the LangChain advanced-analytics sidecar. Returns null on any failure
/// (unreachable / timeout / non-200 / bad body) so the endpoint can fall back safely — the sidecar
/// is an OPTIONAL enrichment, never a hard dependency.
/// </summary>
public interface IAdvancedClaimAnalyticsClient
{
    Task<AdvancedReviewResult?> ReviewAsync(AdvancedReviewRequest request, CancellationToken ct = default);
}

public sealed class HttpAdvancedClaimAnalyticsClient : IAdvancedClaimAnalyticsClient
{
    public const string HttpClientName = "advanced-analytics";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHttpClientFactory _factory;
    private readonly AdvancedAiReviewOptions _options;

    public HttpAdvancedClaimAnalyticsClient(IHttpClientFactory factory, AdvancedAiReviewOptions options)
    {
        _factory = factory;
        _options = options;
    }

    public async Task<AdvancedReviewResult?> ReviewAsync(AdvancedReviewRequest request, CancellationToken ct = default)
    {
        try
        {
            var client = _factory.CreateClient(HttpClientName);
            client.BaseAddress ??= new Uri(_options.SidecarBaseUrl);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_options.TimeoutMs);

            using var resp = await client.PostAsJsonAsync("/advanced-claim-analytics", request, Json, cts.Token);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<AdvancedReviewResult>(Json, cts.Token);
        }
        catch
        {
            return null; // honest fallback — never throw into the request path
        }
    }
}
