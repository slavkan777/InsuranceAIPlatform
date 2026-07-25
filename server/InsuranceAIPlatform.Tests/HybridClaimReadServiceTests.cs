using InsuranceAIPlatform.Api.Services;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.Claims;
using Microsoft.Extensions.Logging.Abstractions;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Regression tests for the BFF read path when the claims database is unreachable.
///
/// The value they protect: the deployed demo container runs with NO SQL server. The DB pull
/// in <see cref="HybridClaimReadService"/> is additive enrichment on top of the in-memory
/// seed list, so a database outage must degrade to "seed list only". Before this guard the
/// unguarded blocking call surfaced as HTTP 500 on every claim list/detail request, which
/// would have taken the live demo from "renders data" to "renders nothing".
/// </summary>
public class HybridClaimReadServiceTests
{
    private const string OutageMessage = "simulated database outage";

    private static HybridClaimReadService Build(IClaimsService claims) =>
        new(new InMemoryClaimReadService(), claims, NullLogger<HybridClaimReadService>.Instance);

    // ---- fakes -------------------------------------------------------------

    private abstract class FakeClaimsServiceBase : IClaimsService
    {
        public string ServiceName => "claims";
        public ServiceHealthSnapshot GetHealth() => throw new NotSupportedException();
        public Task<string> AllocateNextClaimIdAsync(CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<string> CreateClaimAsync(NewSyntheticClaim seed, ActorContext actor, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public abstract Task<IReadOnlyList<SyntheticClaimSummary>> GetAllClaimsAsync(CancellationToken ct = default);
        public abstract Task<SyntheticClaimSummary?> GetClaimByIdAsync(string claimId, CancellationToken ct = default);
    }

    /// <summary>Stands in for an unreachable SQL server — exactly the deployed demo's state.</summary>
    private sealed class UnavailableClaimsService : FakeClaimsServiceBase
    {
        public override Task<IReadOnlyList<SyntheticClaimSummary>> GetAllClaimsAsync(CancellationToken ct = default) =>
            throw new InvalidOperationException(OutageMessage);
        public override Task<SyntheticClaimSummary?> GetClaimByIdAsync(string claimId, CancellationToken ct = default) =>
            throw new InvalidOperationException(OutageMessage);
    }

    /// <summary>A healthy DB holding one operator-created claim.</summary>
    private sealed class HealthyClaimsService : FakeClaimsServiceBase
    {
        public static readonly SyntheticClaimSummary Row = new(
            ClaimId: "CLM-2001", CustomerId: "CUST-9001", Customer: "Dana Fletcher",
            Vehicle: "Ford Focus 2020", VehicleVin: "VIN-TEST-2001",
            PolicyId: "POL-9001", Policy: "Auto Comprehensive",
            EventType: "RoadAccident", EventDate: new DateOnly(2026, 5, 1),
            Location: "Springfield, Main Street 24", Status: "New", Risk: "Low",
            RiskScore: 12, DocumentsReceived: 1, DocumentsTotal: 4,
            SlaDeadline: DateTimeOffset.UtcNow.AddDays(3),
            Estimate: 1000m, RecommendedPayout: 900m, Description: "Synthetic test claim.");

        public override Task<IReadOnlyList<SyntheticClaimSummary>> GetAllClaimsAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<SyntheticClaimSummary>>(new[] { Row });
        public override Task<SyntheticClaimSummary?> GetClaimByIdAsync(string claimId, CancellationToken ct = default) =>
            Task.FromResult<SyntheticClaimSummary?>(claimId == Row.ClaimId ? Row : null);
    }

    // ---- degradation when the database is unreachable -----------------------

    [Fact]
    public void GetClaims_serves_the_seed_list_when_the_database_is_unavailable()
    {
        var expected = new InMemoryClaimReadService().GetClaims();

        var actual = Build(new UnavailableClaimsService()).GetClaims();   // must not throw

        Assert.Equal(expected.Count, actual.Count);
        Assert.NotEmpty(actual);
        Assert.Equal(expected.Select(c => c.Id), actual.Select(c => c.Id));
    }

    [Fact]
    public void GetClaim_still_returns_a_seed_claim_when_the_database_is_unavailable()
    {
        var claim = Build(new UnavailableClaimsService()).GetClaim("CLM-1006");

        Assert.NotNull(claim);
        Assert.Equal("CLM-1006", claim!.Id);
    }

    [Fact]
    public void GetClaim_reports_an_unknown_id_as_not_found_when_the_database_is_unavailable()
    {
        // Not found (→ 404), never a thrown exception (→ 500).
        var claim = Build(new UnavailableClaimsService()).GetClaim("CLM-9999");

        Assert.Null(claim);
    }

    [Fact]
    public void GetSummary_is_unaffected_by_a_database_outage()
    {
        var summary = Build(new UnavailableClaimsService()).GetSummary();

        Assert.NotNull(summary);
    }

    // ---- the healthy path is unchanged --------------------------------------

    [Fact]
    public void GetClaims_still_appends_database_rows_when_the_database_is_available()
    {
        var seed = new InMemoryClaimReadService().GetClaims();

        var actual = Build(new HealthyClaimsService()).GetClaims();

        Assert.Equal(seed.Count + 1, actual.Count);
        Assert.Equal(seed.Select(c => c.Id), actual.Take(seed.Count).Select(c => c.Id));
        Assert.Equal("CLM-2001", actual[^1].Id);          // DB rows appended after the seed
        Assert.Equal("RoadAccident", actual[^1].EventType);
    }

    [Fact]
    public void GetClaim_still_resolves_a_database_claim_when_the_database_is_available()
    {
        var claim = Build(new HealthyClaimsService()).GetClaim("CLM-2001");

        Assert.NotNull(claim);
        Assert.Equal("Dana Fletcher", claim!.Customer);
        Assert.Equal("RoadAccident", claim.EventType);
    }

    // ---- English-only contract on the no-database path -----------------------

    [Fact]
    public void Seed_list_served_without_a_database_contains_no_cyrillic()
    {
        var claims = Build(new UnavailableClaimsService()).GetClaims();

        var text = string.Concat(claims.Select(c =>
            string.Join('|', c.Id, c.Customer, c.Vehicle, c.EventType, c.Status,
                             c.AiStatus, c.Risk, c.Sla, c.NextAction)));

        Assert.DoesNotContain(text, ch => ch >= 'Ѐ' && ch <= 'ӿ');
        Assert.DoesNotContain("\\u04", text, StringComparison.OrdinalIgnoreCase);
    }
}
