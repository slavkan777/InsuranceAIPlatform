using InsuranceAIPlatform.BuildingBlocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.CustomersPolicies.Persistence;

/// <summary>
/// DI registration for CustomersPolicies persistence. Registers the DbContextFactory
/// and replaces the skeleton <see cref="ICustomersPoliciesService"/> with the DB-backed
/// <see cref="PersistenceCustomersPoliciesService"/>.
/// </summary>
public static class CustomersPoliciesPersistenceExtensions
{
    public static IServiceCollection AddCustomersPoliciesPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContextFactory<CustomersPoliciesDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "customers_policies")),
            ServiceLifetime.Singleton);

        if (!services.Any(d => d.ServiceType == typeof(IClock)))
            services.AddSingleton<IClock, SystemClock>();

        services.AddSingleton<ICustomersPoliciesService, PersistenceCustomersPoliciesService>();

        return services;
    }
}
