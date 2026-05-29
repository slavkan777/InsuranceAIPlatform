using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsuranceAIPlatform.Api.Controllers;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.Approval.Persistence;
using InsuranceAIPlatform.Services.AuditCost.Persistence;
using InsuranceAIPlatform.Services.Claims.Persistence;
using InsuranceAIPlatform.Services.CustomersPolicies;
using InsuranceAIPlatform.Services.CustomersPolicies.Persistence;
using InsuranceAIPlatform.Services.Documents.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Realistic-sandbox endpoint tests (gate
/// PRE_AZURE_REALISTIC_DB_SANDBOX_AND_TRIPLE_OWNER_FLOW_FIX_V0.1). Covers:
///   - GET /api/customers (list + count + search + paging)
///   - POST /api/claims (synthetic claim creation, audit + outbox)
///   - POST /api/claims/{id}/documents/upload (DB-backed text content)
///   - POST /api/claims/{id}/payout-simulation (SimulationOnly=true)
///   - GET  /api/claims/{id}/payout-simulations (listing)
/// All DbContexts are InMemory; customers DB is pre-seeded with 5 synthetic rows.
/// </summary>
public sealed class SandboxSurfaceTests : IClassFixture<SandboxTestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly SandboxTestWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public SandboxSurfaceTests(SandboxTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // -----------------------------------------------------------------------
    // (S1) Customers list — paginated; respects IsSynthetic filter
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetCustomers_returns_synthetic_only_paginated()
    {
        await _factory.EnsureCustomersSeeded();
        var response = await _client.GetAsync("/api/customers?page=1&pageSize=3");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(5, doc.RootElement.GetProperty("total").GetInt32());
        Assert.Equal(3, doc.RootElement.GetProperty("items").GetArrayLength());
        foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
        {
            Assert.True(item.GetProperty("isSynthetic").GetBoolean());
        }
    }

    [Fact]
    public async Task GetCustomers_search_filters_by_substring()
    {
        await _factory.EnsureCustomersSeeded();
        var response = await _client.GetAsync("/api/customers?search=T0002");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("items");
        Assert.Equal(1, items.GetArrayLength());
        Assert.Equal("CUST-T0002", items[0].GetProperty("id").GetString());
    }

    [Fact]
    public async Task GetCustomers_count_endpoint_returns_synthetic_count()
    {
        await _factory.EnsureCustomersSeeded();
        var response = await _client.GetAsync("/api/customers/count");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(5, doc.RootElement.GetProperty("count").GetInt32());
        Assert.True(doc.RootElement.GetProperty("syntheticOnly").GetBoolean());
    }

    // -----------------------------------------------------------------------
    // (S2) New claim creation — writes claim row + audit + outbox
    // -----------------------------------------------------------------------

    [Fact]
    public async Task PostClaims_creates_synthetic_claim_with_audit_and_outbox()
    {
        await _factory.EnsureCustomersSeeded();
        var body = new
        {
            customerId = "CUST-T0001",
            customerName = "Synthetic Customer 001",
            vehicle = "Honda Civic 2022",
            vehicleVin = "VIN ****1234",
            eventType = "ДТП",
            eventDate = "2026-05-25",
            location = "Київ, проспект Перемоги 50",
            description = "Test claim",
        };
        var response = await _client.PostAsJsonAsync("/api/claims", body);
        Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");

        var result = await response.Content.ReadFromJsonAsync<CreateClaimResult>(JsonOpts);
        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.StartsWith("CLM-", result.ClaimId);
        Assert.Equal("Новий", result.Status);
        Assert.NotNull(result.AuditEventId);
        Assert.True(result.AuditEventId > 0);
        Assert.NotNull(result.OutboxMessageId);
        Assert.True(result.OutboxMessageId > 0);
        Assert.Equal("CUST-T0001", result.CustomerId);
    }

    [Fact]
    public async Task PostClaims_unknown_customer_returns_404()
    {
        await _factory.EnsureCustomersSeeded();
        var body = new
        {
            customerId = "CUST-DOES-NOT-EXIST",
            vehicle = "X",
            eventType = "ДТП",
            eventDate = "2026-05-25",
            location = "X",
        };
        var response = await _client.PostAsJsonAsync("/api/claims", body);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // (S3) Document content upload — DB-backed content persisted
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UploadDocumentContent_persists_text_content_and_audit_outbox()
    {
        var body = new
        {
            kind = "police-report",
            title = "Test Police Report",
            docType = "PoliceReport",
            content = "ДТП на перехресті — синтетичний тестовий вміст.",
        };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/documents/upload", body);
        Assert.True(response.IsSuccessStatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("Uploaded", doc.RootElement.GetProperty("status").GetString());
        Assert.True(doc.RootElement.GetProperty("auditEventId").GetInt32() > 0);
        Assert.True(doc.RootElement.GetProperty("outboxMessageId").GetInt32() > 0);
    }

    [Fact]
    public async Task UploadDocumentContent_missing_content_returns_400()
    {
        var body = new { kind = "note", title = "X", content = "" };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/documents/upload", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // (S4) Payout simulation — DB-only with SimulationOnly=true
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreatePayoutSimulation_persists_simulation_with_safety_flag()
    {
        var body = new
        {
            amount = 1800m,
            deductible = 500m,
            currency = "USD",
            decisionSource = "Human",
            notes = "Sandbox simulation",
        };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/payout-simulation", body);
        Assert.True(response.IsSuccessStatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("DraftSimulated", doc.RootElement.GetProperty("status").GetString());
        Assert.Equal(1800m, doc.RootElement.GetProperty("amount").GetDecimal());
        Assert.Equal(1300m, doc.RootElement.GetProperty("netPayoutAmount").GetDecimal());
        Assert.Equal("USD", doc.RootElement.GetProperty("currency").GetString());
        Assert.True(doc.RootElement.GetProperty("simulationOnly").GetBoolean());
        Assert.True(doc.RootElement.GetProperty("auditEventId").GetInt32() > 0);
        Assert.True(doc.RootElement.GetProperty("outboxMessageId").GetInt32() > 0);
    }

    [Fact]
    public async Task CreatePayoutSimulation_invalid_amount_returns_400()
    {
        var body = new { amount = 0m, deductible = 0m };
        var response = await _client.PostAsJsonAsync("/api/claims/CLM-1006/payout-simulation", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListPayoutSimulations_returns_persisted_rows()
    {
        // Create one first
        var body = new { amount = 999.99m, deductible = 0m, currency = "USD", decisionSource = "Human" };
        await _client.PostAsJsonAsync("/api/claims/CLM-1006/payout-simulation", body);

        var listResponse = await _client.GetAsync("/api/claims/CLM-1006/payout-simulations");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var doc = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetArrayLength() >= 1);
        foreach (var sim in doc.RootElement.EnumerateArray())
        {
            // Schema-level guarantee — every row must carry SimulationOnly=true
            Assert.True(sim.GetProperty("simulationOnly").GetBoolean());
        }
    }
}

/// <summary>
/// Test factory with all 5 DbContexts replaced by InMemory + a seeded synthetic
/// customer set so the realistic-sandbox flows have inputs.
/// </summary>
public sealed class SandboxTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string SharedDbName = "SandboxTest_InMemory_Shared";
    private bool _seeded;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ReplaceWithInMemory<ApprovalDbContext>(services);
            ReplaceWithInMemory<AuditCostDbContext>(services);
            ReplaceWithInMemory<DocumentsDbContext>(services);
            ReplaceWithInMemory<ClaimsDbContext>(services);
            ReplaceWithInMemory<CustomersPoliciesDbContext>(services);
        });
    }

    public async Task EnsureCustomersSeeded()
    {
        if (_seeded) return;
        using var scope = Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CustomersPoliciesDbContext>>();
        await using var db = await factory.CreateDbContextAsync();
        if (!await db.SyntheticCustomers.AnyAsync())
        {
            for (var i = 1; i <= 5; i++)
            {
                db.SyntheticCustomers.Add(new SyntheticCustomer
                {
                    Id = $"CUST-T{i.ToString("0000")}",
                    FullName = $"Synthetic Customer {i:000}",
                    Email = $"testuser{i:000}@example.invalid",
                    Phone = $"+1-555-{1000 + i}",
                    AddressLine = "Local sandbox address",
                    CustomerSince = new DateOnly(2024, 1, 1),
                    PreviousClaimsCount = 0,
                    IsSynthetic = true,
                });
            }
            await db.SaveChangesAsync();
        }
        _seeded = true;
    }

    private static void ReplaceWithInMemory<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
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

        services.AddDbContextFactory<TContext>(options =>
            options.UseInMemoryDatabase(SharedDbName), ServiceLifetime.Singleton);
    }
}
