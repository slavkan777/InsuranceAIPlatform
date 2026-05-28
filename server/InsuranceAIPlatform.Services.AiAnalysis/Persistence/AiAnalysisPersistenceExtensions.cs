using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.AiAnalysis.Persistence;

public static class AiAnalysisPersistenceExtensions
{
    /// <summary>
    /// Registers AiAnalysisDbContext as a singleton factory.
    /// Uses AddDbContextFactory so the singleton PersistenceAiAnalysisOrchestrator can safely
    /// create scoped DbContext instances without scoped-in-singleton violations.
    /// </summary>
    public static IServiceCollection AddAiAnalysisPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContextFactory<AiAnalysisDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "ai_analysis")),
            ServiceLifetime.Singleton);

        return services;
    }
}
