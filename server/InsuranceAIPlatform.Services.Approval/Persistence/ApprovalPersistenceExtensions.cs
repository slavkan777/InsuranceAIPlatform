using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Services.Approval.Persistence;

public static class ApprovalPersistenceExtensions
{
    public static IServiceCollection AddApprovalPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ApprovalDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "approval")));

        return services;
    }
}
