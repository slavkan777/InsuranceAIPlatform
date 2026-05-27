using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.Approval;

/// <summary>
/// DI registration for the Approval service skeleton. Registers the service once and exposes
/// the same instance as an <see cref="IServiceHealthContributor"/> for BFF health aggregation.
/// </summary>
public static class ApprovalServiceCollectionExtensions
{
    public static IServiceCollection AddApprovalServiceSkeleton(this IServiceCollection services)
    {
        // Register the concrete skeleton singleton directly — this ensures IServiceHealthContributor
        // always resolves the skeleton ApprovalService regardless of IApprovalService overrides.
        services.AddSingleton<ApprovalService>();
        services.AddSingleton<IApprovalService>(sp => sp.GetRequiredService<ApprovalService>());
        services.AddSingleton<IServiceHealthContributor>(sp => sp.GetRequiredService<ApprovalService>());
        return services;
    }
}
