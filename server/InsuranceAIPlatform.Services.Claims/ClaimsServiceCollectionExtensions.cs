using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.Claims;

/// <summary>
/// DI registration for the Claims service skeleton. Registers the service once and also
/// exposes the same instance as an <see cref="IServiceHealthContributor"/> for BFF health aggregation.
/// </summary>
public static class ClaimsServiceCollectionExtensions
{
    public static IServiceCollection AddClaimsServiceSkeleton(this IServiceCollection services)
    {
        services.AddSingleton<IClaimsService, ClaimsService>();
        services.AddSingleton<IServiceHealthContributor>(sp => sp.GetRequiredService<IClaimsService>());
        return services;
    }
}
