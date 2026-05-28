using System.Text.Json.Serialization;

namespace InsuranceAIPlatform.Services.AiAnalysis.Providers;

// -----------------------------------------------------------------------
// Internal DTOs for DeepSeek OpenAI-compatible chat-completions API.
// These are purely serialization shapes — no business logic here.
// DEEPSEEK_API_KEY is NEVER a field, property, or default in these types.
// -----------------------------------------------------------------------

/// <summary>Request body for DeepSeek chat-completions (OpenAI-compatible).</summary>
internal sealed record DeepSeekChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] IReadOnlyList<DeepSeekChatMessage> Messages,
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("max_tokens")] int MaxTokens,
    [property: JsonPropertyName("response_format")] DeepSeekResponseFormat ResponseFormat);

/// <summary>A single chat message (system, user, or assistant role).</summary>
internal sealed record DeepSeekChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

/// <summary>Response format specifier — use Type="json_object" for structured output.</summary>
internal sealed record DeepSeekResponseFormat(
    [property: JsonPropertyName("type")] string Type);

/// <summary>Top-level DeepSeek chat-completions response.</summary>
internal sealed record DeepSeekChatResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("choices")] IReadOnlyList<DeepSeekChatChoice> Choices,
    [property: JsonPropertyName("usage")] DeepSeekUsage Usage);

/// <summary>A single choice in the response choices array.</summary>
internal sealed record DeepSeekChatChoice(
    [property: JsonPropertyName("message")] DeepSeekChatMessage Message,
    [property: JsonPropertyName("finish_reason")] string FinishReason);

/// <summary>Token usage from the DeepSeek API response.</summary>
internal sealed record DeepSeekUsage(
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int CompletionTokens,
    [property: JsonPropertyName("total_tokens")] int TotalTokens);

// -----------------------------------------------------------------------
// Structured assistant JSON parsed from choices[0].message.content.
// DeepSeek returns this as a JSON string when response_format=json_object.
// -----------------------------------------------------------------------

/// <summary>
/// Strongly-typed shape of the assistant's structured JSON response content.
/// All fields must be present — if parsing fails, the provider throws safely.
/// </summary>
internal sealed record DeepSeekStructuredAssistantResponse(
    [property: JsonPropertyName("summaryText")] string SummaryText,
    [property: JsonPropertyName("recommendedActionText")] string RecommendedActionText,
    [property: JsonPropertyName("policyExplanationText")] string PolicyExplanationText,
    [property: JsonPropertyName("riskLevel")] string RiskLevel,
    [property: JsonPropertyName("confidenceScore")] int ConfidenceScore,
    [property: JsonPropertyName("findings")] IReadOnlyList<RawFinding> Findings,
    [property: JsonPropertyName("evidence")] IReadOnlyList<RawEvidence> Evidence,
    [property: JsonPropertyName("risks")] IReadOnlyList<RawRisk> Risks);

/// <summary>Individual finding from the structured assistant response.</summary>
internal sealed record RawFinding(
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("severity")] string Severity);

/// <summary>Individual evidence reference from the structured assistant response.</summary>
internal sealed record RawEvidence(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("note")] string Note,
    [property: JsonPropertyName("confidence")] int Confidence);

/// <summary>Individual risk signal from the structured assistant response.</summary>
internal sealed record RawRisk(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("weight")] int Weight);
