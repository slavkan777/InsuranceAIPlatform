using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.AuditCost.Persistence;

public static class AuditCostPersistenceExtensions
{
    /// <summary>
    /// Registers <see cref="AuditCostDbContext"/> factory and replaces the skeleton
    /// <see cref="IAuditCostService"/> with the DB-backed <see cref="PersistenceAuditCostService"/>
    /// as a singleton (uses <see cref="IDbContextFactory{T}"/> for scoping).
    /// Call AFTER <c>AddAuditCostServiceSkeleton()</c>.
    /// </summary>
    public static IServiceCollection AddAuditCostPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        // Singleton factory: lets PersistenceAuditCostService be a singleton without scoped-in-singleton violations.
        // Note: AddDbContextFactory with ServiceLifetime.Singleton registers all factory internals as singleton.
        // We do NOT call AddDbContext here — DbMigrator registers its own context separately.
        services.AddDbContextFactory<AuditCostDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "audit_cost")),
            ServiceLifetime.Singleton);

        // Register IClock if not already present
        if (!services.Any(d => d.ServiceType == typeof(IClock)))
            services.AddSingleton<IClock, SystemClock>();

        // Replace the singleton skeleton IAuditCostService with the DB-backed singleton.
        // PersistenceAuditCostService uses IDbContextFactory<> so it can be singleton-safe.
        services.AddSingleton<IAuditCostService, PersistenceAuditCostService>();

        return services;
    }
}
