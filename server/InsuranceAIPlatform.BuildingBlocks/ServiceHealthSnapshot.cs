namespace InsuranceAIPlatform.BuildingBlocks;

/// <summary>
/// Immutable, synthetic readiness snapshot for one internal service skeleton.
/// Cross-cutting primitive only — carries no business data and no PII.
/// </summary>
public record ServiceHealthSnapshot(
    string ServiceName,
    ServiceReadinessStatus Status,
    string Stage,
    IReadOnlyList<string> Capabilities);
