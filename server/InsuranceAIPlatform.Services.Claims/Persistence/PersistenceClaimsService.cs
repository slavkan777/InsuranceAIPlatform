using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.Claims.Persistence;

/// <summary>
/// DB-backed implementation of <see cref="IClaimsService"/> (singleton-safe via IDbContextFactory).
/// Owns the claims schema. All IDs/data are synthetic — no real PII, no real money.
/// Allocation strategy: parse the numeric suffix of the highest existing CLM-#### id, +1.
/// </summary>
public sealed class PersistenceClaimsService : IClaimsService
{
    private readonly IDbContextFactory<ClaimsDbContext> _factory;
    private readonly IClock _clock;

    public PersistenceClaimsService(IDbContextFactory<ClaimsDbContext> factory, IClock clock)
    {
        _factory = factory;
        _clock   = clock;
    }

    public string ServiceName => ServiceNames.Claims;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.Claims,
        ServiceReadinessStatus.Ready,
        "persistence-v0.1",
        new[] { "claim-queue", "claim-detail", "claim-create", "claim-lifecycle" });

    public async Task<string> AllocateNextClaimIdAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var ids = await db.Claims.Select(c => c.ClaimId).ToListAsync(ct);
        var maxNum = ids
            .Select(id =>
            {
                if (id.StartsWith("CLM-", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(id[4..], out var n))
                    return n;
                return 0;
            })
            .DefaultIfEmpty(1005)
            .Max();
        var next = Math.Max(maxNum, 1005) + 1;
        return $"CLM-{next:0000}";
    }

    public async Task<string> CreateClaimAsync(NewSyntheticClaim seed, ActorContext actor, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var claimId = await AllocateNextClaimIdAsync(ct);
        var now = _clock.UtcNow;

        var claim = new Claim
        {
            ClaimId           = claimId,
            CustomerId        = seed.CustomerId,
            PolicyId          = seed.PolicyId,
            Customer          = seed.CustomerName,
            Vehicle           = seed.Vehicle,
            VehicleVin        = string.IsNullOrWhiteSpace(seed.VehicleVin) ? "VIN ****0000" : seed.VehicleVin,
            Policy            = string.IsNullOrWhiteSpace(seed.Policy) ? "Auto Comprehensive" : seed.Policy,
            EventType         = seed.EventType,
            EventDate         = seed.EventDate,
            Location          = seed.Location,
            Description       = string.IsNullOrWhiteSpace(seed.Description) ? string.Empty : seed.Description!,
            Status            = "Новий",
            Risk              = "Невизначений",
            RiskScore         = 0,
            Confidence        = 0,
            SlaDeadline       = now.AddDays(7),
            DocumentsReceived = 0,
            DocumentsTotal    = 0,
            MissingDocument   = null,
            Estimate          = 0m,
            ExpectedBenchmark = 0m,
            Deductible        = 0m,
            RecommendedPayout = 0m,
            TraceId           = "trc_" + Guid.NewGuid().ToString("N")[..8],
            RunId             = string.Empty,
            Tokens            = 0,
            Cost              = 0m,
            DurationSec       = 0,
        };

        claim.StatusHistory.Add(new ClaimStatusHistory
        {
            ClaimId   = claimId,
            Status    = "Новий",
            ChangedAt = now,
            Note      = $"Створено: {actor.ActorName} ({actor.ActorType}). Локальний sandbox.",
        });

        db.Claims.Add(claim);
        await db.SaveChangesAsync(ct);
        return claimId;
    }

    public async Task<IReadOnlyList<SyntheticClaimSummary>> GetAllClaimsAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var rows = await db.Claims
            .OrderByDescending(c => c.ClaimId)
            .ToListAsync(ct);
        return rows.Select(ToSummary).ToList();
    }

    public async Task<SyntheticClaimSummary?> GetClaimByIdAsync(string claimId, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var row = await db.Claims.FirstOrDefaultAsync(c => c.ClaimId == claimId, ct);
        return row is null ? null : ToSummary(row);
    }

    private static SyntheticClaimSummary ToSummary(Claim c) => new(
        c.ClaimId, c.CustomerId, c.Customer, c.Vehicle, c.VehicleVin,
        c.PolicyId, c.Policy, c.EventType, c.EventDate, c.Location,
        c.Status, c.Risk, c.RiskScore, c.DocumentsReceived, c.DocumentsTotal,
        c.SlaDeadline, c.Estimate, c.RecommendedPayout, c.Description ?? string.Empty);
}
