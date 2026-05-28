using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using InsuranceAIPlatform.BuildingBlocks;
using InsuranceAIPlatform.Services.AiAnalysis;
using InsuranceAIPlatform.Services.AiAnalysis.Configuration;
using InsuranceAIPlatform.Services.AiAnalysis.Contracts;
using InsuranceAIPlatform.Services.AiAnalysis.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace InsuranceAIPlatform.Tests;

/// <summary>
/// Unit tests for RealDeepSeekAiProvider using a fake HttpMessageHandler.
/// NO real HTTP calls are made. DEEPSEEK_API_KEY value is never printed.
/// </summary>
public class RealDeepSeekProviderTests
{
    // Safe sentinel key for tests — clearly not a real key, not a prefix/suffix of any real value.
    private const string TestKeySentinel = "test-key-sentinel-not-real";

    private static RealDeepSeekAiProvider BuildProvider(
        string? apiKey,
        HttpMessageHandler? handler = null,
        string endpoint = "https://api.deepseek.com/v1/chat/completions",
        string model = "deepseek-chat")
    {
        var configValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (apiKey is not null)
            configValues["DEEPSEEK_API_KEY"] = apiKey;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var options = Options.Create(new AiProviderOptions
        {
            Mode = "DeepSeek",
            RealCallsEnabled = true,
            DeepSeek = new DeepSeekOptions
            {
                Model = model,
                Endpoint = endpoint,
                TimeoutSeconds = 30,
            },
        });

        // Build an IHttpClientFactory using a real ServiceCollection with the fake handler.
        var services = new ServiceCollection();
        if (handler is not null)
        {
            services.AddHttpClient("deepseek")
                .ConfigurePrimaryHttpMessageHandler(() => handler);
        }
        else
        {
            services.AddHttpClient("deepseek");
        }
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IHttpClientFactory>();

        return new RealDeepSeekAiProvider(
            factory,
            config,
            options,
            new SystemClock(),
            NullLogger<RealDeepSeekAiProvider>.Instance);
    }

    private static string BuildHappyResponseJson(
        string model = "deepseek-chat",
        string responseId = "resp-123",
        int totalTokens = 850,
        string assistantContent = "")
    {
        if (string.IsNullOrEmpty(assistantContent))
        {
            assistantContent = JsonSerializer.Serialize(new
            {
                summaryText = "Advisory analysis complete. Human review required.",
                recommendedActionText = "Request missing rear bumper photo. Human adjuster must decide.",
                policyExplanationText = "Policy covers road accidents after deductible of $500.",
                riskLevel = "moderate",
                confidenceScore = 82,
                findings = new[]
                {
                    new { category = "Documents", text = "Missing rear bumper photo.", severity = "warn" },
                    new { category = "Damage estimate", text = "Estimate exceeds benchmark by 38%.", severity = "warn" },
                },
                evidence = new[]
                {
                    new { source = "Police report", note = "Accident confirmed 18.05.2026.", confidence = 95 },
                },
                risks = new[]
                {
                    new { label = "Repair sum above expected range", weight = 25 },
                },
            });
        }

        return JsonSerializer.Serialize(new
        {
            id = responseId,
            model = model,
            choices = new[]
            {
                new
                {
                    message = new { role = "assistant", content = assistantContent },
                    finish_reason = "stop",
                },
            },
            usage = new
            {
                prompt_tokens = 300,
                completion_tokens = 550,
                total_tokens = totalTokens,
            },
        });
    }

