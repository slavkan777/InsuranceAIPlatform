using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Embedding;
using InsuranceAIPlatform.Services.AiAnalysis.Rag.Persistence;
using InsuranceAIPlatform.Services.Approval.Persistence;
using InsuranceAIPlatform.Services.AuditCost.Persistence;
using InsuranceAIPlatform.Services.Claims.Persistence;
using InsuranceAIPlatform.Services.CustomersPolicies.Persistence;
using InsuranceAIPlatform.Services.Documents.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=== InsuranceAIPlatform DbMigrator ===");
Console.WriteLine();

// Resolve connection string: env override → default LocalDB
var connectionString =
    Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringConfigKey)
    ?? Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringEnvVar)
    ?? SeedConstants.DefaultConnectionString;

// Print non-secret summary only — never print the connection string
Console.WriteLine($"Target DB:  InsuranceAIPlatform on (localdb)\\MSSQLLocalDB");
Console.WriteLine();

// Build service collection with all 6 DbContexts
var services = new ServiceCollection();
services.AddCustomersPoliciesPersistence(connectionString);
services.AddClaimsPersistence(connectionString);
services.AddDocumentsPersistence(connectionString);
services.AddApprovalPersistence(connectionString);
services.AddAuditCostPersistence(connectionString);
services.AddAiAnalysisPersistence(connectionString);

await using var provider = services.BuildServiceProvider();

var cts = new CancellationTokenSource();
var ct = cts.Token;

// Migrate + seed each bounded context
await MigrateAndSeedAsync<CustomersPoliciesDbContext>(
    provider, "customers_policies", async db =>
    {
        await CustomersPoliciesSeeder.SeedAsync((CustomersPoliciesDbContext)db, ct);
        return new[]
        {
            ($"SyntheticCustomer", await ((CustomersPoliciesDbContext)db).SyntheticCustomers.CountAsync(ct)),
            ($"Policy",            await ((CustomersPoliciesDbContext)db).Policies.CountAsync(ct)),
            ($"Vehicle",           await ((CustomersPoliciesDbContext)db).Vehicles.CountAsync(ct)),
        };
    }, ct);

await MigrateAndSeedAsync<ClaimsDbContext>(
    provider, "claims", async db =>
    {
        await ClaimsSeeder.SeedAsync((ClaimsDbContext)db, ct);
        return new[]
        {
            ($"Claim", await ((ClaimsDbContext)db).Claims.CountAsync(ct)),
        };
    }, ct);

await MigrateAndSeedAsync<DocumentsDbContext>(
    provider, "documents", async db =>
    {
        await DocumentsSeeder.SeedAsync((DocumentsDbContext)db, ct);
        return new[]
        {
            ($"ClaimDocument", await ((DocumentsDbContext)db).ClaimDocuments.CountAsync(ct)),
        };
    }, ct);

await MigrateAndSeedAsync<ApprovalDbContext>(
    provider, "approval", async db =>
    {
        await ApprovalSeeder.SeedAsync((ApprovalDbContext)db, ct);
        return new[]
        {
            ($"ApprovalDraft",          await ((ApprovalDbContext)db).ApprovalDrafts.CountAsync(ct)),
            ($"ApprovalDecisionOption", await ((ApprovalDbContext)db).ApprovalDecisionOptions.CountAsync(ct)),
        };
    }, ct);

await MigrateAndSeedAsync<AuditCostDbContext>(
    provider, "audit_cost", async db =>
    {
        await AuditCostSeeder.SeedAsync((AuditCostDbContext)db, ct);
        return new[]
        {
            ($"AuditEvent",      await ((AuditCostDbContext)db).AuditEvents.CountAsync(ct)),
            ($"CostTrace",       await ((AuditCostDbContext)db).CostTraces.CountAsync(ct)),
            ($"TokenUsageTrace", await ((AuditCostDbContext)db).TokenUsageTraces.CountAsync(ct)),
        };
    }, ct);

await MigrateAndSeedAsync<AiAnalysisDbContext>(
    provider, "ai_analysis", async db =>
    {
        await AiAnalysisSeeder.SeedAsync((AiAnalysisDbContext)db, ct);
        // Local RAG foundation seed (deterministic embeddings; same ai_analysis schema).
        var ragEmbed = new DeterministicEmbeddingProvider();
        await RagSeeder.SeedAsync((AiAnalysisDbContext)db, ragEmbed, ct);
        return new[]
        {
            ($"AiAnalysisRun",      await ((AiAnalysisDbContext)db).AiAnalysisRuns.CountAsync(ct)),
            ($"AiFinding",          await ((AiAnalysisDbContext)db).AiFindings.CountAsync(ct)),
            ($"AiEvidenceReference",await ((AiAnalysisDbContext)db).AiEvidenceReferences.CountAsync(ct)),
            ($"AiRiskSignal",       await ((AiAnalysisDbContext)db).AiRiskSignals.CountAsync(ct)),
            ($"PolicyClause",       await ((AiAnalysisDbContext)db).PolicyClauses.CountAsync(ct)),
            ($"EvidenceChunk",      await ((AiAnalysisDbContext)db).EvidenceChunks.CountAsync(ct)),
            ($"RagEvaluationQuestion", await ((AiAnalysisDbContext)db).RagEvaluationQuestions.CountAsync(ct)),
            ($"RagAuditTrace",      await ((AiAnalysisDbContext)db).RagAuditTraces.CountAsync(ct)),
        };
    }, ct);

Console.WriteLine();
Console.WriteLine("=== Migration + Seed complete ===");

// -----------------------------------------------------------------------
static async Task MigrateAndSeedAsync<TContext>(
    IServiceProvider provider,
    string schema,
    Func<DbContext, Task<(string table, int count)[]>> seedAndCount,
    CancellationToken ct)
    where TContext : DbContext
{
    Console.WriteLine($"[{schema}] Migrating...");
    await using var scope = provider.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<TContext>();
    await db.Database.MigrateAsync(ct);
    Console.WriteLine($"[{schema}] Migration applied.");

    Console.WriteLine($"[{schema}] Seeding...");
    var counts = await seedAndCount(db);
    foreach (var (table, count) in counts)
        Console.WriteLine($"  {schema}.{table}: {count} rows");
}
