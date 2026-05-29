# Azure Resource Naming — InsuranceAIPlatform (v0.1)

Convention: `<prefix>-<env>-<role>` with `prefix=iap`. One resource group, one region, consistent tags.

| Resource | Name pattern | Example (`env=demo`) | Notes |
|---|---|---|---|
| Resource group | `rg-iap-<env>` | `rg-iap-demo` | tear-down unit |
| Static Web App | `iap-<env>-swa` | `iap-demo-swa` | Free tier |
| Container Apps env | `iap-<env>-cae` | `iap-demo-cae` | |
| Container App (api) | `iap-<env>-api` | `iap-demo-api` | minReplicas=0 |
| Managed Identity | `iap-<env>-api-mi` | `iap-demo-api-mi` | user-assigned |
| Key Vault | `iap<env>kv<hash>` | `iapdemokv<hash>` | ≤24 chars, globally unique (`uniqueString`) |
| Storage account | `iap<env>st<hash>` | `iapdemost<hash>` | ≤24 chars, lowercase, globally unique |
| SQL server | `iap-<env>-sql-<hash>` | `iap-demo-sql-<hash>` | globally unique |
| SQL database | `InsuranceAIPlatform` | — | same name as LocalDB → connection-string swap only |
| Log Analytics | `iap-<env>-law` | `iap-demo-law` | |
| App Insights | `iap-<env>-appi` | `iap-demo-appi` | workspace-based |
| Azure OpenAI (opt) | `iap-<env>-openai` | `iap-demo-openai` | only if `enableAi` |
| Document Intelligence (opt) | `iap-<env>-docintel` | `iap-demo-docintel` | F0 |
| AI Search (opt) | `iap-<env>-search` | `iap-demo-search` | Free tier |

## Tags (all resources)
`project=InsuranceAIPlatform`, `env=<env>`, `costCenter=portfolio-demo`, `managedBy=bicep`.

## Region
Single region (no cross-region egress). Default `westeurope` (low latency for a Ukraine-based operator; standard pricing). `eastus` is an acceptable cheap alternative. **Confirm the region at deploy time** and keep every resource in it. Note: a few SKUs (e.g. some AI models) are region-constrained — verify model availability in the chosen region during the AI gate.

## Globally-unique names
Key Vault / Storage / SQL server names use `uniqueString(resourceGroup().id)` suffixes so re-deploys are deterministic and collision-safe without hardcoding random values.
