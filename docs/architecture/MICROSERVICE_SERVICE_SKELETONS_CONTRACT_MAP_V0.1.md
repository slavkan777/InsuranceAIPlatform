# Microservice Service Skeletons — Contract & Dependency Map V0.1

**Gate:** `MICROSERVICE_SERVICE_SKELETONS_PLANNING_V0.1` · companion to `MICROSERVICE_SERVICE_SKELETONS_PLANNING_V0.1.md` · **Branch:** `dev` @ `9f494a1`
Planning-only. Defines, per service, the skeleton artifacts + contracts the next implementation gate will create behind the BFF, plus future ownership and Azure mapping. Skeleton phase = class libraries, in-process behind the BFF, **no DB, no web hosts, no writes, no AI calls**.

## Per-service contract map

| Service | Purpose | Owns | Does not own | Initial skeleton artifacts | Initial contracts / interfaces | BFF dependency | Future read ownership | Future command ownership | Future events | Future data boundary | Tests required | Azure mapping later |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Claims** | Claim queue, detail, lifecycle, deterministic status rules | Claim cases, claim status | Customers/policies, documents, AI runs, approvals, audit store | `Services.Claims` class lib; `AddClaimsService()` DI ext; health contributor | `IClaimsService`; `ClaimListItem`, `ClaimDetails`, `ClaimSummary` contracts (mirror current read DTOs) | BFF resolves `IClaimsService` for `/api/claims`, `/api/claims/{id}`, `/api/claims/summary` | claim list / detail / summary | create case, status transition | `ClaimCreated`, `ClaimStatusChanged` | `claims` schema, own DbContext | DI resolves `IClaimsService`; preserved `/api/claims*` routes 200 | Container App + Azure SQL (claims) |
| **Customers & Policies** | Customers, vehicles, policies, coverage validation; 200 synthetic users | Customers, vehicles, policies, coverage, test users | Claims, documents, AI, approval, audit | `Services.CustomersPolicies` lib; DI ext; health | `ICustomersPoliciesService`; `PolicyCoverage`, `CustomerVehicleContext` contracts | `/api/claims/{id}/policy`, `/api/claims/{id}/customer-vehicle` | policy + customer/vehicle reads | seed users; update customer contact (later) | `PolicyValidated` | `customers` schema + 200 synthetic users seed | DI resolves; policy + customer-vehicle routes 200 | Container App + Azure SQL (customers) |
| **Documents** | Document/photo metadata, missing-evidence detection | Document/photo metadata, checklist | Claim decisions, blob bytes (later), AI extraction logic | `Services.Documents` lib; DI ext; health | `IDocumentsService`; `DocumentChecklistItem`, `DamagePhoto` contracts | `/api/claims/{id}/documents` | documents / photos reads | request missing doc, confirm doc, add metadata | `DocumentRequested/Confirmed/Added` | `documents` schema (metadata); Blob later | DI resolves; documents route 200 | Container App + Azure SQL + Blob |
| **AI Analysis** | Advisory AI analysis (DeepSeek later, mock default) | AI run contracts, findings/evidence/confidence/cost metadata (later) | Claim decisions, approval, payout; never final authority | `Services.AiAnalysis` lib; DI ext; health; `IAiProvider` placeholder (no impl, **no call**) | `IAiAnalysisService`; `AiEvidence`, `AiRun` contracts; `IAiProvider` { Mock default, DeepSeek (opt-in/disabled), Disabled } | `/api/claims/{id}/ai-evidence` (read) | ai-evidence reads | run analysis, customer draft (advisory) | `AiRunStarted/Completed` | `ai` schema (runs/cost); **DeepSeek isolated here only**; `DEEPSEEK_API_KEY` env/Key Vault | DI resolves; ai-evidence route 200; **assert no provider call** | Container App + Azure SQL (ai) + Key Vault |
| **Approval** | Approval drafts + human decision workflow | Approval drafts, decision state, transition requests | AI authority, payout execution, audit store | `Services.Approval` lib; DI ext; health | `IApprovalService`; `ApprovalDraft`, `HumanDecisionOption` contracts | `/api/claims/{id}/approval` (read) | approval draft reads | save draft, **submit (human-only)** | `ApprovalDraftSaved/Submitted` | `approval` schema | DI resolves; approval route 200 | Container App + Azure SQL (approval) |
| **Audit & Cost** | Append-only audit + token/cost traces + governance trace | Audit events, cost/token traces, correlation governance trace | Business decisions, claim/approval logic | `Services.AuditCost` lib; DI ext; health | `IAuditCostService`; `AuditTrace`, `CostLine` contracts | `/api/claims/{id}/audit` (read); receives audit from all commands later | audit trace reads | append audit (internal, from other services) | consumes all `*Event`; `AuditAppended` | `audit` schema (append-only) | DI resolves; audit route 200 | Container App + Azure SQL (audit) + App Insights |

## Shared kernel (not a service)
`InsuranceAIPlatform.BuildingBlocks` (class lib): `ServiceNames` constants, correlation-id accessor, error envelope `{ code, message, traceId }`, health-contributor abstraction, `Result`/error primitive. **No domain, no DTOs, no DbContext.** Referenced by all services + BFF; references nothing internal.

## Dependency direction
`InsuranceAIPlatform.Api` (BFF) → `Services.*` → `BuildingBlocks`. Services.* do **not** reference one another; cross-service data is id-only via contracts when introduced. BFF maps service contracts → BFF public DTOs (the existing frontend shapes); the frontend never depends on internal service contracts and never calls a service directly.

## BFF endpoint → service (skeleton delegation)
| BFF route (preserved) | Service interface | Skeleton delegation (Stage 2) |
|---|---|---|
| `/api/claims`, `/api/claims/{id}`, `/api/claims/summary` | `IClaimsService` | → current `InMemoryClaimReadService` (response-identical) |
| `/api/claims/{id}/policy`, `/api/claims/{id}/customer-vehicle` | `ICustomersPoliciesService` | → current read service |
| `/api/claims/{id}/documents` | `IDocumentsService` | → current read service |
| `/api/claims/{id}/ai-evidence` | `IAiAnalysisService` | → current read service (no provider call) |
| `/api/claims/{id}/approval` | `IApprovalService` | → current read service (read only) |
| `/api/claims/{id}/audit` | `IAuditCostService` | → current read service |
| `/api/demo/scenario`, `/api/bff/*`, `/health`, `/api/system/demo-status` | BFF-owned | stays in BFF (no business service) |

Skeleton implementations return the **same shapes** the BFF already serves, so the 22 existing tests and the frontend contract stay green. Read ownership migrates into the services in later stages; persistence/writes/AI/events come in their own gates.
