using System.Diagnostics;
using System.Text.RegularExpressions;

namespace InsuranceAIPlatform.Api.Middleware;

/// <summary>
/// BFF / API Gateway correlation middleware (Stage-1 skeleton).
/// - Reads an incoming X-Correlation-Id header if present and well-formed.
/// - If absent or invalid, generates a new Guid-based correlation id.
/// - Sets Activity.Current's TraceId so the value propagates through structured logging.
/// - Writes X-Correlation-Id and X-Trace-Id back on every response.
/// - Stamps X-Bff: api-gateway on every response to identify this BFF layer.
/// No secrets are read, logged, or forwarded.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    // Accepts typical short tokens: alphanumeric, hyphens, underscores. 8–64 chars.
    private static readonly Regex SafeToken = new(@"^[A-Za-z0-9\-_]{8,64}$", RegexOptions.Compiled);

    internal const string CorrelationIdKey = "CorrelationId";
    internal const string CorrelationIdHeader = "X-Correlation-Id";
    internal const string TraceIdHeader = "X-Trace-Id";
    internal const string BffIdentityHeader = "X-Bff";
    internal const string BffIdentityValue = "api-gateway";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.Items[CorrelationIdKey] = correlationId;

        // Propagate into the current Activity so structured logging picks it up.
        var activity = Activity.Current;
        if (activity is not null)
        {
            activity.AddTag("correlation.id", correlationId);
        }

        // Write response headers before the downstream pipeline flushes.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            context.Response.Headers[TraceIdHeader] = activity?.TraceId.ToString() ?? correlationId;
            context.Response.Headers[BffIdentityHeader] = BffIdentityValue;
            return Task.CompletedTask;
        });

        await next(context);
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var incoming))
        {
            var raw = incoming.ToString();
            if (SafeToken.IsMatch(raw))
                return raw;
        }
        return Guid.NewGuid().ToString("D");
    }
}
