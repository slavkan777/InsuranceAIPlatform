using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.AuditCost.Persistence;

public static class AuditCostPersistenceExtensions
{
    public static IServiceCollection AddAuditCostPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AuditCostDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "audit_cost")));

        return services;
    }
}
