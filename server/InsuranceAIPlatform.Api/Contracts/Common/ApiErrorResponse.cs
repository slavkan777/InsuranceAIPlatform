namespace InsuranceAIPlatform.Api.Contracts.Common;

/// <summary>
/// Uniform error envelope for all 4xx and 5xx responses.
/// Never includes stack traces, exception types, or domain entity internals.
/// </summary>
public record ApiErrorResponse(
    string Code,
    string Message,
    string TraceId);
