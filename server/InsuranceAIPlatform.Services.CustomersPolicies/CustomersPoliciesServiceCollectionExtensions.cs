using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.CustomersPolicies;

/// <summary>
/// DI registration for the Customers &amp; Policies service skeleton. Registers the service once
/// and exposes the same instance as an <see cref="IServiceHealthContributor"/> for BFF health aggregation.
/// </summary>
public static class CustomersPoliciesServiceCollectionExtensions
{
    public static IServiceCollection AddCustomersPoliciesServiceSkeleton(this IServiceCollection services)
    {
        services.AddSingleton<ICustomersPoliciesService, CustomersPoliciesService>();
        services.AddSingleton<IServiceHealthContributor>(sp => sp.GetRequiredService<ICustomersPoliciesService>());
        return services;
    }
}
