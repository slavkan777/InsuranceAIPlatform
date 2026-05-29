// Key Vault (RBAC-authorization). Holds future secrets (e.g. DEEPSEEK / Azure AI keys,
// SQL connection notes) added OUT OF BAND by the deploy gate — NEVER committed to the repo.
// The app's managed identity is granted "Key Vault Secrets User" (read-only).
@description('Resource name prefix.')
param prefix string
@description('Environment name.')
param environmentName string
@description('Location.')
param location string
@description('Tags.')
param tags object
@description('Principal id of the app managed identity (granted Secrets User).')
param appPrincipalId string

// Globally-unique, <=24 chars. uniqueString keeps it deterministic + collision-safe.
var kvName = take('${prefix}${environmentName}kv${uniqueString(resourceGroup().id)}', 24)

// Built-in role: Key Vault Secrets User
var kvSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: kvName
  location: location
  tags: tags
  properties: {
    sku: { family: 'A', name: 'standard' }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    publicNetworkAccess: 'Enabled'
  }
}

resource kvRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(kv.id, appPrincipalId, kvSecretsUserRoleId)
  scope: kv
  properties: {
    principalId: appPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', kvSecretsUserRoleId)
  }
}

output name string = kv.name
output id string = kv.id
output uri string = kv.properties.vaultUri
