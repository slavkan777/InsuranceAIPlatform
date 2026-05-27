using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.Approval.Persistence;

public static class ApprovalPersistenceExtensions
{
    /// <summary>
    /// Registers <see cref="ApprovalDbContext"/> factory and replaces the skeleton
    /// <see cref="IApprovalService"/> with the DB-backed <see cref="PersistenceApprovalService"/>
    /// as a singleton (uses <see cref="IDbContextFactory{T}"/> for scoping).
    /// Call AFTER <c>AddApprovalServiceSkeleton()</c>.
    /// </summary>
    public static IServiceCollection AddApprovalPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        // Singleton factory: lets PersistenceApprovalService be a singleton without scoped-in-singleton violations.
        services.AddDbContextFactory<ApprovalDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "approval")),
            ServiceLifetime.Singleton);

        // Register IClock if not already present
        if (!services.Any(d => d.ServiceType == typeof(IClock)))
            services.AddSingleton<IClock, SystemClock>();

        // Replace the singleton skeleton IApprovalService with the DB-backed singleton.
        services.AddSingleton<IApprovalService, PersistenceApprovalService>();

        return services;
    }
}
