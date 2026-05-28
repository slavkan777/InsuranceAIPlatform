namespace InsuranceAIPlatform.Services.AiAnalysis.Configuration;

/// <summary>
/// Configuration for the AI provider. Bound from appsettings section "AiProvider".
/// NO key property — DEEPSEEK_API_KEY must never appear here.
/// Default mode: Mock (no external call).
/// </summary>
public sealed class AiProviderOptions
{
    public string Mode { get; set; } = "Mock";

    /// <summary>
    /// Defense-in-depth: even if Mode="DeepSeek", real calls are blocked unless this is true.
    /// Default: false. This gate always forces false regardless of config value.
    /// </summary>
    public bool RealCallsEnabled { get; set; } = false;

    public DeepSeekOptions DeepSeek { get; set; } = new();
}

/// <summary>
/// DeepSeek model config. NO key property — DEEPSEEK_API_KEY is never stored here.
/// The API key is intentionally NOT a property here; the value is loaded only via
/// IConfiguration["DEEPSEEK_API_KEY"] at request time and never stored on this options object.
/// </summary>
public sealed class DeepSeekOptions
{
    /// <summary>DeepSeek model ID. Use "deepseek-chat" for the real OpenAI-compatible chat endpoint.</summary>
    public string Model { get; set; } = "deepseek-chat";

    /// <summary>DeepSeek OpenAI-compatible chat-completions endpoint.</summary>
    public string Endpoint { get; set; } = "https://api.deepseek.com/v1/chat/completions";

    /// <summary>HTTP request timeout in seconds for DeepSeek calls.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Bounded total number of attempts for a single AnalyzeAsync call.
    /// Default: 2 (i.e. one retry after the first failure).
    /// Only safe transient failures are retried: 408, 429, 500, 502, 503, 504, or timeout.
    /// Auth/validation failures (400/401/403) are NEVER retried.
    /// </summary>
    public int MaxAttempts { get; set; } = 2;

    /// <summary>
    /// Base delay in milliseconds before a retry attempt. A small deterministic jitter is added
    /// (0–RetryBaseDelayMs/2 ms). Default: 200ms.
    /// </summary>
    public int RetryBaseDelayMs { get; set; } = 200;
}
