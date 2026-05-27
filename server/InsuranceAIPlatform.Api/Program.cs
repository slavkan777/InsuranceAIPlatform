using InsuranceAIPlatform.Api.Middleware;
using InsuranceAIPlatform.Api.Services;
using InsuranceAIPlatform.Services.Claims;
using InsuranceAIPlatform.Services.CustomersPolicies;
using InsuranceAIPlatform.Services.Documents;
using InsuranceAIPlatform.Services.AiAnalysis;
using InsuranceAIPlatform.Services.Approval;
using InsuranceAIPlatform.Services.AuditCost;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

const string ViteDevCors = "ViteDevServer";

// In-memory claim service — singleton, deterministic, no DB dependency.
builder.Services.AddSingleton<IClaimReadService, InMemoryClaimReadService>();

// Internal service skeletons (Stage-2): registered in-process behind the BFF.
// Each is a boundary marker only — no DB, no writes, no AI provider, no data ownership yet.
// The existing read routes are unchanged; these skeletons prove the boundaries + DI seam.
builder.Services
    .AddClaimsServiceSkeleton()
    .AddCustomersPoliciesServiceSkeleton()
    .AddDocumentsServiceSkeleton()
    .AddAiAnalysisServiceSkeleton()
    .AddApprovalServiceSkeleton()
    .AddAuditCostServiceSkeleton();

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
