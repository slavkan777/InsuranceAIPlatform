using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Approval;

/// <summary>
/// Skeleton implementation of <see cref="IApprovalService"/>. Reports readiness only; holds no
/// drafts and performs no decisions yet. Submit remains human-only and is added in a later gate.
/// </summary>
public sealed class ApprovalService : IApprovalService
{
    public string ServiceName => ServiceNames.Approval;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.Approval,
        ServiceReadinessStatus.Stub,
        "skeleton-v0.1",
        new[] { "approval-draft-read", "human-decision" });
}
