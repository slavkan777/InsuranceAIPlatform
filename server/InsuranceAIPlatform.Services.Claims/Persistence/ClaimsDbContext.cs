using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.Claims.Persistence;

/// <summary>
/// EF Core DbContext for the Claims bounded context.
/// Schema: claims. No cross-context DbSets.
/// </summary>
public sealed class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : base(options) { }

    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimStatusHistory> ClaimStatusHistories => Set<ClaimStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("claims");

        modelBuilder.Entity<Claim>(e =>
        {
            e.HasKey(x => x.ClaimId);
            e.Property(x => x.ClaimId).HasMaxLength(32);
            e.Property(x => x.CustomerId).HasMaxLength(32).IsRequired();
            e.Property(x => x.PolicyId).HasMaxLength(64).IsRequired();
            e.Property(x => x.Customer).HasMaxLength(200);
            e.Property(x => x.Vehicle).HasMaxLength(200);
            e.Property(x => x.VehicleVin).HasMaxLength(50);
            e.Property(x => x.Policy).HasMaxLength(200);
            e.Property(x => x.EventType).HasMaxLength(100);
            e.Property(x => x.Location).HasMaxLength(300);
            e.Property(x => x.Status).HasMaxLength(100);
            e.Property(x => x.Risk).HasMaxLength(50);
            e.Property(x => x.MissingDocument).HasMaxLength(300);
            e.Property(x => x.TraceId).HasMaxLength(64);
            e.Property(x => x.RunId).HasMaxLength(64);
            e.Property(x => x.Estimate).HasColumnType("decimal(18,2)");
            e.Property(x => x.ExpectedBenchmark).HasColumnType("decimal(18,2)");
            e.Property(x => x.Deductible).HasColumnType("decimal(18,2)");
            e.Property(x => x.RecommendedPayout).HasColumnType("decimal(18,2)");
            e.Property(x => x.Cost).HasColumnType("decimal(18,4)");
            e.HasMany(x => x.StatusHistory).WithOne(h => h.Claim)
                .HasForeignKey(h => h.ClaimId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClaimStatusHistory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.Status).HasMaxLength(100);
            e.Property(x => x.Note).HasMaxLength(500);
        });
    }
}
