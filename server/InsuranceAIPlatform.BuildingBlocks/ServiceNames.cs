namespace InsuranceAIPlatform.BuildingBlocks;

/// <summary>
/// Canonical names of the internal service boundaries behind the BFF / API Gateway.
/// Used for health/metadata and logging categories only — no business meaning.
/// </summary>
public static class ServiceNames
{
    public const string Claims = "claims-service";
    public const string CustomersPolicies = "customers-policies-service";
    public const string Documents = "documents-service";
    public const string AiAnalysis = "ai-analysis-service";
    public const string Approval = "approval-service";
    public const string AuditCost = "audit-cost-service";
}
