using InsuranceAIPlatform.Services.Approval.Persistence;
using InsuranceAIPlatform.Services.AuditCost.Persistence;
using InsuranceAIPlatform.Services.Documents.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Custom WebApplicationFactory for command endpoint tests.
/// Replaces the SqlServer DbContextFactory registrations for the three command-owning
/// bounded contexts with InMemory providers so all command tests run without a live DB.
/// The read-route services (IClaimReadService, skeleton health contributors) remain unmodified.
/// </summary>
public sealed class CommandTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string SharedDbName = "CommandTest_InMemory_Shared";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace all SqlServer/options registrations for each command DbContext with InMemory.
            // We remove every service descriptor whose implementation type or service type
            // is related to the target DbContext, then re-add with InMemory.
            ReplaceWithInMemory<AuditCostDbContext>(services);
            ReplaceWithInMemory<ApprovalDbContext>(services);
            ReplaceWithInMemory<DocumentsDbContext>(services);
        });
    }

    private static void ReplaceWithInMemory<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        // Remove every descriptor associated with TContext
        var toRemove = services
            .Where(d =>
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericArguments().Any(a => a == typeof(TContext))) ||
                (d.ImplementationType is not null && d.ImplementationType == typeof(TContext)) ||
                (d.ServiceType == typeof(DbContextOptions<TContext>)) ||
                (d.ServiceType == typeof(DbContextOptions) && d.ImplementationType == typeof(DbContextOptions<TContext>)))
            .ToList();

        foreach (var d in toRemove)
            services.Remove(d);

        // Re-add with InMemory (singleton to match the existing pattern)
        services.AddDbContextFactory<TContext>(options =>
            options.UseInMemoryDatabase(SharedDbName), ServiceLifetime.Singleton);
    }
}
