using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InsuranceAIPlatform.Api.Contracts;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis;
using InsuranceAIPlatform.Services.Approval;
using InsuranceAIPlatform.Services.AuditCost;
using InsuranceAIPlatform.Services.Claims;
using InsuranceAIPlatform.Services.CustomersPolicies;
using InsuranceAIPlatform.Services.Documents;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Stage-2 internal service skeleton tests: DI registration/resolution behind the BFF, readiness
/// metadata, the dependency-direction boundary (services never reference each other; BuildingBlocks
/// references no service/API; no EF anywhere), and the additive BFF health contract.
/// In-process via WebApplicationFactory — no DB, no network, no AI provider.
/// </summary>
public class ServiceSkeletonTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public static IEnumerable<object[]> ServiceInterfaceTypes() =>
    [
        [typeof(IClaimsService)],
        [typeof(ICustomersPoliciesService)],
        [typeof(IDocumentsService)],
        [typeof(IAiAnalysisService)],
        [typeof(IApprovalService)],
        [typeof(IAuditCostService)],
    ];

    // (1) Every service interface resolves from the BFF's DI container as a health contributor.
    [Theory]
    [MemberData(nameof(ServiceInterfaceTypes))]
    public void Service_interface_resolves_from_bff_di(Type serviceInterface)
    {
        using var scope = _factory.Services.CreateScope();
        var resolved = scope.ServiceProvider.GetService(serviceInterface);
        Assert.NotNull(resolved);
        Assert.IsAssignableFrom<IServiceHealthContributor>(resolved);
    }

    // (2) Exactly six distinct service health contributors are registered.
    [Fact]
    public void Six_service_health_contributors_are_registered()
    {
        using var scope = _factory.Services.CreateScope();
        var contributors = scope.ServiceProvider.GetServices<IServiceHealthContributor>().ToList();
        Assert.Equal(6, contributors.Count);

        var names = contributors.Select(c => c.GetHealth().ServiceName).ToHashSet();
        Assert.Equal(6, names.Count); // all distinct
        Assert.Contains(ServiceNames.Claims, names);
        Assert.Contains(ServiceNames.CustomersPolicies, names);
        Assert.Contains(ServiceNames.Documents, names);
        Assert.Contains(ServiceNames.AiAnalysis, names);
        Assert.Contains(ServiceNames.Approval, names);
        Assert.Contains(ServiceNames.AuditCost, names);
    }

    // (3) Health snapshots report expected readiness + non-empty capabilities.
    [Fact]
    public void Service_health_reports_expected_readiness()
    {
        using var scope = _factory.Services.CreateScope();
        var byName = scope.ServiceProvider.GetServices<IServiceHealthContributor>()
            .Select(c => c.GetHealth())
            .ToDictionary(s => s.ServiceName);

        foreach (var snap in byName.Values)
        {
            Assert.Equal("skeleton-v0.1", snap.Stage);
            Assert.NotEmpty(snap.Capabilities);
        }

        // AI Analysis is deferred (provider integration is a later gate); the rest are stubs.
        Assert.Equal(ServiceReadinessStatus.Deferred, byName[ServiceNames.AiAnalysis].Status);
        Assert.Equal(ServiceReadinessStatus.Stub, byName[ServiceNames.Claims].Status);
        Assert.Equal(ServiceReadinessStatus.Stub, byName[ServiceNames.AuditCost].Status);
    }

    // (4) AI Analysis wires a provider and stays advisory-only — no real AI call is possible.
    // Updated for gate AddAiAnalysisRunStructuredFields: IAiProvider is now MockAiProvider (no HTTP).
    [Fact]
    public void AiAnalysis_skeleton_has_no_provider_and_is_advisory_only()
    {
        using var scope = _factory.Services.CreateScope();
        var ai = scope.ServiceProvider.GetRequiredService<IAiAnalysisService>();
        Assert.True(ai.AdvisoryOnly);

        // IAiProvider is now MockAiProvider (deterministic local mock, no HTTP, no real provider).
        var provider = scope.ServiceProvider.GetService<IAiProvider>();
        Assert.NotNull(provider);
        Assert.Equal(AiProviderMode.Mock, provider!.Mode);
    }

    // (5) BFF health additively surfaces the six skeleton services without breaking identity fields.
    [Fact]
    public async Task BffHealth_lists_six_internal_services()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/bff/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<BffHealthResponse>(JsonOpts);
        Assert.NotNull(body);
        // Pre-existing identity fields still present (no contract break).
        Assert.Equal("bff-api-gateway", body!.Service);
        Assert.Equal("healthy", body.Status);
        Assert.Equal("skeleton-v0.1", body.Stage);
        // Additive readiness list with all six services.
        Assert.NotNull(body.Services);
        Assert.Equal(6, body.Services.Count);
    }

    // (6) Dependency boundary: no Services.* assembly references another Services.* assembly.
    [Fact]
    public void Services_do_not_reference_each_other()
    {
        var serviceAssemblies = new[]
        {
            typeof(ClaimsService).Assembly,
            typeof(CustomersPoliciesService).Assembly,
            typeof(DocumentsService).Assembly,
            typeof(AiAnalysisService).Assembly,
            typeof(ApprovalService).Assembly,
            typeof(AuditCostService).Assembly,
        };
        var serviceAsmNames = serviceAssemblies.Select(a => a.GetName().Name!).ToHashSet();

        foreach (var asm in serviceAssemblies)
        {
            var self = asm.GetName().Name!;
            var referencedSiblings = asm.GetReferencedAssemblies()
                .Select(r => r.Name!)
                .Where(n => serviceAsmNames.Contains(n) && n != self)
                .ToList();
            Assert.True(
                referencedSiblings.Count == 0,
                $"{self} must not reference sibling services: {string.Join(", ", referencedSiblings)}");
        }
    }

    // (7) BuildingBlocks references no service and no API assembly (clean shared-kernel direction).
    [Fact]
    public void BuildingBlocks_references_no_service_or_api()
    {
        var referenced = typeof(IServiceHealthContributor).Assembly
            .GetReferencedAssemblies().Select(r => r.Name!).ToList();
        Assert.DoesNotContain(referenced, n => n.StartsWith("InsuranceAIPlatform.Services."));
        Assert.DoesNotContain(referenced, n => n == "InsuranceAIPlatform.Api");
    }

    // (8) BuildingBlocks and BFF/API do not reference Entity Framework directly.
    //     Service assemblies now own EF (persistence added in Stage-3) — excluded here.
    [Fact]
    public void BuildingBlocks_and_Api_do_not_reference_entity_framework()
    {
        var assemblies = new[]
        {
            typeof(Program).Assembly,                    // BFF / API
            typeof(IServiceHealthContributor).Assembly,  // BuildingBlocks
        };
        foreach (var asm in assemblies)
        {
            var ef = asm.GetReferencedAssemblies()
                .Select(r => r.Name!)
                .Where(n => n.Contains("EntityFramework", StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.True(ef.Count == 0, $"{asm.GetName().Name} must not reference EF: {string.Join(", ", ef)}");
        }
    }
}
