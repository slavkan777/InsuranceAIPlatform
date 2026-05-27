using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.AuditCost;

/// <summary>
/// DI registration for the Audit &amp; Cost service skeleton. Registers the service once and exposes
/// the same instance as an <see cref="IServiceHealthContributor"/> for BFF health aggregation.
/// </summary>
public static class AuditCostServiceCollectionExtensions
{
    public static IServiceCollection AddAuditCostServiceSkeleton(this IServiceCollection services)
    {
        // Register the concrete skeleton singleton directly — this ensures IServiceHealthContributor
        // always resolves the skeleton AuditCostService regardless of IAuditCostService overrides.
        services.AddSingleton<AuditCostService>();
        services.AddSingleton<IAuditCostService>(sp => sp.GetRequiredService<AuditCostService>());
        services.AddSingleton<IServiceHealthContributor>(sp => sp.GetRequiredService<AuditCostService>());
        return services;
    }
}
