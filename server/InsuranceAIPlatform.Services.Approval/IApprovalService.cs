using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Approval;

/// <summary>
/// Approval service boundary (skeleton). Future owner of approval drafts and the human-controlled
/// decision workflow. AI is never the final authority; submit is human-only. No payout, no messaging,
/// no submit behaviour in the skeleton.
/// </summary>
public interface IApprovalService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.Approval"/>).</summary>
    string ServiceName { get; }
}
