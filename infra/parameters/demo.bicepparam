// Safe demo parameters for main.bicep — NO secrets, NO subscription ids, NO keys.
// Used at the deploy gate (later) via:  az deployment sub create -f infra/main.bicep -p infra/parameters/demo.bicepparam -l <region>
using './main.bicep'

param environmentName = 'demo'
param location = 'westeurope'
param prefix = 'iap'

// AI OFF by default → zero AI cost until AZURE_AI_CONTROLLED_DEMO_V0.1.
param enableAi = false

// Demo-data auto-delete window (blob lifecycle).
param blobTtlDays = 14

// insuranceApiImage intentionally left at its default placeholder until the image is
// published to GHCR in AZURE_MINIMAL_DEPLOY_V0.1.
// SQL Entra-admin object id is supplied at deploy time (NOT here) — never commit it.
