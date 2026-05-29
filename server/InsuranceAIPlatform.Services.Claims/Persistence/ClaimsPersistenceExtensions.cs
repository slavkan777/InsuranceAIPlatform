using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.Claims.Persistence;

public static class ClaimsPersistenceExtensions
{
    /// <summary>
    /// Registers <see cref="ClaimsDbContext"/> factory and replaces the skeleton
    /// <see cref="IClaimsService"/> with the DB-backed <see cref="PersistenceClaimsService"/>
    /// as a singleton (uses <see cref="IDbContextFactory{T}"/> for scoping).
    /// Call AFTER <c>AddClaimsServiceSkeleton()</c>.
    /// </summary>
    public static IServiceCollection AddClaimsPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        // Singleton factory: lets PersistenceClaimsService be a singleton without scoped-in-singleton violations.
        services.AddDbContextFactory<ClaimsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "claims")),
            ServiceLifetime.Singleton);

        if (!services.Any(d => d.ServiceType == typeof(IClock)))
            services.AddSingleton<IClock, SystemClock>();

        // Replace the singleton skeleton IClaimsService with the DB-backed singleton.
        services.AddSingleton<IClaimsService, PersistenceClaimsService>();

        return services;
    }
}
