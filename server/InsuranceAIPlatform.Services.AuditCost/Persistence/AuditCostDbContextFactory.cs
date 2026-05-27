using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InsuranceAIPlatform.Services.AuditCost.Persistence;

public sealed class AuditCostDbContextFactory : IDesignTimeDbContextFactory<AuditCostDbContext>
{
    public AuditCostDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringConfigKey)
            ?? Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringEnvVar)
            ?? SeedConstants.DefaultConnectionString;

        var options = new DbContextOptionsBuilder<AuditCostDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "audit_cost"))
            .Options;

        return new AuditCostDbContext(options);
    }
}
