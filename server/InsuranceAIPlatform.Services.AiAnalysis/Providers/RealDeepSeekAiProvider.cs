using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis.Configuration;
using InsuranceAIPlatform.Services.AiAnalysis.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InsuranceAIPlatform.Services.AiAnalysis.Providers;

/// <summary>
/// Real DeepSeek AI provider using the OpenAI-compatible chat-completions API.
/// Opt-in only — requires Mode in {"DeepSeek","DeepSeekReal"} + RealCallsEnabled=true +
/// DEEPSEEK_API_KEY non-empty in IConfiguration (sourced from environment variable).
///
/// Security contract:
/// - DEEPSEEK_API_KEY is read ONLY via IConfiguration["DEEPSEEK_API_KEY"] at request time.
/// - The key value is NEVER stored in a field, property, log message, exception text,
///   or any other durable or observable location.
/// - No SDK package is used — raw HttpClient via IHttpClientFactory only.
/// - AI output is advisory only; this provider never approves payouts, rejects claims,
///   or performs any irreversible action.
/// </summary>
public sealed class RealDeepSeekAiProvider : IAiProvider
{
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration _config;
    private readonly IOptions<AiProviderOptions> _options;
    private readonly IClock _clock;
    private readonly ILogger<RealDeepSeekAiProvider> _logger;

    // Advisory-only system prompt — enforces guardrails at the model level.
    private const string SystemPrompt =
        "You are an advisory AI analysis assistant for an insurance claim workbench. " +
        "AI output is advisory only. " +
        "You CANNOT approve payout, CANNOT reject claim, CANNOT accuse fraud as a final fact, " +
        "CANNOT send customer messages, CANNOT change claim status. " +
        "You MUST return STRUCTURED JSON ONLY with fields: " +
        "summaryText, recommendedActionText, policyExplanationText, " +
        "riskLevel (one of: low|moderate|high), " +
        "confidenceScore (0-100 integer), " +
        "findings (array of {category,text,severity:'ok'|'warn'}), " +
        "evidence (array of {source,note,confidence:0-100 integer}), " +
        "risks (array of {label,weight:0-100 integer}). " +
        "All input data is synthetic. Human decision is always final.";

    // Synthetic CLM-1006 context — hardcoded mini-summary, no real PII.
    private const string ClaimContextUserMessage =
        "Analyse the following synthetic insurance claim and return structured JSON as instructed.\n\n" +
        "Claim ID: CLM-1006 (synthetic)\n" +
        "Customer: Robert Johnson (synthetic)\n" +
        "Vehicle: Toyota Camry 2021\n" +
        "Event: Road traffic accident (ДТП), 18.05.2026, Boryspil area\n" +
        "Recommended payout: $2720 | Benchmark: $1970 (difference: +38%)\n" +
        "Documents: 6 of 7 submitted — missing: rear bumper photo\n" +
        "Policy: Auto Comprehensive POL-2025-AC-4421 | Deductible: $500\n\n" +
        "All data above is synthetic test data. Provide your advisory-only structured JSON analysis.";

    // Pricing estimate for cost trace — NOT authoritative; actual prices differ.
    // See https://api-docs.deepseek.com/quick_start/pricing for current rates.
    private const decimal CostPerTokenEstimate = 0.00000027m;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public AiProviderMode Mode => AiProviderMode.DeepSeek;

    public RealDeepSeekAiProvider(
        IHttpClientFactory factory,
        IConfiguration config,
        IOptions<AiProviderOptions> options,
        IClock clock,
        ILogger<RealDeepSeekAiProvider> logger)
    {
        _factory = factory;
        _config = config;
        _options = options;
        _clock = clock;
        _logger = logger;
    }

    public async Task<AiProviderRawOutput> AnalyzeAsync(AiAnalysisRequest request, CancellationToken ct = default)
    {
        // Step 1: Read API key from IConfiguration (environment variable provider).
        // Value is read here, used once, and never stored.
        // Remove all whitespace/newline characters — HTTP Bearer header does not allow them.
        var apiKeyRaw = _config["DEEPSEEK_API_KEY"];
        // Strip all whitespace including embedded CR/LF (some env vars store keys with line breaks).
        var apiKey = apiKeyRaw is null ? null : new string(apiKeyRaw.Where(c => !char.IsWhiteSpace(c)).ToArray());
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException(
                "DeepSeek API key not configured. Set DEEPSEEK_API_KEY in environment or user-secrets.");
        }

        var opts = _options.Value.DeepSeek;

