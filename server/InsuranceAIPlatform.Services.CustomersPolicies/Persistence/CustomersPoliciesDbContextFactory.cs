using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InsuranceAIPlatform.Services.CustomersPolicies.Persistence;

/// <summary>
/// Design-time factory for EF Core tools (dotnet ef migrations add).
/// Reads connection string from env vars, falls back to LocalDB default.
/// </summary>
public sealed class CustomersPoliciesDbContextFactory
    : IDesignTimeDbContextFactory<CustomersPoliciesDbContext>
{
    public CustomersPoliciesDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringConfigKey)
            ?? Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringEnvVar)
            ?? SeedConstants.DefaultConnectionString;

        var options = new DbContextOptionsBuilder<CustomersPoliciesDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "customers_policies"))
            .Options;

        return new CustomersPoliciesDbContext(options);
    }
}
