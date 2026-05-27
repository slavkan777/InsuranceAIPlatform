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
        services.AddSingleton<IApprovalService, ApprovalService>();
        services.AddSingleton<IServiceHealthContributor>(sp => sp.GetRequiredService<IApprovalService>());
        return services;
    }
}
