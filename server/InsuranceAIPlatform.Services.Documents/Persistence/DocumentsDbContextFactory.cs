using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InsuranceAIPlatform.Services.Documents.Persistence;

public sealed class DocumentsDbContextFactory : IDesignTimeDbContextFactory<DocumentsDbContext>
{
    public DocumentsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringConfigKey)
            ?? Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringEnvVar)
            ?? SeedConstants.DefaultConnectionString;

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "documents"))
            .Options;

        return new DocumentsDbContext(options);
    }
}
