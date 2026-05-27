using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.AiAnalysis.Persistence;

public static class AiAnalysisPersistenceExtensions
{
    public static IServiceCollection AddAiAnalysisPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AiAnalysisDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "ai_analysis")));

        return services;
    }
}
