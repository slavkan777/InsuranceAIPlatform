using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Claims;

/// <summary>
/// Skeleton implementation of <see cref="IClaimsService"/>. Reports readiness only;
/// owns no data and performs no claim operations yet. Read logic stays in the BFF
/// until a later gate moves it here (response-identical migration).
/// </summary>
public sealed class ClaimsService : IClaimsService
{
    public string ServiceName => ServiceNames.Claims;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.Claims,
        ServiceReadinessStatus.Stub,
        "skeleton-v0.1",
        new[] { "claim-queue", "claim-detail", "claim-lifecycle" });
}
