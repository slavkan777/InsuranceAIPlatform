// Observability: Log Analytics workspace + workspace-based Application Insights.
// Cost guardrails: 30-day retention + daily ingestion cap (free tier ~5 GB/mo).
@description('Resource name prefix.')
param prefix string
@description('Environment name.')
param environmentName string
@description('Location.')
param location string
@description('Tags.')
param tags object

@description('Daily ingestion cap in GB (cost guardrail).')
param dailyQuotaGb int = 1

var lawName = '${prefix}-${environmentName}-law'
var appiName = '${prefix}-${environmentName}-appi'

resource law 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: lawName
  location: location
  tags: tags
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
    workspaceCapping: {
      dailyQuotaGb: dailyQuotaGb
    }
    features: {
      searchVersion: 1
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appiName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: law.id
    SamplingPercentage: 50
    RetentionInDays: 30
  }
}

output logAnalyticsName string = law.name
output logAnalyticsId string = law.id
output appInsightsName string = appInsights.name
output appInsightsConnectionString string = appInsights.properties.ConnectionString
