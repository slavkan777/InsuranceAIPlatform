using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InsuranceAIPlatform.Services.Claims.Persistence;

public sealed class ClaimsDbContextFactory : IDesignTimeDbContextFactory<ClaimsDbContext>
{
    public ClaimsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringConfigKey)
            ?? Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringEnvVar)
            ?? SeedConstants.DefaultConnectionString;

        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "claims"))
            .Options;

        return new ClaimsDbContext(options);
    }
}
