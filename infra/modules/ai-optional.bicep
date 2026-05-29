// OPTIONAL AI services — deployed ONLY when main.bicep is called with enableAi=true.
// Default is OFF → zero AI cost. Even when deployed, AI is advisory-only, manual-trigger,
// token/page-capped, Mock-default in the app. Free/cheap tiers chosen on purpose.
@description('Resource name prefix.')
param prefix string
@description('Environment name.')
param environmentName string
@description('Location.')
param location string
@description('Tags.')
param tags object
@description('Principal id of the app managed identity (granted Cognitive Services OpenAI User).')
param appPrincipalId string

// Built-in role: Cognitive Services OpenAI User (data-plane, no key needed)
var openAiUserRoleId = '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'

// Azure OpenAI (governed real LLM behind IAiProvider; manual-trigger only).
resource openai 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: '${prefix}-${environmentName}-openai'
  location: location
  tags: tags
  kind: 'OpenAI'
  sku: { name: 'S0' }
  properties: {
    customSubDomainName: '${prefix}-${environmentName}-openai'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true // force Entra/Managed-Identity (no API key)
  }
}

// Document Intelligence (FormRecognizer) — F0 FREE tier for claim doc/photo extraction.
resource docIntel 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: '${prefix}-${environmentName}-docintel'
  location: location
  tags: tags
  kind: 'FormRecognizer'
  sku: { name: 'F0' }
  properties: {
    customSubDomainName: '${prefix}-${environmentName}-docintel'
    publicNetworkAccess: 'Enabled'
  }
}

// Azure AI Search — FREE tier ONLY (Basic ~$75/mo is the cost trap, never used for demo).
resource search 'Microsoft.Search/searchServices@2024-06-01-preview' = {
  name: '${prefix}-${environmentName}-search'
  location: location
  tags: tags
  sku: { name: 'free' }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
  }
}

resource openAiRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(openai.id, appPrincipalId, openAiUserRoleId)
  scope: openai
  properties: {
    principalId: appPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', openAiUserRoleId)
  }
}

output openAiName string = openai.name
output docIntelName string = docIntel.name
output searchName string = search.name
