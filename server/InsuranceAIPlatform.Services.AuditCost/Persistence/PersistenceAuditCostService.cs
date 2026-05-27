using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.AuditCost.Persistence;

/// <summary>
/// DB-backed implementation of <see cref="IAuditCostService"/> (singleton-safe via IDbContextFactory).
/// Writes audit events and outbox messages to <see cref="AuditCostDbContext"/>.
/// AppendAudit + WriteOutbox are separate SaveChanges calls (two local transactions, acceptable for demo;
/// true atomic outbox deferred to a future gate).
/// </summary>
public sealed class PersistenceAuditCostService : IAuditCostService
{
    private readonly IDbContextFactory<AuditCostDbContext> _factory;
    private readonly IClock _clock;

    public PersistenceAuditCostService(IDbContextFactory<AuditCostDbContext> factory, IClock clock)
    {
        _factory = factory;
        _clock   = clock;
    }

    public string ServiceName => ServiceNames.AuditCost;

    public ServiceHealthSnapshot GetHealth() => new(
        ServiceNames.AuditCost,
        ServiceReadinessStatus.Ready,
        "persistence-v0.1",
        new[] { "audit-trace-read", "audit-trace-write", "outbox-write", "cost-trace", "governance-correlation" });

    // -----------------------------------------------------------------------
    // AppendAuditAsync
    // -----------------------------------------------------------------------

    public async Task<int> AppendAuditAsync(
        string claimId,
        string actionType,
        ActorContext actor,
        string correlationId,
        string severity,
        string message,
        string? metadataJson,
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var now = _clock.UtcNow;
        var ev = new AuditEvent
        {
            ClaimId       = claimId,
            At            = now.ToString("HH:mm:ss"),
            Source        = "bff-command",
            Message       = message,
            Severity      = severity,
            CorrelationId = correlationId,
            Actor         = $"{actor.ActorName} ({actor.ActorType})",
            ActionType    = actionType,
            OccurredAtUtc = now,
            MetadataJson  = metadataJson,
        };
        db.AuditEvents.Add(ev);
        await db.SaveChangesAsync(ct);
        return ev.Id;
    }

    // -----------------------------------------------------------------------
    // WriteOutboxAsync — idempotency check on IdempotencyKey
    // -----------------------------------------------------------------------

    public async Task<(int Id, string? Warning)> WriteOutboxAsync(
        string eventType,
        string claimId,
        string correlationId,
        string payloadJson,
        string? idempotencyKey,
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Idempotency: if key already present, return existing id + warning
        if (idempotencyKey is not null)
        {
            var existing = await db.OutboxMessages
                .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, ct);
            if (existing is not null)
                return (existing.Id, $"duplicate-idempotency-key: outbox row {existing.Id} already exists");
        }

        var msg = new OutboxMessage
        {
            EventType      = eventType,
            ClaimId        = claimId,
            OccurredAtUtc  = _clock.UtcNow,
            CorrelationId  = correlationId,
            PayloadJson    = payloadJson,
            Processed      = false,
            IdempotencyKey = idempotencyKey,
        };
        db.OutboxMessages.Add(msg);
        await db.SaveChangesAsync(ct);
        return (msg.Id, null);
    }
}
