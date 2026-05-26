namespace InsuranceAIPlatform.Api.Contracts;

/// <summary>
/// Synthetic local-demo status for the workbench. This is NOT a real production
/// or claims-processing signal — it advertises which layers are wired vs planned.
/// </summary>
public record DemoStatusResponse(
    string Project,
    string Mode,
    string Data,
    string Backend,
    string Database,
    string AiProvider,
    string ClaimFlow,
    bool HumanApprovalRequired,
    string Message,
    DateTimeOffset TimestampUtc);
