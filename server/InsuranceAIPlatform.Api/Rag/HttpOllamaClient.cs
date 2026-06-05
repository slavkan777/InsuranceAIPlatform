using System.Text;
using System.Text.Json;
using InsuranceAIPlatform.Services.AiAnalysis.Rag;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Generation;

namespace InsuranceAIPlatform.Api.Rag;

/// <summary>
/// HTTP implementation of <see cref="ILocalLlamaClient"/> against a LOCAL Ollama instance
/// (default http://localhost:11434). Uses the raw REST API (no SDK / NuGet dependency):
///   - generate: POST /api/chat   {"model":..,"stream":false,"messages":[{system},{user}]}
///
/// Safety posture (identical to <see cref="HttpQdrantVectorClient"/>): local-only endpoint, no secret,
/// no cloud call. A timeout (RagOptions.LocalLlamaTimeoutMs) bounds the call. This method NEVER throws and
/// returns <c>null</c> on ANY failure (unreachable, timeout, non-2xx, empty/garbled body) so the generator
/// falls back to the deterministic mock — a slow/absent Ollama cannot break the flow or be mislabelled.
/// </summary>
public sealed class HttpOllamaClient : ILocalLlamaClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _endpoint;
    private readonly string _model;
    private readonly int _timeoutMs;

    public HttpOllamaClient(IHttpClientFactory httpClientFactory, RagOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _endpoint = (string.IsNullOrWhiteSpace(options.LocalLlamaEndpoint) ? "http://localhost:11434" : options.LocalLlamaEndpoint).TrimEnd('/');
        _model = string.IsNullOrWhiteSpace(options.LocalLlamaModel) ? "llama3.2:1b" : options.LocalLlamaModel;
        _timeoutMs = options.LocalLlamaTimeoutMs > 0 ? options.LocalLlamaTimeoutMs : 30000;
    }

    public LocalLlamaCompletion? TryComplete(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                model = _model,
                stream = false,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                // Low temperature for grounded, repeatable answers; cap output length.
                options = new { temperature = 0.1, num_predict = 512 }
            };
            string body = JsonSerializer.Serialize(payload);

            using var client = _httpClientFactory.CreateClient("ollama");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_timeoutMs);

            using var req = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/api/chat")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            // Synchronous send to match the synchronous IGroundedAnswerGenerator seam.
            using var resp = client.Send(req, HttpCompletionOption.ResponseContentRead, cts.Token);
            if (!resp.IsSuccessStatusCode) return null;

            using var stream = resp.Content.ReadAsStream(cts.Token);
            using var doc = JsonDocument.Parse(stream);
            var root = doc.RootElement;

            string text =
                root.TryGetProperty("message", out var msg) &&
                msg.ValueKind == JsonValueKind.Object &&
                msg.TryGetProperty("content", out var content) &&
                content.ValueKind == JsonValueKind.String
                    ? content.GetString() ?? string.Empty
                    : string.Empty;

            if (string.IsNullOrWhiteSpace(text)) return null;

            int promptTokens = root.TryGetProperty("prompt_eval_count", out var pe) && pe.TryGetInt32(out var pv) ? pv : 0;
            int completionTokens = root.TryGetProperty("eval_count", out var ec) && ec.TryGetInt32(out var cv) ? cv : 0;
            string model = root.TryGetProperty("model", out var m) && m.ValueKind == JsonValueKind.String
                ? m.GetString() ?? _model
                : _model;

            return new LocalLlamaCompletion(text, promptTokens, completionTokens, model);
        }
        catch
        {
            // Unreachable / timeout / non-2xx / malformed body — honest fallback to mock.
            return null;
        }
    }
}
