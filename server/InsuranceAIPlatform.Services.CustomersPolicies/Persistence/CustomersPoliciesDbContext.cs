using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Services.CustomersPolicies.Persistence;

/// <summary>
/// EF Core DbContext for the Customers &amp; Policies bounded context.
/// Schema: customers_policies. No cross-context DbSets.
/// </summary>
public sealed class CustomersPoliciesDbContext : DbContext
{
    public CustomersPoliciesDbContext(DbContextOptions<CustomersPoliciesDbContext> options)
        : base(options) { }

    public DbSet<SyntheticCustomer> SyntheticCustomers => Set<SyntheticCustomer>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customers_policies");

        modelBuilder.Entity<SyntheticCustomer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(32);
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.AddressLine).HasMaxLength(300);
            e.HasMany(x => x.Policies).WithOne(p => p.Customer)
                .HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Vehicles).WithOne(v => v.Customer)
                .HasForeignKey(v => v.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Policy>(e =>
        {
            e.HasKey(x => x.PolicyId);
            e.Property(x => x.PolicyId).HasMaxLength(64);
            e.Property(x => x.CustomerId).HasMaxLength(32).IsRequired();
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Premium).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(32);
            e.Property(x => x.CustomerId).HasMaxLength(32).IsRequired();
            e.Property(x => x.Make).HasMaxLength(100);
            e.Property(x => x.Model).HasMaxLength(100);
            e.Property(x => x.Vin).HasMaxLength(50);
            e.Property(x => x.Color).HasMaxLength(50);
        });
    }
}
