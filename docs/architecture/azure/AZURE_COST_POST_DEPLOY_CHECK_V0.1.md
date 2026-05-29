# Azure Cost — Post-Deploy Check V0.1

Verified immediately after the first minimal deploy (2026-05-29).

## Guardrails (all PASS)

| Check | Result | Evidence |
|---|---|---|
| Compute scale-to-zero | ✅ | Container App `minReplicas=0` |
| No always-on App Service | ✅ | none deployed |
| No AKS | ✅ | `az resource list` → no `ManagedClusters` in subscription |
| No ACR | ✅ | `az resource list` → no `ContainerRegistry/registries` (image via GHCR free) |
| Static hosting cost | ✅ | SWA `sku.name = Free` |
| Storage | ✅ | `allowSharedKeyAccess=false`; lifecycle TTL on demo containers |
| Log Analytics cap | ✅ | retention 30d + `dailyQuotaGb=1` |
| App Insights | ✅ | sampling 50% |
| SQL | ✅ not deployed | `enableSql=false`; deployment output `sqlServerFqdn=""` |
| AI paid services | ✅ not deployed | `enableAi=false` |
| Resource count in RG | ✅ 8 | exactly the planned minimal set |

## Expected spend

- **Idle:** ~$0 — Container Apps scale-to-zero (no replica when idle), SWA Free, App Insights sampled, Log Analytics capped. Residual = Log Analytics ingestion + Storage/KV transactions ≈ well under $1–2/mo.
- **Active demo:** brief Container Apps consumption per request (cold start then scale back to 0); negligible.
- **Budget:** $30/mo with alerts $5/$10/$20/$30/$50 (set by operator on this subscription). Real target $5–10/mo.

## Cleanup

```bash
az group delete -n rg-iap-demo --yes --no-wait
```
Single command removes all 8 resources. GHCR image + budget are outside the RG (unaffected).
