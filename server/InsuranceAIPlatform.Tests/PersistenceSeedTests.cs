using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.Approval.Persistence;
using InsuranceAIPlatform.Services.AuditCost.Persistence;
using InsuranceAIPlatform.Services.Claims.Persistence;
using InsuranceAIPlatform.Services.CustomersPolicies.Persistence;
using InsuranceAIPlatform.Services.Documents.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// EF InMemory tests for seed correctness. No SQL Server required — pure InMemory provider.
/// These are additive tests; existing 35 tests are unaffected.
/// </summary>
public class PersistenceSeedTests
{
    // -----------------------------------------------------------------------
    // Helper: build an InMemory context with a unique DB per test
    // -----------------------------------------------------------------------

    private static CustomersPoliciesDbContext BuildCustomersPoliciesContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CustomersPoliciesDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new CustomersPoliciesDbContext(options);
    }

    private static ClaimsDbContext BuildClaimsContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ClaimsDbContext(options);
    }

    private static DocumentsDbContext BuildDocumentsContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new DocumentsDbContext(options);
    }

    private static ApprovalDbContext BuildApprovalContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApprovalDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApprovalDbContext(options);
    }

    private static AuditCostDbContext BuildAuditCostContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AuditCostDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AuditCostDbContext(options);
    }

    private static AiAnalysisDbContext BuildAiAnalysisContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AiAnalysisDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AiAnalysisDbContext(options);
    }

    // -----------------------------------------------------------------------
    // (S1) CustomersPolicies: exactly 200 synthetic customers
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CustomersPoliciesSeeder_produces_exactly_200_synthetic_customers()
    {
        await using var db = BuildCustomersPoliciesContext(nameof(CustomersPoliciesSeeder_produces_exactly_200_synthetic_customers));
        await CustomersPoliciesSeeder.SeedAsync(db);

        var syntheticCount = await db.SyntheticCustomers
            .Where(c => c.Id.StartsWith("CUST-T"))
            .CountAsync();

        Assert.Equal(SeedConstants.SyntheticUserCount, syntheticCount);
    }

    // -----------------------------------------------------------------------
    // (S2) All synthetic emails end with @example.invalid — no real PII
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CustomersPoliciesSeeder_all_synthetic_emails_end_with_invalid_domain()
    {
        await using var db = BuildCustomersPoliciesContext(nameof(CustomersPoliciesSeeder_all_synthetic_emails_end_with_invalid_domain));
        await CustomersPoliciesSeeder.SeedAsync(db);

        var badEmails = await db.SyntheticCustomers
            .Where(c => c.IsSynthetic && !c.Email.EndsWith($"@{SeedConstants.SyntheticEmailDomain}"))
            .CountAsync();

        Assert.Equal(0, badEmails);
    }

    // -----------------------------------------------------------------------
    // (S3) CustomersPolicies: seed is idempotent (second call = same count)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CustomersPoliciesSeeder_is_idempotent()
    {
        await using var db = BuildCustomersPoliciesContext(nameof(CustomersPoliciesSeeder_is_idempotent));
        await CustomersPoliciesSeeder.SeedAsync(db);
        await CustomersPoliciesSeeder.SeedAsync(db);  // second call

        var count = await db.SyntheticCustomers.Where(c => c.Id.StartsWith("CUST-T")).CountAsync();
        Assert.Equal(SeedConstants.SyntheticUserCount, count);
    }

    // -----------------------------------------------------------------------
    // (S4) Claims: CLM-1006 has golden field values
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ClaimsSeeder_CLM1006_has_golden_field_values()
    {
        await using var db = BuildClaimsContext(nameof(ClaimsSeeder_CLM1006_has_golden_field_values));
        await ClaimsSeeder.SeedAsync(db);

        var claim = await db.Claims.FindAsync("CLM-1006");
        Assert.NotNull(claim);
        Assert.Equal("CUST-4421", claim!.CustomerId);
        Assert.Equal("POL-2025-AC-4421", claim.PolicyId);
        Assert.Equal("Роберт Джонсон", claim.Customer);
        Assert.Equal("Toyota Camry 2021", claim.Vehicle);
        Assert.Equal("В роботі", claim.Status);
        Assert.Equal(82, claim.RiskScore);
        Assert.Equal(78, claim.Confidence);
        Assert.Equal(4261, claim.Tokens);
        Assert.Equal(0.0187m, claim.Cost);
        Assert.Equal("trc_8f3d2a7e", claim.TraceId);
        Assert.Equal("run_8f3d2a7e", claim.RunId);
        Assert.Equal("POL-2025-AC-4421", claim.PolicyId);
        Assert.Equal(2720.00m, claim.Estimate);
        Assert.Equal(1800.00m, claim.RecommendedPayout);
    }

    // -----------------------------------------------------------------------
    // (S5) Claims: more than 1 distinct status (dashboard variety)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ClaimsSeeder_has_status_variety()
    {
        await using var db = BuildClaimsContext(nameof(ClaimsSeeder_has_status_variety));
        await ClaimsSeeder.SeedAsync(db);

        var distinctStatuses = await db.Claims
            .Select(c => c.Status)
            .Distinct()
            .CountAsync();

        Assert.True(distinctStatuses > 1, $"Expected >1 distinct status, got {distinctStatuses}");
    }

    // -----------------------------------------------------------------------
    // (S6) Documents: CLM-1006 has exactly 7 documents
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DocumentsSeeder_CLM1006_has_7_documents()
    {
        await using var db = BuildDocumentsContext(nameof(DocumentsSeeder_CLM1006_has_7_documents));
        await DocumentsSeeder.SeedAsync(db);

        var count = await db.ClaimDocuments
            .Where(d => d.ClaimId == "CLM-1006")
            .CountAsync();

        Assert.Equal(7, count);
    }

    // -----------------------------------------------------------------------
    // (S7) Approval: CLM-1006 draft has 4 decision options, Submitted=false
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ApprovalSeeder_CLM1006_draft_has_4_options_and_not_submitted()
    {
        await using var db = BuildApprovalContext(nameof(ApprovalSeeder_CLM1006_draft_has_4_options_and_not_submitted));
        await ApprovalSeeder.SeedAsync(db);

        var draft = await db.ApprovalDrafts
            .Include(d => d.Options)
            .FirstOrDefaultAsync(d => d.ClaimId == SeedConstants.GoldenClaimId);

        Assert.NotNull(draft);
        Assert.False(draft!.Submitted);
        Assert.Equal(4, draft.Options.Count);
    }

    // -----------------------------------------------------------------------
    // (S8) AuditCost: CLM-1006 has 6 audit events, 4 cost rows, 1 token usage row
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AuditCostSeeder_CLM1006_has_correct_row_counts()
    {
        await using var db = BuildAuditCostContext(nameof(AuditCostSeeder_CLM1006_has_correct_row_counts));
        await AuditCostSeeder.SeedAsync(db);

        var events = await db.AuditEvents.Where(e => e.ClaimId == SeedConstants.GoldenClaimId).CountAsync();
        var costs  = await db.CostTraces.Where(c => c.ClaimId == SeedConstants.GoldenClaimId).CountAsync();
        var tokens = await db.TokenUsageTraces.Where(t => t.ClaimId == SeedConstants.GoldenClaimId).CountAsync();

        Assert.Equal(6, events);
        Assert.Equal(4, costs);
        Assert.Equal(1, tokens);
    }

    // -----------------------------------------------------------------------
    // (S9) AuditCost: BLOCK event is present
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AuditCostSeeder_has_BLOCK_event()
    {
        await using var db = BuildAuditCostContext(nameof(AuditCostSeeder_has_BLOCK_event));
        await AuditCostSeeder.SeedAsync(db);

        var hasBlock = await db.AuditEvents
            .AnyAsync(e => e.ClaimId == SeedConstants.GoldenClaimId && e.Severity == "BLOCK");

        Assert.True(hasBlock, "Expected a BLOCK severity audit event for CLM-1006");
    }

    // -----------------------------------------------------------------------
    // (S10) AiAnalysis: ProviderMode is never a real provider
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AiAnalysisSeeder_provider_mode_is_disabled_or_mock_only()
    {
        await using var db = BuildAiAnalysisContext(nameof(AiAnalysisSeeder_provider_mode_is_disabled_or_mock_only));
        await AiAnalysisSeeder.SeedAsync(db);

        var realProviderRows = await db.AiAnalysisRuns
            .Where(r => r.ProviderMode != "Disabled" && r.ProviderMode != "Mock")
            .CountAsync();

        Assert.Equal(0, realProviderRows);
    }

    // -----------------------------------------------------------------------
    // (S11) AiAnalysis: CLM-1006 run has correct golden values
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AiAnalysisSeeder_CLM1006_run_has_golden_values()
    {
        await using var db = BuildAiAnalysisContext(nameof(AiAnalysisSeeder_CLM1006_run_has_golden_values));
        await AiAnalysisSeeder.SeedAsync(db);

        var run = await db.AiAnalysisRuns
            .Include(r => r.Findings)
            .Include(r => r.EvidenceReferences)
            .Include(r => r.RiskSignals)
            .FirstOrDefaultAsync(r => r.RunId == "run_8f3d2a7e");

        Assert.NotNull(run);
        Assert.Equal("Disabled", run!.ProviderMode);
        Assert.Equal(78, run.ModelConfidence);
        Assert.Equal(4261, run.Tokens);
        Assert.Equal(0.0187m, run.Cost);
        Assert.Equal(3, run.Findings.Count);
        Assert.Equal(2, run.EvidenceReferences.Count);
    }

    // -----------------------------------------------------------------------
    // (S12) DbContext schema config: CustomersPoliciesDbContext uses 'customers_policies'
    // -----------------------------------------------------------------------

    [Fact]
    public void CustomersPoliciesDbContext_has_expected_default_schema()
    {
        var options = new DbContextOptionsBuilder<CustomersPoliciesDbContext>()
            .UseInMemoryDatabase("schema_test_cp")
            .Options;
        using var db = new CustomersPoliciesDbContext(options);
        var schema = db.Model.GetDefaultSchema();
        Assert.Equal("customers_policies", schema);
    }

    [Fact]
    public void ClaimsDbContext_has_expected_default_schema()
    {
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase("schema_test_cl")
            .Options;
        using var db = new ClaimsDbContext(options);
        Assert.Equal("claims", db.Model.GetDefaultSchema());
    }

    [Fact]
    public void DocumentsDbContext_has_expected_default_schema()
    {
        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseInMemoryDatabase("schema_test_doc")
            .Options;
        using var db = new DocumentsDbContext(options);
        Assert.Equal("documents", db.Model.GetDefaultSchema());
    }

    [Fact]
    public void ApprovalDbContext_has_expected_default_schema()
    {
        var options = new DbContextOptionsBuilder<ApprovalDbContext>()
            .UseInMemoryDatabase("schema_test_appr")
            .Options;
        using var db = new ApprovalDbContext(options);
        Assert.Equal("approval", db.Model.GetDefaultSchema());
    }

    [Fact]
    public void AuditCostDbContext_has_expected_default_schema()
    {
        var options = new DbContextOptionsBuilder<AuditCostDbContext>()
            .UseInMemoryDatabase("schema_test_ac")
            .Options;
        using var db = new AuditCostDbContext(options);
        Assert.Equal("audit_cost", db.Model.GetDefaultSchema());
    }

    [Fact]
    public void AiAnalysisDbContext_has_expected_default_schema()
    {
        var options = new DbContextOptionsBuilder<AiAnalysisDbContext>()
            .UseInMemoryDatabase("schema_test_ai")
            .Options;
        using var db = new AiAnalysisDbContext(options);
        Assert.Equal("ai_analysis", db.Model.GetDefaultSchema());
    }

    // -----------------------------------------------------------------------
    // (S13) Boundary: no entity type appears in two DbContexts
    // -----------------------------------------------------------------------

    [Fact]
    public void No_entity_type_is_shared_across_two_contexts()
    {
        // Build each context model and collect entity CLR types
        DbContext[] contexts = [
            BuildCustomersPoliciesContext("boundary_cp"),
            BuildClaimsContext("boundary_cl"),
            BuildDocumentsContext("boundary_doc"),
            BuildApprovalContext("boundary_appr"),
            BuildAuditCostContext("boundary_ac"),
            BuildAiAnalysisContext("boundary_ai"),
        ];

        try
        {
            var allEntityTypes = new List<(Type clrType, string contextName)>();
            foreach (var ctx in contexts)
            {
                var ctxName = ctx.GetType().Name;
                foreach (var et in ctx.Model.GetEntityTypes())
                    allEntityTypes.Add((et.ClrType, ctxName));
            }

            var duplicates = allEntityTypes
                .GroupBy(x => x.clrType)
                .Where(g => g.Select(x => x.contextName).Distinct().Count() > 1)
                .Select(g => $"{g.Key.Name} in [{string.Join(", ", g.Select(x => x.contextName).Distinct())}]")
                .ToList();

            Assert.True(duplicates.Count == 0, $"Entity types shared across contexts: {string.Join("; ", duplicates)}");
        }
        finally
        {
            foreach (var ctx in contexts)
                ctx.Dispose();
        }
    }
}
