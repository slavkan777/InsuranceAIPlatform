using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InsuranceAIPlatform.Services.Approval.Persistence;

public sealed class ApprovalDbContextFactory : IDesignTimeDbContextFactory<ApprovalDbContext>
{
    public ApprovalDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringConfigKey)
            ?? Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringEnvVar)
            ?? SeedConstants.DefaultConnectionString;

        var options = new DbContextOptionsBuilder<ApprovalDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "approval"))
            .Options;

        return new ApprovalDbContext(options);
    }
}
