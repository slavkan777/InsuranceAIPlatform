using InsuranceAIPlatform.BuildingBlocks;

namespace InsuranceAIPlatform.Services.Claims;

/// <summary>
/// Claims service boundary. Owner of the claim queue, claim detail, claim
/// lifecycle, and deterministic status rules.
///
/// Read methods are provided by the dedicated read service on the API side
/// (<c>IClaimReadService</c>); this interface owns the WRITE side — creating
/// new synthetic claims for the local/sandbox scenario.
/// </summary>
public interface IClaimsService : IServiceHealthContributor
{
    /// <summary>Canonical service name (see <see cref="ServiceNames.Claims"/>).</summary>
    string ServiceName { get; }

    /// <summary>
    /// Returns the next available <c>CLM-####</c> id. Numbers are auto-incremented
    /// from the highest existing id in the DB. Test/local sandbox only — never
    /// represents a real production claim.
    /// </summary>
    Task<string> AllocateNextClaimIdAsync(CancellationToken ct = default);

    /// <summary>
    /// Persists a new synthetic Claim row using the provided seed values. All fields
    /// are local/synthetic. NO real PII, NO real money. Returns the created claim's
    /// ClaimId on success.
    /// </summary>
    Task<string> CreateClaimAsync(
        NewSyntheticClaim seed,
        ActorContext actor,
        CancellationToken ct = default);

    /// <summary>
    /// Returns every claim row from the DB ordered by ClaimId DESC. Used by the
    /// BFF read path to merge with in-memory seed list. Local/sandbox scope.
    /// </summary>
    Task<IReadOnlyList<SyntheticClaimSummary>> GetAllClaimsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns a single claim row by id, or null if not found in DB.
    /// </summary>
    Task<SyntheticClaimSummary?> GetClaimByIdAsync(string claimId, CancellationToken ct = default);
}

/// <summary>Inputs for creating a new synthetic claim.</summary>
public sealed record NewSyntheticClaim(
    string CustomerId,
    string CustomerName,
    string Policy,
    string PolicyId,
    string Vehicle,
    string VehicleVin,
    string EventType,
    DateOnly EventDate,
    string Location,
    string? Description);

/// <summary>Read-only summary used by the BFF list endpoint.</summary>
public sealed record SyntheticClaimSummary(
    string ClaimId,
    string CustomerId,
    string Customer,
    string Vehicle,
    string VehicleVin,
    string PolicyId,
    string Policy,
    string EventType,
    DateOnly EventDate,
    string Location,
    string Status,
    string Risk,
    int RiskScore,
    int DocumentsReceived,
    int DocumentsTotal,
    DateTimeOffset SlaDeadline,
    decimal Estimate,
    decimal RecommendedPayout,
    string Description);
