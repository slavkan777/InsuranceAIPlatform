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
        services.AddSingleton<IDocumentsService, DocumentsService>();
        services.AddSingleton<IServiceHealthContributor>(sp => sp.GetRequiredService<IDocumentsService>());
        return services;
    }
}
