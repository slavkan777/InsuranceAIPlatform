using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.Documents.Persistence;

public static class DocumentsPersistenceExtensions
{
    public static IServiceCollection AddDocumentsPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<DocumentsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "documents")));

        return services;
    }
}
