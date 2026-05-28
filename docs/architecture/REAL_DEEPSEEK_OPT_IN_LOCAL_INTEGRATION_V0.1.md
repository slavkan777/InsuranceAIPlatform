# REAL_DEEPSEEK_OPT_IN_LOCAL_INTEGRATION_V0.1

**Gate:** REAL_DEEPSEEK_OPT_IN_LOCAL_INTEGRATION_V0.1  
**Status:** IMPLEMENTED  
**Branch:** dev  
**Base commit:** 918e8a3aec82462bde9239aeec91d3a46b1082b4

## Purpose

Implements the real DeepSeek AI provider as an opt-in path in the InsuranceAIPlatform. The Mock provider remains the default. Real calls require explicit configuration AND a valid API key — the system degrades gracefully to Mock if any condition is unmet.

## Architecture

### Provider selection (Program.cs, startup)

Priority order evaluated at startup:

1. `Mode in {"DeepSeek","DeepSeekReal"}` AND `RealCallsEnabled=true` AND `DEEPSEEK_API_KEY` non-empty → `RealDeepSeekAiProvider`
2. `Mode="DeepSeekDisabled"` → `DisabledDeepSeekAiProvider` (throws, no HTTP)
3. All other cases → `MockAiProvider` (default, deterministic, no external calls)

### RealDeepSeekAiProvider

- Implements `IAiProvider` with `Mode => AiProviderMode.DeepSeek`
- Uses `IHttpClientFactory` (named `"deepseek"`) — no SDK packages
- API key read ONLY via `IConfiguration["DEEPSEEK_API_KEY"]` at request time
- Authorization header set per-request, never on named client default headers
- Request: OpenAI-compatible chat-completions with `response_format=json_object`
- System prompt enforces advisory-only guardrails at model level
- User message: hardcoded synthetic CLM-1006 context (no real PII)
- On non-2xx: throws `HttpRequestException("DeepSeek call failed: HTTP {N}")` — body not leaked
- On malformed JSON: throws `InvalidOperationException("could not be parsed...")` — raw content not leaked
- On missing key: throws `InvalidOperationException("DeepSeek API key not configured...")` — name only

### API key handling

- Key value **NEVER** stored in: fields, properties, logs, exceptions, tests, config files
- Whitespace/newlines stripped before use (env vars may contain embedded line breaks)
- Boolean presence only logged at startup: `key configured: true/false`
- `appsettings*.json` contain zero key references

### New enum value

`AiProviderMode.DeepSeek` added after `Disabled`. Existing values (`Mock`, `DeepSeekDisabled`, `Disabled`) preserved.

## Configuration (appsettings.Development.json)

```json
{
  "AiProvider": {
    "Mode": "Mock",
    "RealCallsEnabled": false,
    "DeepSeek": {
      "Model": "deepseek-chat",
      "Endpoint": "https://api.deepseek.com/v1/chat/completions",
      "TimeoutSeconds": 30
    }
  }
}
```

To opt in (command line / env override only — never in committed config):
```
--AiProvider:Mode=DeepSeek --AiProvider:RealCallsEnabled=true
```

## New files

| File | Purpose |
|------|---------|
| `Services.AiAnalysis/Providers/RealDeepSeekAiProvider.cs` | Real DeepSeek HTTP adapter |
| `Services.AiAnalysis/Providers/DeepSeekChatModels.cs` | Internal DTOs for chat-completions API |
| `Tests/RealDeepSeekProviderTests.cs` | 5 unit tests with fake HttpMessageHandler |
| `Tests/AiProviderSelectionTests.cs` | 5 selection logic tests |
| `docs/architecture/REAL_DEEPSEEK_OPT_IN_LOCAL_INTEGRATION_V0.1.md` | This document |
| `docs/reports/real-deepseek-opt-in-local-integration-v0.1/report.md` | Implementation report |

## Modified files

| File | Change |
|------|--------|
| `Services.AiAnalysis/AiProviderMode.cs` | Added `DeepSeek` enum value |
| `Services.AiAnalysis/Configuration/AiProviderOptions.cs` | Added `Endpoint`, `TimeoutSeconds` to `DeepSeekOptions`; model default updated to `deepseek-chat` |
| `Services.AiAnalysis/InsuranceAIPlatform.Services.AiAnalysis.csproj` | Added `Microsoft.Extensions.Http`, `Options`, `Configuration.Abstractions`, `Logging.Abstractions` |
| `Services.AiAnalysis/Orchestration/PersistenceAiAnalysisOrchestrator.cs` | Fixed hardcoded `ProviderMode: "Mock"` — now uses `_provider.Mode.ToString()` |
| `Api/Program.cs` | Replaced defensive-block with tri-way provider selection logic |
| `Api/appsettings.Development.json` | Updated model to `deepseek-chat`, added `Endpoint`, `TimeoutSeconds` |

## Guardrails

All 7 advisory-only guardrail flags remain enforced regardless of provider:
- `advisoryOnly=true`, `requiresHumanReview=true`
- `canApprovePayout=false`, `canRejectClaim=false`, `canAccuseFraudFinal=false`
- `canSendCustomerMessage=false`, `canChangeClaimStatus=false`
