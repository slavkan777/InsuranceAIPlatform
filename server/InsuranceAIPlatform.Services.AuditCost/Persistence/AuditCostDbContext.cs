using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.AuditCost.Persistence;

/// <summary>
/// EF Core DbContext for the Audit &amp; Cost bounded context.
/// Schema: audit_cost. No cross-context DbSets.
/// </summary>
public sealed class AuditCostDbContext : DbContext
{
    public AuditCostDbContext(DbContextOptions<AuditCostDbContext> options) : base(options) { }

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<CostTrace> CostTraces => Set<CostTrace>();
    public DbSet<TokenUsageTrace> TokenUsageTraces => Set<TokenUsageTrace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("audit_cost");

        modelBuilder.Entity<AuditEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.At).HasMaxLength(20);
            e.Property(x => x.Source).HasMaxLength(100);
            e.Property(x => x.Message).HasMaxLength(500);
            e.Property(x => x.Severity).HasMaxLength(20);
        });

        modelBuilder.Entity<CostTrace>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100);
            e.Property(x => x.Amount).HasColumnType("decimal(18,4)");
        });

        modelBuilder.Entity<TokenUsageTrace>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.Cost).HasColumnType("decimal(18,4)");
        });
    }
}
