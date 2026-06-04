using InsuranceAIPlatform.Services.AiAnalysis.Rag;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Runtime;

namespace InsuranceAIPlatform.Api.Rag;

/// <summary>
/// HTTP implementation of <see cref="IRagRuntimeProbe"/>. Issues a short-timeout GET to a local
/// runtime (Ollama, Qdrant) and treats ANY HTTP response as "the process is up". Connection refused,
/// timeout, or DNS failure => not reachable. Never throws to the caller — it is used only for honest
/// status reporting and must not affect the pipeline. No secret, no cloud call: local endpoints only.
/// </summary>
public sealed class HttpRagRuntimeProbe : IRagRuntimeProbe
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly int _timeoutMs;

    public HttpRagRuntimeProbe(IHttpClientFactory httpClientFactory, RagOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _timeoutMs = options.RuntimeProbeTimeoutMs > 0 ? options.RuntimeProbeTimeoutMs : 1500;
    }

    public async Task<bool> IsReachableAsync(string endpoint, string? healthPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint)) return false;
        if (!Uri.TryCreate(Combine(endpoint, healthPath), UriKind.Absolute, out var uri)) return false;

        try
        {
            using var client = _httpClientFactory.CreateClient("rag-runtime-probe");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_timeoutMs);
            using var resp = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            return true; // any HTTP response means the local runtime is listening
        }
        catch
        {
            return false; // connection refused / timeout / DNS — runtime not available locally
        }
    }

    private static string Combine(string endpoint, string? healthPath) =>
        string.IsNullOrWhiteSpace(healthPath) ? endpoint : endpoint.TrimEnd('/') + "/" + healthPath.TrimStart('/');
}
