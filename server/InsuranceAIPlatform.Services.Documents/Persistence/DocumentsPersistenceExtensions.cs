using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.Documents.Persistence;

public static class DocumentsPersistenceExtensions
{
    /// <summary>
    /// Registers <see cref="DocumentsDbContext"/> factory and replaces the skeleton
    /// <see cref="IDocumentsService"/> with the DB-backed <see cref="PersistenceDocumentsService"/>
    /// as a singleton (uses <see cref="IDbContextFactory{T}"/> for scoping).
    /// Call AFTER <c>AddDocumentsServiceSkeleton()</c>.
    /// </summary>
    public static IServiceCollection AddDocumentsPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        // Singleton factory: lets PersistenceDocumentsService be a singleton without scoped-in-singleton violations.
        services.AddDbContextFactory<DocumentsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "documents")),
            ServiceLifetime.Singleton);

        // Register IClock if not already present
        if (!services.Any(d => d.ServiceType == typeof(IClock)))
            services.AddSingleton<IClock, SystemClock>();

        // Replace the singleton skeleton IDocumentsService with the DB-backed singleton.
        services.AddSingleton<IDocumentsService, PersistenceDocumentsService>();

        return services;
    }
}
