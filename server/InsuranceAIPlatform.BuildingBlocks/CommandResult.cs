namespace InsuranceAIPlatform.BuildingBlocks;

/// <summary>
/// Canonical result returned by every BFF command endpoint.
/// Carries the new audit and outbox ids so callers can trace the write.
/// </summary>
public sealed record CommandResult(
    bool Success,
    string CommandId,
    string ClaimId,
    string? Status,
    int? AuditEventId,
    int? OutboxMessageId,
    string CorrelationId,
    string Message,
    IReadOnlyList<string> Warnings);
