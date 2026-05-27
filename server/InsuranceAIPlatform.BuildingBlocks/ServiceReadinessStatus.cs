namespace InsuranceAIPlatform.BuildingBlocks;

/// <summary>
/// Readiness state of an internal service skeleton behind the BFF / API Gateway.
/// Skeleton-phase only: no service performs real work, persistence, or AI calls yet.
/// </summary>
public enum ServiceReadinessStatus
{
    /// <summary>Service exposes a working capability in-process.</summary>
    Ready,

    /// <summary>Skeleton present and resolvable; no real behaviour yet.</summary>
    Stub,

    /// <summary>Capability intentionally deferred to a later gate (e.g. the AI provider).</summary>
    Deferred
}
