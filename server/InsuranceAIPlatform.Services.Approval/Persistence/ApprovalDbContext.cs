using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.Approval.Persistence;

/// <summary>
/// EF Core DbContext for the Approval bounded context.
/// Schema: approval. No cross-context DbSets.
/// </summary>
public sealed class ApprovalDbContext : DbContext
{
    public ApprovalDbContext(DbContextOptions<ApprovalDbContext> options) : base(options) { }

    public DbSet<ApprovalDraft> ApprovalDrafts => Set<ApprovalDraft>();
    public DbSet<ApprovalDecisionOption> ApprovalDecisionOptions => Set<ApprovalDecisionOption>();
    public DbSet<PayoutSimulation> PayoutSimulations => Set<PayoutSimulation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("approval");

        modelBuilder.Entity<ApprovalDraft>(e =>
        {
            e.HasKey(x => x.ClaimId);
            e.Property(x => x.ClaimId).HasMaxLength(32);
            e.Property(x => x.CurrentDecision).HasMaxLength(100);
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.Property(x => x.AiRecommendation).HasMaxLength(300);
            e.Property(x => x.RecommendedPayout).HasColumnType("decimal(18,2)");
            e.HasMany(x => x.Options).WithOne(o => o.Draft)
                .HasForeignKey(o => o.ClaimId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApprovalDecisionOption>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.Key).HasMaxLength(50);
            e.Property(x => x.Label).HasMaxLength(300);
            e.Property(x => x.Rationale).HasMaxLength(500);
        });

        modelBuilder.Entity<PayoutSimulation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.Status).HasMaxLength(50).IsRequired();
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Deductible).HasColumnType("decimal(18,2)");
            e.Property(x => x.NetPayoutAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Currency).HasMaxLength(8).IsRequired();
            e.Property(x => x.DecisionSource).HasMaxLength(40).IsRequired();
            e.Property(x => x.DecisionActor).HasMaxLength(200).IsRequired();
            e.Property(x => x.SourceAiRunId).HasMaxLength(64);
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.Property(x => x.CorrelationId).HasMaxLength(80).IsRequired();
            // SimulationOnly intentionally has no default in EF — service layer
            // hard-sets to true at construction. Filtered index helps audits.
            e.HasIndex(x => x.ClaimId);
            e.HasIndex(x => x.Status);
        });
    }
}
