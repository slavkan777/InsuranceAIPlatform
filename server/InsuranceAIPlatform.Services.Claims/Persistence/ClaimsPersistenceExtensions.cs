using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.Claims.Persistence;

public static class ClaimsPersistenceExtensions
{
    public static IServiceCollection AddClaimsPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ClaimsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "claims")));

        return services;
    }
}
