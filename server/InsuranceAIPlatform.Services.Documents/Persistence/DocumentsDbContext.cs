using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.Documents.Persistence;

/// <summary>
/// EF Core DbContext for the Documents bounded context.
/// Schema: documents. No cross-context DbSets.
/// </summary>
public sealed class DocumentsDbContext : DbContext
{
    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options) : base(options) { }

    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
    public DbSet<MissingDocumentRequest> MissingDocumentRequests => Set<MissingDocumentRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("documents");

        modelBuilder.Entity<ClaimDocument>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.Kind).HasMaxLength(100);
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Meta).HasMaxLength(300);
            e.Property(x => x.Status).HasMaxLength(50);
            e.Property(x => x.DocType).HasMaxLength(50);
        });

        modelBuilder.Entity<MissingDocumentRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ClaimId).HasMaxLength(32).IsRequired();
            e.Property(x => x.DocumentTitle).HasMaxLength(300).IsRequired();
            e.Property(x => x.Reason).HasMaxLength(1000);
            e.Property(x => x.RequestedByActor).HasMaxLength(200);
        });
    }
}
