using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.DbMigrator;
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
// Explicit English-only data backfill step.
//
// This runs ONLY here, in the migrator — never implicitly at API startup.
// It is versioned, idempotent and non-destructive: UPDATEs only, no deletes,
// and rerunning it is a no-op once the data is already English.
// -----------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine($"=== Data backfill: {EnglishOnlyBackfill.Version} ===");

await using (var scope = provider.CreateAsyncScope())
{
    var claimsDb    = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
    var documentsDb = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
    var customersDb = scope.ServiceProvider.GetRequiredService<CustomersPoliciesDbContext>();
    var aiDb        = scope.ServiceProvider.GetRequiredService<AiAnalysisDbContext>();
    var auditDb     = scope.ServiceProvider.GetRequiredService<AuditCostDbContext>();
    var approvalDb  = scope.ServiceProvider.GetRequiredService<ApprovalDbContext>();

    var alreadyApplied = await EnglishOnlyBackfill.WasAppliedAsync(claimsDb, ct);
    Console.WriteLine($"  checkpoint: {(alreadyApplied ? "version already recorded — rerunning is safe (expect 0 updates)" : "not yet applied")}");

    var report = await EnglishOnlyBackfill.RunAsync(
        claimsDb, documentsDb, customersDb, aiDb, auditDb, approvalDb,
        new DeterministicEmbeddingProvider(), ct);

    Console.WriteLine($"  updated:          {report.Updated}");
    Console.WriteLine($"  already English:  {report.AlreadyEnglish}");
    Console.WriteLine($"  chunks re-embedded: {report.ChunksReEmbedded}");
    Console.WriteLine($"  skipped (left untouched): {report.Skipped}");
    foreach (var sample in report.SkippedSamples)
        Console.WriteLine($"    - {sample}");
    Console.WriteLine($"=== Data backfill complete: {report.Version} ===");

    // ---- Lease 6 closure pass: runtime/E2E-created rows -----------------
    Console.WriteLine();
    Console.WriteLine($"=== Data backfill: {EnglishOnlyClosureBackfill.Version} ===");
    var closureApplied = await EnglishOnlyClosureBackfill.WasAppliedAsync(claimsDb, ct);
    Console.WriteLine($"  checkpoint: {(closureApplied ? "version already recorded — rerunning is safe (expect 0 updates)" : "not yet applied")}");

    var closure = await EnglishOnlyClosureBackfill.RunAsync(
        claimsDb, documentsDb, customersDb, aiDb, auditDb,
        new DeterministicEmbeddingProvider(), ct);

    Console.WriteLine($"  updated:          {closure.Updated}");
    Console.WriteLine($"  already English:  {closure.AlreadyEnglish}");
    Console.WriteLine($"  chunks re-embedded: {closure.ChunksReEmbedded}");
    Console.WriteLine($"  skipped (left untouched): {closure.Skipped}");
    foreach (var sample in closure.SkippedSamples)
        Console.WriteLine($"    - {sample}");
    Console.WriteLine($"=== Data backfill complete: {closure.Version} ===");

    // ---- Lease 8: historical synthetic/demo columns --------------------
    Console.WriteLine();
    Console.WriteLine($"=== Data backfill: {EnglishOnlyHistoricalBackfill.Version} ===");
    var histApplied = await EnglishOnlyHistoricalBackfill.WasAppliedAsync(claimsDb, ct);
    Console.WriteLine($"  checkpoint: {(histApplied ? "version already recorded — rerunning is safe (expect 0 updates)" : "not yet applied")}");

    var hist = await EnglishOnlyHistoricalBackfill.RunAsync(aiDb, documentsDb, approvalDb, claimsDb, ct);
    Console.WriteLine($"  updated:          {hist.Updated}");
    Console.WriteLine($"  already English:  {hist.AlreadyEnglish}");
    Console.WriteLine($"=== Data backfill complete: {hist.Version} ===");

    // ---- Lease 10: JSON-escaped content + stale language tags ----------
    Console.WriteLine();
    Console.WriteLine($"=== Data backfill: {EnglishOnlyJsonBackfill.Version} ===");
    var jsonApplied = await EnglishOnlyJsonBackfill.WasAppliedAsync(claimsDb, ct);
    Console.WriteLine($"  checkpoint: {(jsonApplied ? "version already recorded — rerunning is safe (expect 0 updates)" : "not yet applied")}");
    var js = await EnglishOnlyJsonBackfill.RunAsync(aiDb, claimsDb, ct);
    Console.WriteLine($"  updated:          {js.Updated}");
    Console.WriteLine($"  already English:  {js.AlreadyEnglish}");
    Console.WriteLine($"  skipped:          {js.Skipped}");
    foreach (var smp in js.SkippedSamples) Console.WriteLine($"    - {smp}");
    Console.WriteLine($"=== Data backfill complete: {js.Version} ===");

    // ---- Lease 11: audit/outbox history JSON ---------------------------
    Console.WriteLine();
    Console.WriteLine($"=== Data backfill: {EnglishOnlyAuditJsonBackfill.Version} ===");
    var auditApplied = await EnglishOnlyAuditJsonBackfill.WasAppliedAsync(claimsDb, ct);
    Console.WriteLine($"  checkpoint: {(auditApplied ? "version already recorded — rerunning is safe (expect 0 updates)" : "not yet applied")}");
    var aj = await EnglishOnlyAuditJsonBackfill.RunAsync(auditDb, claimsDb, ct);
    Console.WriteLine($"  updated:          {aj.Updated}");
    Console.WriteLine($"  already English:  {aj.AlreadyEnglish}");
    Console.WriteLine($"  blocked (unchanged): {aj.Skipped}");
    foreach (var smp in aj.SkippedSamples) Console.WriteLine($"    - {smp}");
    Console.WriteLine($"=== Data backfill complete: {aj.Version} ===");

    // ---- Lease 15: interactively-created demo residue ------------------
    Console.WriteLine();
    Console.WriteLine($"=== Data backfill: {EnglishOnlyDemoResidueBackfill.Version} ===");
    var residueApplied = await EnglishOnlyDemoResidueBackfill.WasAppliedAsync(claimsDb, ct);
    Console.WriteLine($"  checkpoint: {(residueApplied ? "version already recorded — rerunning is safe (expect 0 updates)" : "not yet applied")}");
    var residue = await EnglishOnlyDemoResidueBackfill.RunAsync(
        claimsDb, aiDb, new DeterministicEmbeddingProvider(), claimsDb, ct);
    Console.WriteLine($"  updated:          {residue.Updated}");
    Console.WriteLine($"  already English:  {residue.AlreadyEnglish}");
    Console.WriteLine($"  chunks re-embedded: {residue.ChunksReEmbedded}");
    Console.WriteLine($"  skipped (left untouched): {residue.Skipped}");
    foreach (var smp in residue.SkippedSamples) Console.WriteLine($"    - {smp}");
    Console.WriteLine($"=== Data backfill complete: {residue.Version} ===");
}

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
