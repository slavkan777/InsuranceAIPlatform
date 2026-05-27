using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.AiAnalysis;

/// <summary>
/// DI registration for the AI Analysis service skeleton. Registers the service once and exposes it as
/// an <see cref="IServiceHealthContributor"/>. Intentionally registers NO <see cref="IAiProvider"/>
/// implementation — the skeleton wires no provider, so no AI call is possible.
/// </summary>
public static class AiAnalysisServiceCollectionExtensions
{
    public static IServiceCollection AddAiAnalysisServiceSkeleton(this IServiceCollection services)
    {
        services.AddSingleton<IAiAnalysisService, AiAnalysisService>();
        services.AddSingleton<IServiceHealthContributor>(sp => sp.GetRequiredService<IAiAnalysisService>());
        return services;
    }
}
