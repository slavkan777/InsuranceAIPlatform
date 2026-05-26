using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

const string ViteDevCors = "ViteDevServer";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "InsuranceAIPlatform API",
        Version = "v0.1",
        Description =
            "Auto Insurance Claim AI Workbench — local demo backend skeleton. " +
            "Synthetic data only; this service performs no real claims operations. " +
            "AI outputs are advisory and human approval is always final. " +
            "Database and AI provider are planned future gates."
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

app.UseCors(ViteDevCors);
app.MapControllers();

app.Run();

// Exposed so the test project's WebApplicationFactory<Program> can boot the app.
public partial class Program { }
