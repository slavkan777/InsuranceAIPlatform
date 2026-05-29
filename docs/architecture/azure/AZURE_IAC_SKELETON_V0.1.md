# Azure IaC Skeleton — InsuranceAIPlatform (v0.1)

**Status: SKELETON, NOT DEPLOYED.** The Bicep under `infra/` compiles (`az bicep build` → 0 errors / 0 warnings) but has **not** been applied. No Azure resources exist; no `az login`/`az deployment` was run in the authoring gate.

## Approach: Bicep-first
- **Selected:** Bicep (subscription-scope `main.bicep` creates the RG and wires RG-scoped modules).
- **Why:** Azure-native, no remote state backend to manage, first-class type checking, azd-compatible, strongest Azure interview story.
- **Why not Terraform:** would add a state backend + provider auth for a single-cloud demo with no multi-cloud requirement.
- **Why not azd-only:** azd uses Bicep underneath; a clean modular Bicep tree can be wrapped by azd later without rework.

## Module map → architecture
| Module | Resource(s) | Cost-critical setting |
|---|---|---|
| `monitoring.bicep` | Log Analytics + App Insights | `retentionInDays:30`, `dailyQuotaGb:1`, `SamplingPercentage:50` |
| `identity.bicep` | user-assigned MI | passwordless auth for app |
| `key-vault.bicep` | Key Vault (RBAC) + role | `enableRbacAuthorization:true`; Secrets User to app MI; **no secret values** |
| `storage.bicep` | Storage + 4 containers + lifecycle + role | `allowSharedKeyAccess:false`; TTL delete after `blobTtlDays` |
| `container-apps.bicep` | ACA env + `insurance-api` | **`minReplicas:0`** (scale-to-zero); HTTP scale rule; UAMI |
| `sql-serverless.bicep` | SQL server + DB + fw | `GP_S_Gen5_1`, **`autoPauseDelay:60`**, `azureADOnlyAuthentication:true` |
| `static-web-app.bicep` | Static Web Apps | `sku:Free` |
| `ai-optional.bicep` | OpenAI + Doc Intelligence + Search | only if `enableAi=true`; **F0 / Free** tiers; `disableLocalAuth:true` |
| `budgets-notes.md` | (notes) | budget created manually; alerts notify-only |

## Boundaries encoded in the templates
- `enableAi=false` by default → **no AI resources, no AI cost** until the AI gate.
- `insuranceApiImage` defaults to a public placeholder image → the template is deployable-shaped before our image exists; the real `ghcr.io/...` image is supplied at the deploy gate.
- `sqlAdminObjectId` is an all-zeros **placeholder** → must be supplied at deploy (never committed). Entra-only auth means **no SQL password** anywhere.
- AKS intentionally not modelled (deferred).

## Verification (this gate)
```
az bicep build --file infra/main.bicep   → EXIT 0, 0 errors, 0 warnings, ARM template emitted (1291 lines)
```
Offline only — no `az login`, no `az deployment`, no `what-if` (would need login).

## Truth boundary
**Deployed today:** nothing in Azure. **Architecture-ready:** the full topology above, compiled and parameterised. The gap between "compiles" and "deployed" closes in `AZURE_MINIMAL_DEPLOY_V0.1` (and stateful/AI gates after).

## Files (all uncommitted in this gate — committed in `AZURE_IAC_SKELETON_COMMIT_V0.1`)
`infra/main.bicep`, `infra/parameters/demo.bicepparam`, `infra/modules/*.bicep` (8) + `budgets-notes.md`, `infra/README.md`, `docs/architecture/azure/AZURE_IAC_SKELETON_V0.1.md`, `AZURE_DEPLOYMENT_RUNBOOK_V0.1.md`, `AZURE_RESOURCE_NAMING_V0.1.md`, `AZURE_INTERVIEW_STORY_V0.1.md`, `.github/workflows/azure-deploy-demo.yml` (disabled / workflow_dispatch only) — plus the 4 pre-flight docs from the prior gate.
