using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.AiAnalysis.Persistence;

/// <summary>
/// EF Core DbContext for the AI Analysis bounded context.
/// Schema: ai_analysis. No cross-context DbSets. ProviderMode is enforced at seeder level (never real provider).
/// </summary>
public sealed class AiAnalysisDbContext : DbContext
{
    public AiAnalysisDbContext(DbContextOptions<AiAnalysisDbContext> options) : base(options) { }

    public DbSet<AiAnalysisRun> AiAnalysisRuns => Set<AiAnalysisRun>();
    public DbSet<AiFinding> AiFindings => Set<AiFinding>();
    public DbSet<AiEvidenceReference> AiEvidenceReferences => Set<AiEvidenceReference>();
    public DbSet<AiRiskSignal> AiRiskSignals => Set<AiRiskSignal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ai_analysis");

        modelBuilder.Entity<AiAnalysisRun>(e =>
        {
            e.HasKey(x => x.RunId);
            e.Property(x => x.RunId).HasMaxLength(64);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.ProviderMode).HasMaxLength(50);
            e.Property(x => x.Cost).HasColumnType("decimal(18,4)");
            e.HasMany(x => x.Findings).WithOne(f => f.Run)
                .HasForeignKey(f => f.RunId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.EvidenceReferences).WithOne(r => r.Run)
                .HasForeignKey(r => r.RunId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.RiskSignals).WithOne(s => s.Run)
                .HasForeignKey(s => s.RunId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiFinding>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(32);
            e.Property(x => x.RunId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100);
            e.Property(x => x.Text).HasMaxLength(500);
            e.Property(x => x.Severity).HasMaxLength(20);
        });

        modelBuilder.Entity<AiEvidenceReference>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(32);
            e.Property(x => x.RunId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Source).HasMaxLength(200);
            e.Property(x => x.Note).HasMaxLength(500);
        });

        modelBuilder.Entity<AiRiskSignal>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(32);
            e.Property(x => x.RunId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Label).HasMaxLength(200);
        });
    }
}
