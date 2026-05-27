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
    }
}
