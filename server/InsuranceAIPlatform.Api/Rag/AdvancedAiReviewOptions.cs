namespace InsuranceAIPlatform.Api.Rag;

/// <summary>
/// Feature flag + connection config for the optional LangChain "Advanced AI Review" sidecar.
/// Bound from appsettings section "AdvancedAiReview". DISABLED by default — when off, the
/// .NET endpoint returns a safe "unavailable" fallback and NEVER contacts the sidecar, so the
/// existing claim flow is unchanged. No secret/key property (the sidecar needs none).
/// </summary>
public sealed class AdvancedAiReviewOptions
{
    public const string SectionName = "AdvancedAiReview";

    /// <summary>Master switch. False (default) = feature off, fallback response, no sidecar call.</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>Base URL of the Python FastAPI LangChain sidecar (local-only / dev-test).</summary>
    public string SidecarBaseUrl { get; set; } = "http://localhost:8090";

    /// <summary>HTTP timeout (ms) for a sidecar call. On timeout/error the endpoint degrades to fallback.</summary>
    public int TimeoutMs { get; set; } = 15000;

    /// <summary>Max claim-scoped evidence chunks sent to the sidecar (bounds payload size).</summary>
    public int MaxEvidenceChunks { get; set; } = 12;
}
