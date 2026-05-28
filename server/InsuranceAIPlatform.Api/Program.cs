using InsuranceAIPlatform.Api.Middleware;
using InsuranceAIPlatform.Api.Services;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.Claims;
using InsuranceAIPlatform.Services.CustomersPolicies;
using InsuranceAIPlatform.Services.Documents;
using InsuranceAIPlatform.Services.Documents.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis;
using InsuranceAIPlatform.Services.AiAnalysis.Configuration;
using InsuranceAIPlatform.Services.AiAnalysis.Guardrails;
using InsuranceAIPlatform.Services.AiAnalysis.Orchestration;
using InsuranceAIPlatform.Services.AiAnalysis.Persistence;
using InsuranceAIPlatform.Services.AiAnalysis.Providers;
using InsuranceAIPlatform.Services.Approval;
using InsuranceAIPlatform.Services.Approval.Persistence;
using InsuranceAIPlatform.Services.AuditCost;
using InsuranceAIPlatform.Services.AuditCost.Persistence;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

const string ViteDevCors = "ViteDevServer";

// -----------------------------------------------------------------------
// Connection string resolution:
//   1. ConnectionStrings:InsuranceAIPlatform (appsettings.json)
//   2. INSURANCEAI_CONNECTION_STRING env var
//   3. SeedConstants.DefaultConnectionString (LocalDB fallback)
// DB contexts use lazy connection — tests without a DB stay green
// -----------------------------------------------------------------------
var connectionString =
    builder.Configuration.GetConnectionString(SeedConstants.ConnectionStringName)
    ?? Environment.GetEnvironmentVariable(SeedConstants.ConnectionStringEnvVar)
    ?? SeedConstants.DefaultConnectionString;

// IClock — registered once, used by all persistence services
builder.Services.AddSingleton<IClock, SystemClock>();

// In-memory claim service — singleton, deterministic, no DB dependency.
builder.Services.AddSingleton<IClaimReadService, InMemoryClaimReadService>();

// Internal service skeletons (Stage-2): registered in-process behind the BFF.
// Each is a boundary marker only — no AI provider, no data ownership yet.
// The existing read routes are unchanged; these skeletons prove the boundaries + DI seam.
// Persistence extensions below REPLACE the skeleton impls for command-owning services.
builder.Services
    .AddClaimsServiceSkeleton()
    .AddCustomersPoliciesServiceSkeleton()
    .AddDocumentsServiceSkeleton()
    .AddAiAnalysisServiceSkeleton()
    .AddApprovalServiceSkeleton()
    .AddAuditCostServiceSkeleton();

// -----------------------------------------------------------------------
// Stage-3 persistence: register DbContexts + DB-backed service impls.
// AddDbContext does NOT open a connection until a query is issued, so
// the existing read-only route tests stay green without a live DB.
// -----------------------------------------------------------------------
builder.Services.AddApprovalPersistence(connectionString);
builder.Services.AddDocumentsPersistence(connectionString);
builder.Services.AddAuditCostPersistence(connectionString);

// -----------------------------------------------------------------------
// AI provider mode: Mock by default; DeepSeek adapter is disabled (no HTTP,
// never reads DEEPSEEK_API_KEY); orchestrator persists structured output +
// audit + outbox; AI is advisory-only.
// -----------------------------------------------------------------------
var aiOptions = builder.Configuration.GetSection("AiProvider").Get<AiProviderOptions>() ?? new AiProviderOptions();

// Defense-in-depth: if RealCallsEnabled=true AND Mode="DeepSeek", log warning and force Mock.
if (aiOptions.RealCallsEnabled && aiOptions.Mode.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddLogging();  // Ensure logging is registered
    // Will log at runtime below via the app logger — force mode to Mock
    aiOptions.Mode = "Mock";
}

// Register guardrail evaluator
builder.Services.AddSingleton<IGuardrailEvaluator, AdvisoryOnlyGuardrailEvaluator>();

// Register AI provider based on mode
if (aiOptions.Mode.Equals("DeepSeekDisabled", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IAiProvider, DisabledDeepSeekAiProvider>();
}
else
{
    // Default: Mock (or unknown mode defaults to Mock)
    builder.Services.AddSingleton<IAiProvider, MockAiProvider>();
}

builder.Services.AddAiAnalysisPersistence(connectionString);

// Claim existence delegate — wires IClaimReadService (Api layer) into the orchestrator (Service layer)
// without creating a circular project reference.
builder.Services.AddSingleton<Func<string, bool>>(sp =>
{
    var claimRead = sp.GetRequiredService<IClaimReadService>();
    return claimId => claimRead.GetClaim(claimId) is not null;
});

// Audit/outbox delegates — wires IAuditCostService (AuditCost layer) into orchestrator without
// a cross-service assembly reference (Services.AiAnalysis must not reference Services.AuditCost).
builder.Services.AddSingleton<InsuranceAIPlatform.Services.AiAnalysis.Orchestration.AppendAuditDelegate>(sp =>
{
    var audit = sp.GetRequiredService<IAuditCostService>();
    return (claimId, actionType, actor, correlationId, severity, message, meta, ct) =>
        audit.AppendAuditAsync(claimId, actionType, actor, correlationId, severity, message, meta, ct);
});

builder.Services.AddSingleton<InsuranceAIPlatform.Services.AiAnalysis.Orchestration.WriteOutboxDelegate>(sp =>
{
    var audit = sp.GetRequiredService<IAuditCostService>();
    return async (eventType, claimId, correlationId, payloadJson, idempotencyKey, ct) =>
    {
        await audit.WriteOutboxAsync(eventType, claimId, correlationId, payloadJson, idempotencyKey, ct);
    };
});

builder.Services.AddSingleton<IAiAnalysisOrchestrator, PersistenceAiAnalysisOrchestrator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "InsuranceAIPlatform BFF / API Gateway",
        Version = "v0.1",
        Description =
            "Auto Insurance Claim AI Workbench — BFF / API Gateway (Stage-1 skeleton). " +
            "Synthetic data only; this service performs no real claims operations. " +
            "AI outputs are advisory and human approval is always final. " +
            "Database and AI provider are planned future gates. " +
            "Correlation-id tracking: X-Correlation-Id header is echoed on all responses."
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(ViteDevCors, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "InsuranceAIPlatform API v0.1");
        options.DocumentTitle = "InsuranceAIPlatform API";
    });
}

// Correlation-id middleware registered early so all downstream components (controllers,
// health checks, error handlers) see the resolved id via HttpContext.Items.
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseCors(ViteDevCors);
app.MapControllers();

app.Run();

// Exposed so the test project's WebApplicationFactory<Program> can boot the app.
public partial class Program { }
