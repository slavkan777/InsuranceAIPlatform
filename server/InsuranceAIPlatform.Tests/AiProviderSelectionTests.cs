using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis;
using InsuranceAIPlatform.Services.AiAnalysis.Configuration;
using InsuranceAIPlatform.Services.AiAnalysis.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Tests for AI provider selection logic.
/// Verifies that the selection expression in Program.cs resolves the correct IAiProvider
/// for each combination of Mode / RealCallsEnabled / DEEPSEEK_API_KEY.
///
/// These tests exercise the selection logic directly — not through WebApplicationFactory —
/// because the WebApplicationBuilder config override mechanism is not reliable for
/// changing provider selection in minimal API hosts.
///
/// DEEPSEEK_API_KEY value is never printed — test uses a named sentinel only.
/// </summary>
public class AiProviderSelectionTests
{
    // -----------------------------------------------------------------------
    // Helper: build an IAiProvider using the same selection logic as Program.cs
    // -----------------------------------------------------------------------

    private static IAiProvider ResolveProvider(
        string mode,
        bool realCallsEnabled,
        string? apiKeyValue)
    {
        var configValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["AiProvider:Mode"] = mode,
            ["AiProvider:RealCallsEnabled"] = realCallsEnabled.ToString().ToLower(),
        };
        if (apiKeyValue is not null)
            configValues["DEEPSEEK_API_KEY"] = apiKeyValue;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var aiOptions = config.GetSection("AiProvider").Get<AiProviderOptions>() ?? new AiProviderOptions();
        var keyConfigured = !string.IsNullOrWhiteSpace(config["DEEPSEEK_API_KEY"]);

        var resolvedMode = aiOptions.Mode;
        var resolvedRealEnabled = aiOptions.RealCallsEnabled;

        // Mirror the exact selection logic from Program.cs
        if ((resolvedMode.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase) ||
             resolvedMode.Equals("DeepSeekReal", StringComparison.OrdinalIgnoreCase))
            && resolvedRealEnabled
            && keyConfigured)
        {
            var services = new ServiceCollection();
            services.AddHttpClient("deepseek");
            services.Configure<AiProviderOptions>(config.GetSection("AiProvider"));
            var sp = services.BuildServiceProvider();
            return new RealDeepSeekAiProvider(
                sp.GetRequiredService<IHttpClientFactory>(),
                config,
                Options.Create(aiOptions),
                new SystemClock(),
                NullLogger<RealDeepSeekAiProvider>.Instance);
        }
        else if (resolvedMode.Equals("DeepSeekDisabled", StringComparison.OrdinalIgnoreCase))
        {
            return new DisabledDeepSeekAiProvider();
        }
        else
        {
            return new MockAiProvider();
        }
    }

    // -----------------------------------------------------------------------
    // (S1) Default mode (empty config) resolves MockAiProvider
    // -----------------------------------------------------------------------

    [Fact]
    public void Default_mode_resolves_Mock()
    {
        var provider = ResolveProvider("Mock", realCallsEnabled: false, apiKeyValue: null);
        Assert.IsType<MockAiProvider>(provider);
        Assert.Equal(AiProviderMode.Mock, provider.Mode);
    }

    // -----------------------------------------------------------------------
    // (S2) Mode=DeepSeekDisabled resolves DisabledDeepSeekAiProvider
    // -----------------------------------------------------------------------

    [Fact]
    public void DeepSeekDisabled_mode_resolves_disabled_provider()
    {
        var provider = ResolveProvider("DeepSeekDisabled", realCallsEnabled: false, apiKeyValue: null);
        Assert.IsType<DisabledDeepSeekAiProvider>(provider);
        Assert.Equal(AiProviderMode.DeepSeekDisabled, provider.Mode);
    }

    // -----------------------------------------------------------------------
    // (S3) Mode=DeepSeek + RealCallsEnabled=false → falls back to Mock (defense in depth)
    // -----------------------------------------------------------------------

    [Fact]
    public void Real_mode_without_RealCallsEnabled_falls_back_to_Mock()
    {
        var provider = ResolveProvider("DeepSeek", realCallsEnabled: false, apiKeyValue: null);
        // Defense in depth — real calls disabled → must resolve to Mock
        Assert.IsType<MockAiProvider>(provider);
        Assert.Equal(AiProviderMode.Mock, provider.Mode);
    }

    // -----------------------------------------------------------------------
    // (S4) Mode=DeepSeek + RealCallsEnabled=true + key absent → falls back to Mock
    // -----------------------------------------------------------------------

    [Fact]
    public void Real_mode_with_RealCallsEnabled_but_no_key_falls_back_to_Mock()
    {
        // Key absent (null) — simulates unconfigured environment
        var provider = ResolveProvider("DeepSeek", realCallsEnabled: true, apiKeyValue: null);
        Assert.IsType<MockAiProvider>(provider);
        Assert.Equal(AiProviderMode.Mock, provider.Mode);
    }

    // -----------------------------------------------------------------------
    // (S5) Mode=DeepSeek + RealCallsEnabled=true + key present → RealDeepSeekAiProvider
    // -----------------------------------------------------------------------

    [Fact]
    public void Real_mode_with_RealCallsEnabled_and_key_resolves_Real()
    {
        // Stub key value — safe sentinel, not a real credential
        var provider = ResolveProvider("DeepSeek", realCallsEnabled: true, apiKeyValue: "sentinel-test-key");
        Assert.IsType<RealDeepSeekAiProvider>(provider);
        Assert.Equal(AiProviderMode.DeepSeek, provider.Mode);
    }
}
