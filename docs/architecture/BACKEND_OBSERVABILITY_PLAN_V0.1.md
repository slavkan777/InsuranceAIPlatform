---
title: Backend Observability Plan V0.1
type: knowledge
status: active
created: 2026-05-27
tags: [observability, logging, tracing, health, audit, telemetry]
---

# Backend Observability Plan V0.1

## 1. Philosophy

V0.1 observability is deliberately minimal. The goal is:
1. Enough signal to debug integration issues between the React frontend and the .NET backend during local development
2. Audit trail completeness for AI/risk/approval events (non-negotiable from day one)
3. A clean extension path to OpenTelemetry when the project matures

No external telemetry services (Prometheus, Grafana, Application Insights, Datadog, Jaeger) are configured at V0.1. All signals are local — console/file logs, in-memory audit store, and a `/health` endpoint.

---

## 2. Observability Signals Table

| Signal | Implementation | Where It Appears | V0.1 Status | Future Path |
|---|---|---|---|---|
| **Structured application logs** | `Microsoft.Extensions.Logging` (built-in `ILogger<T>`) | Console (dev) + optional file sink | Active from day 1 | Add Serilog rolling file + JSON sink when log volume warrants |
| **Request correlation ID** | Custom `CorrelationIdMiddleware` — reads `X-Correlation-ID` header or generates a new `Guid`; writes to response header and `ILogger` scope | Every request log line; response header `X-Correlation-ID` | Active | Feeds into OpenTelemetry trace context later |
| **ApiErrorDto.traceId** | Error responses include `{ "type": "...", "title": "...", "status": 500, "traceId": "<correlationId>" }` | JSON error responses | Active | traceId = correlationId from middleware |
| **Health endpoint** | `app.MapHealthChecks("/health")` returning `{ "status": "Healthy" }` | `GET http://localhost:5174/health` | Active | Add liveness/readiness split + DB health check when SQL Server connected |
| **Audit events** | `IAuditTrailService` append-only in-memory store; each event: `{ eventId, claimId, eventType, actor, timestamp, payload, runId?, traceId? }` | `GET /api/claims/{id}/audit-trace` response; console log on append | Active (in-memory) | Persist to `dbo.AuditEvents` in SQL Server phase |
| **Synthetic AI cost telemetry** | `ICostTelemetryService` records tokens/cost/latency per `runId`; returned in `AuditTraceDto.distribution` | `GET /api/claims/{id}/audit-trace` | Active (synthetic) | Replace with real LLM response metadata in AI integration phase |
| **Request timing** | `ILogger` + Stopwatch in controller or middleware; log `DurationMs` per request | Console logs | Active (manual) | Replace with OpenTelemetry `Activity` spans |
| **Swagger / OpenAPI** | Swashbuckle `AddSwaggerGen` + `UseSwagger` + `UseSwaggerUI` | `GET http://localhost:5174/swagger` | Active | Add XML doc comments for richer schema descriptions |
| **Demo status endpoint** | `GET /api/system/demo-status` returns `{ demoReady, seedClaimId, demoMode }` | Frontend demo screen + integration smoke tests | Active | No change needed; extend payload if demo scenarios multiply |
| **OpenTelemetry traces** | Not configured at V0.1 | N/A | Deferred | Add `OpenTelemetry.Extensions.Hosting` + OTLP exporter when moving beyond local dev |
| **OpenTelemetry metrics** | Not configured at V0.1 | N/A | Deferred | Add `System.Diagnostics.Metrics` + Prometheus exporter at P2 phase |
| **Distributed tracing (Jaeger/Zipkin)** | Not configured at V0.1 | N/A | Deferred | Backend architecture is pre-wired via `ILogger` scope; adding OTel is additive |

---

## 3. Logging Recommendation

**Use built-in `Microsoft.Extensions.Logging` at V0.1.** Do not add Serilog yet.

Rationale:
- `ILogger<T>` is already present in every ASP.NET Core project
- Structured logging via `LoggerMessage.Define` or `BeginScope` is sufficient for local development
- Serilog adds value when: log sinks beyond console are needed (file, Seq, Elasticsearch), or JSON structured output is required for log aggregation. At V0.1, neither applies.
- The migration path from `ILogger` to Serilog is trivial (swap `builder.Host.UseSerilog(...)` without changing `ILogger<T>` injection anywhere)

When to add Serilog: when a log file sink is needed for demo sessions, or when JSON-format logs are required for a CI pipeline.

---

## 4. Correlation ID Middleware Specification

The `CorrelationIdMiddleware` must:

1. Read `X-Correlation-ID` from the incoming request headers
2. If absent, generate `Guid.NewGuid().ToString("N")`
3. Add the ID to the current `ILogger` scope: `using (_logger.BeginScope(new { CorrelationId = id })) { await _next(context); }`
4. Write the ID back to the response header: `context.Response.Headers["X-Correlation-ID"] = id`
5. Store it in `HttpContext.Items["CorrelationId"]` for controller access

Controllers use `HttpContext.Items["CorrelationId"]` to populate `ApiErrorDto.traceId` and AI run `traceId` fields.

---

## 5. Health Endpoint Specification

`GET /health` response (V0.1):

```json
{
  "status": "Healthy",
  "demoMode": true,
  "version": "0.1.0",
  "seedClaimId": "CLM-1006"
}
```

Future extensions (when SQL Server is connected):
- Add `HealthCheck` for DB connectivity: `services.AddHealthChecks().AddSqlServer(...)`
- Split into `/health/live` (process alive) and `/health/ready` (DB reachable)
- Return `Degraded` if DB is unreachable but demo mode is active (service can still serve mock data)

---

## 6. Audit Event Schema

Every audit event appended via `IAuditTrailService`:

```json
{
  "eventId": "evt_<guid>",
  "claimId": "CLM-1006",
  "eventType": "AiAnalysisRun | RiskScored | PolicyChecked | ApprovalDraftSaved | ApprovalSubmitted | GovernanceBlock | DemoStep",
  "actor": "system | adjuster:<id> | demo",
  "timestamp": "2026-05-18T14:23:11Z",
  "runId": "run_8f3d2a7e",
  "traceId": "trc_8f3d2a7e",
  "payload": { }
}
```

The `GovernanceBlock` event type is mandatory when the system detects auto-approval conditions and blocks them. CLM-1006 pre-seed must include this event with description "Авто-погодження заблоковано".

---

## 7. OpenTelemetry Extension Path (Deferred)

When the project reaches P1/P2 maturity, the following additions are backward-compatible with the V0.1 logging setup:

1. Add `OpenTelemetry.Extensions.Hosting` NuGet package
2. Register `AddOpenTelemetry().WithTracing(b => b.AddAspNetCoreInstrumentation().AddOtlpExporter())`
3. Replace manual `Stopwatch` + `ILogger` duration logging with `Activity` spans
4. The existing `CorrelationId` becomes the OTel `TraceId`
5. No changes to controllers or services needed — instrumentation is middleware-level

This path is documented here so that V0.1 implementation choices (ILogger scopes, CorrelationId middleware, structured log fields) are made with OTel compatibility in mind.
