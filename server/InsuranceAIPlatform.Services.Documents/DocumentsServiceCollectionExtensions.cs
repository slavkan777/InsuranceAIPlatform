using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.Documents;

/// <summary>
/// DI registration for the Documents service skeleton. Registers the service once and exposes
/// the same instance as an <see cref="IServiceHealthContributor"/> for BFF health aggregation.
/// </summary>
public static class DocumentsServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentsServiceSkeleton(this IServiceCollection services)
    {
        // Register the concrete skeleton singleton directly — this ensures IServiceHealthContributor
        // always resolves the skeleton DocumentsService regardless of IDocumentsService overrides.
        services.AddSingleton<DocumentsService>();
        services.AddSingleton<IDocumentsService>(sp => sp.GetRequiredService<DocumentsService>());
        services.AddSingleton<IServiceHealthContributor>(sp => sp.GetRequiredService<DocumentsService>());
        return services;
    }
}
