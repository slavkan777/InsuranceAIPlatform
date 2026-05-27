using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.CustomersPolicies.Persistence;

/// <summary>
/// DI registration for CustomersPolicies persistence (additive — does not replace skeleton health wiring).
/// </summary>
public static class CustomersPoliciesPersistenceExtensions
{
    public static IServiceCollection AddCustomersPoliciesPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<CustomersPoliciesDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "customers_policies")));

        return services;
    }
}
