using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Claims;

/// <summary>
/// Skeleton implementation of <see cref="IClaimsService"/>. Reports readiness only;
/// owns no data and performs no claim operations. The DB-backed
/// <see cref="Persistence.PersistenceClaimsService"/> handles real reads + writes.
/// </summary>
public sealed class ClaimsService : IClaimsService
{
    public string ServiceName => ServiceNames.Claims;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.Claims,
        ServiceReadinessStatus.Stub,
        "skeleton-v0.1",
        new[] { "claim-queue", "claim-detail", "claim-lifecycle" });

    public Task<string> AllocateNextClaimIdAsync(CancellationToken ct = default)
        => Task.FromResult(string.Empty); // no-op in skeleton

    public Task<string> CreateClaimAsync(NewSyntheticClaim seed, ActorContext actor, CancellationToken ct = default)
        => Task.FromResult(string.Empty); // no-op in skeleton

    public Task<IReadOnlyList<SyntheticClaimSummary>> GetAllClaimsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<SyntheticClaimSummary>>(Array.Empty<SyntheticClaimSummary>());

    public Task<SyntheticClaimSummary?> GetClaimByIdAsync(string claimId, CancellationToken ct = default)
        => Task.FromResult<SyntheticClaimSummary?>(null);
}