        // Step 2: Build the OpenAI-compatible chat-completions request.
        var chatRequest = new DeepSeekChatRequest(
            Model: opts.Model,
            Messages:
            [
                new DeepSeekChatMessage("system", SystemPrompt),
                new DeepSeekChatMessage("user", ClaimContextUserMessage),
            ],
            Temperature: 0.2,
            MaxTokens: 1200,
            ResponseFormat: new DeepSeekResponseFormat("json_object"));

        var requestJson = JsonSerializer.Serialize(chatRequest, JsonOpts);
        var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Step 3: Get a named HttpClient — default headers are free of any credentials.
        // The Authorization header is set on the individual request message only.
        var httpClient = _factory.CreateClient("deepseek");
        httpClient.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, opts.Endpoint)
        {
            Content = requestContent,
        };
        // Key is set per-request, never on the named client's default headers.
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // Log the outgoing host+path ONLY — never the body, never the auth header.
        _logger.LogInformation(
            "DeepSeek AI provider sending request. Host: {Host} Path: {Path}",
            new Uri(opts.Endpoint).Host,
            new Uri(opts.Endpoint).AbsolutePath);

        // Step 4: POST to DeepSeek endpoint.
        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(requestMessage, ct);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new HttpRequestException($"DeepSeek call timed out after {opts.TimeoutSeconds}s.", ex);
        }

        // Step 5: Non-2xx → safe error without response body.
        if (!response.IsSuccessStatusCode)
        {
            var statusCode = (int)response.StatusCode;
            // Response body is NOT included — it may contain echoed request content or PII.
            throw new HttpRequestException($"DeepSeek call failed: HTTP {statusCode}");
        }

        // Step 6: Parse response JSON.
        var responseBody = await response.Content.ReadAsStringAsync(ct);
        var contentLength = responseBody.Length;

        DeepSeekChatResponse chatResponse;
        try
        {
            chatResponse = JsonSerializer.Deserialize<DeepSeekChatResponse>(responseBody, JsonOpts)
                ?? throw new InvalidOperationException("DeepSeek returned a null response body.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "DeepSeek returned a response that could not be parsed as the expected structured JSON.", ex);
        }

        var choice = chatResponse.Choices.FirstOrDefault()
            ?? throw new InvalidOperationException(
                "DeepSeek returned a response that could not be parsed as the expected structured JSON.");

        var assistantContent = choice.Message.Content;

        // Step 7: Parse the assistant's structured JSON content.
        DeepSeekStructuredAssistantResponse structured;
        try
        {
            structured = JsonSerializer.Deserialize<DeepSeekStructuredAssistantResponse>(assistantContent, JsonOpts)
                ?? throw new InvalidOperationException(
                    "DeepSeek returned a response that could not be parsed as the expected structured JSON.");
        }
        catch (JsonException ex)
        {
            // Raw assistant content is NOT included in the exception message.
            throw new InvalidOperationException(
                "DeepSeek returned a response that could not be parsed as the expected structured JSON.", ex);
        }

        var totalTokens = chatResponse.Usage.TotalTokens;

        // Log safe summary — tokens and model ID only, no content.
        _logger.LogInformation(
            "DeepSeek response received. Model: {Model}, Tokens: {Tokens}, ContentLength: {ContentLength}",
            chatResponse.Model,
            totalTokens,
            contentLength);

        // Step 8: Map to AiProviderRawOutput.
        var findings = structured.Findings
            .Select((f, i) => new AiFindingDraft($"f{i + 1}", f.Category, f.Text, f.Severity))
            .ToList();

        var evidence = structured.Evidence
            .Select((e, i) => new AiEvidenceDraft($"e{i + 1}", e.Source, e.Note, e.Confidence))
            .ToList();

        var risks = structured.Risks
            .Select((r, i) => new AiRiskDraft($"rs{i + 1}", r.Label, r.Weight))
            .ToList();

        // Cost estimate — not authoritative pricing; see comment on CostPerTokenEstimate.
        var estimatedCost = totalTokens * CostPerTokenEstimate;

        return new AiProviderRawOutput(
            ModelName: chatResponse.Model,
            SummaryText: structured.SummaryText,
            Findings: findings,
            Evidence: evidence,
            Risks: risks,
            RecommendedActionText: structured.RecommendedActionText,
            PolicyExplanationText: structured.PolicyExplanationText,
            ConfidenceScore: structured.ConfidenceScore,
            Tokens: totalTokens,
            Cost: estimatedCost);
    }
}