    // -----------------------------------------------------------------------
    // (R1) Provider calls configured endpoint with Bearer header
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_calls_configured_endpoint_with_bearer_header()
    {
        Uri? capturedUri = null;
        AuthenticationHeaderValue? capturedAuth = null;
        string? capturedBody = null;

        var fakeHandler = new FakeHttpHandler(req =>
        {
            capturedUri = req.RequestUri;
            capturedAuth = req.Headers.Authorization;
            capturedBody = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BuildHappyResponseJson(), Encoding.UTF8, "application/json"),
            };
        });

        const string endpoint = "https://api.deepseek.com/v1/chat/completions";
        var provider = BuildProvider(TestKeySentinel, fakeHandler, endpoint);
        var request = new AiAnalysisRequest("CLM-1006", "corr-r1", "test-actor");

        await provider.AnalyzeAsync(request);

        // Assert URI matches configured endpoint
        Assert.NotNull(capturedUri);
        Assert.Equal(endpoint, capturedUri!.ToString());

        // Assert Authorization header is present and matches the test sentinel
        Assert.NotNull(capturedAuth);
        Assert.Equal("Bearer", capturedAuth!.Scheme);
        Assert.Equal(TestKeySentinel, capturedAuth.Parameter);

        // Assert request body contains the configured model
        Assert.NotNull(capturedBody);
        Assert.Contains("deepseek-chat", capturedBody);

        // Assert body contains a system message with the guardrail phrase
        Assert.Contains("advisory only", capturedBody, StringComparison.OrdinalIgnoreCase);

        // Assert body contains a user message with synthetic CLM-1006 context marker
        Assert.Contains("CLM-1006", capturedBody, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------------------------------------------------
    // (R2) Provider maps response to AiProviderRawOutput correctly
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_maps_response_to_raw_output()
    {
        const int expectedTokens = 850;
        const string expectedModel = "deepseek-chat";

        var fakeHandler = new FakeHttpHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    BuildHappyResponseJson(model: expectedModel, totalTokens: expectedTokens),
                    Encoding.UTF8, "application/json"),
            });

        var provider = BuildProvider(TestKeySentinel, fakeHandler);
        var request = new AiAnalysisRequest("CLM-1006", "corr-r2", "test-actor");

        var output = await provider.AnalyzeAsync(request);

        // ModelName must be the REAL DeepSeek model id — NOT the Mock fingerprint
        Assert.Equal(expectedModel, output.ModelName);
        Assert.NotEqual("local-mock-v0.1", output.ModelName);

        Assert.Equal(2, output.Findings.Count);
        Assert.Single(output.Evidence);
        Assert.Single(output.Risks);

        Assert.Equal(expectedTokens, output.Tokens);

        // Cost = tokens * 0.00000027
        Assert.Equal(expectedTokens * 0.00000027m, output.Cost);

        // Confidence score should be non-zero
        Assert.True(output.ConfidenceScore > 0);
    }

    // -----------------------------------------------------------------------
    // (R3) Provider throws safely when key is absent or empty
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_throws_safely_without_key()
    {
        // Empty key — simulate unconfigured environment
        var provider = BuildProvider(apiKey: "");
        var request = new AiAnalysisRequest("CLM-1006", "corr-r3", "test-actor");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.AnalyzeAsync(request));

        // Message must contain the name of the key, not any key value
        Assert.Contains("DeepSeek API key not configured", ex.Message);

        // Message must NOT contain the test sentinel or any key-like value
        Assert.DoesNotContain(TestKeySentinel, ex.Message);
        Assert.DoesNotContain("sk-", ex.Message);
    }

    // -----------------------------------------------------------------------
    // (R4) Provider throws safely on non-2xx response without leaking body
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_throws_safely_on_non_2xx()
    {
        var fakeHandler = new FakeHttpHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("unauthorized", Encoding.UTF8, "application/json"),
            });

        var provider = BuildProvider(TestKeySentinel, fakeHandler);
        var request = new AiAnalysisRequest("CLM-1006", "corr-r4", "test-actor");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => provider.AnalyzeAsync(request));

        // Message must include the status code
        Assert.Contains("HTTP 401", ex.Message);

        // Response body must NOT be leaked into the exception message
        Assert.DoesNotContain("unauthorized", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------------------------------------------------
    // (R5) Provider throws safely when assistant returns malformed JSON
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_throws_safely_on_malformed_json()
    {
        const string malformedContent = "not json at all {{{{";

        var responseJson = JsonSerializer.Serialize(new
        {
            id = "resp-bad",
            model = "deepseek-chat",
            choices = new[]
            {
                new
                {
                    message = new { role = "assistant", content = malformedContent },
                    finish_reason = "stop",
                },
            },
            usage = new { prompt_tokens = 10, completion_tokens = 5, total_tokens = 15 },
        });

        var fakeHandler = new FakeHttpHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
            });

        var provider = BuildProvider(TestKeySentinel, fakeHandler);
        var request = new AiAnalysisRequest("CLM-1006", "corr-r5", "test-actor");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.AnalyzeAsync(request));

        // Message must indicate parsing failure
        Assert.Contains("could not be parsed", ex.Message);

        // Raw assistant content must NOT be leaked into the exception message
        Assert.DoesNotContain(malformedContent, ex.Message);
    }

    // -----------------------------------------------------------------------
    // (R6) Retry happens on 503 — second attempt succeeds
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_retries_on_503_and_succeeds_on_second_attempt()
    {
        var callCount = 0;
        var fakeHandler = new FakeHttpHandler(_ =>
        {
            callCount++;
            if (callCount == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("upstream unavailable", Encoding.UTF8, "text/plain"),
                };
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BuildHappyResponseJson(), Encoding.UTF8, "application/json"),
            };
        });

        var provider = BuildProvider(TestKeySentinel, fakeHandler);
        var request = new AiAnalysisRequest("CLM-1006", "corr-r6", "test-actor");

        var output = await provider.AnalyzeAsync(request);

        Assert.Equal(2, callCount); // exactly one retry
        Assert.Equal("deepseek-chat", output.ModelName);
    }

    // -----------------------------------------------------------------------
    // (R7) Retry happens on 429 — second attempt succeeds
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_retries_on_429_and_succeeds_on_second_attempt()
    {
        var callCount = 0;
        var fakeHandler = new FakeHttpHandler(_ =>
        {
            callCount++;
            if (callCount == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BuildHappyResponseJson(), Encoding.UTF8, "application/json"),
            };
        });

        var provider = BuildProvider(TestKeySentinel, fakeHandler);
        var request = new AiAnalysisRequest("CLM-1006", "corr-r7", "test-actor");

        var output = await provider.AnalyzeAsync(request);

        Assert.Equal(2, callCount);
        Assert.True(output.Tokens > 0);
    }

    // -----------------------------------------------------------------------
    // (R8) Retry happens on timeout (TaskCanceledException) — second attempt succeeds
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_retries_on_timeout_and_succeeds_on_second_attempt()
    {
        var callCount = 0;
        var fakeHandler = new FakeHttpHandler(_ =>
        {
            callCount++;
            if (callCount == 1)
            {
                // Simulate timeout — HttpClient maps TaskCanceledException to its timeout pathway.
                throw new TaskCanceledException("simulated handler timeout");
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(BuildHappyResponseJson(), Encoding.UTF8, "application/json"),
            };
        });

        var provider = BuildProvider(TestKeySentinel, fakeHandler);
        var request = new AiAnalysisRequest("CLM-1006", "corr-r8", "test-actor");

        var output = await provider.AnalyzeAsync(request);

        Assert.Equal(2, callCount);
        Assert.NotNull(output);
    }

    // -----------------------------------------------------------------------
    // (R9) NO retry on 401 — auth failures are not transient
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_does_not_retry_on_401()
    {
        var callCount = 0;
        var fakeHandler = new FakeHttpHandler(_ =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("unauthorized", Encoding.UTF8, "application/json"),
            };
        });

        var provider = BuildProvider(TestKeySentinel, fakeHandler);
        var request = new AiAnalysisRequest("CLM-1006", "corr-r9", "test-actor");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => provider.AnalyzeAsync(request));

        Assert.Equal(1, callCount); // exactly one call, NO retry
        Assert.Contains("HTTP 401", ex.Message);
        // Response body must NOT leak
        Assert.DoesNotContain("unauthorized", ex.Message, StringComparison.OrdinalIgnoreCase);
        // Key must NOT leak
        Assert.DoesNotContain(TestKeySentinel, ex.Message);
        Assert.DoesNotContain("sk-", ex.Message);
    }

    // -----------------------------------------------------------------------
    // (R10) NO retry on 403 — forbidden is not transient
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_does_not_retry_on_403()
    {
        var callCount = 0;
        var fakeHandler = new FakeHttpHandler(_ =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        });

        var provider = BuildProvider(TestKeySentinel, fakeHandler);
        var request = new AiAnalysisRequest("CLM-1006", "corr-r10", "test-actor");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => provider.AnalyzeAsync(request));

        Assert.Equal(1, callCount);
        Assert.Contains("HTTP 403", ex.Message);
        Assert.DoesNotContain(TestKeySentinel, ex.Message);
    }

    // -----------------------------------------------------------------------
    // (R11) NO retry on 400 — bad request is not transient
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Real_provider_does_not_retry_on_400()
    {
        var callCount = 0;
        var fakeHandler = new FakeHttpHandler(_ =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var provider = BuildProvider(TestKeySentinel, fakeHandler);
        var request = new AiAnalysisRequest("CLM-1006", "corr-r11", "test-actor");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => provider.AnalyzeAsync(request));

        Assert.Equal(1, callCount);
        Assert.Contains("HTTP 400", ex.Message);
    }

    // -----------------------------------------------------------------------
    // Test double: FakeHttpHandler
    // -----------------------------------------------------------------------

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_handler(request));
    }
}
