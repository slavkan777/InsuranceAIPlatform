// Azure Container Apps: managed environment + insurance-api container.
// minReplicas = 0 → scale-to-zero → $0 when idle (cold-start on first authed request).
// Uses the user-assigned managed identity (passwordless).
@description('Resource name prefix.')
param prefix string
@description('Environment name.')
param environmentName string
@description('Location.')
param location string
@description('Tags.')
param tags object
@description('Container image (placeholder public image until GHCR publish in the deploy gate).')
param image string
@description('User-assigned managed identity resource id.')
param userAssignedIdentityId string
@description('User-assigned managed identity client id (for DefaultAzureCredential in-app).')
param userAssignedClientId string
@description('Log Analytics workspace name (resolved here to wire container logs).')
param logAnalyticsName string
@description('Application Insights connection string (injected as env var).')
param appInsightsConnectionString string

resource law 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: logAnalyticsName
}

resource cae 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${prefix}-${environmentName}-cae'
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: law.properties.customerId
        sharedKey: law.listKeys().primarySharedKey
      }
    }
  }
}

resource app 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${prefix}-${environmentName}-api'
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: cae.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
    }
    template: {
      containers: [
        {
          name: 'insurance-api'
          image: image
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            { name: 'ASPNETCORE_URLS', value: 'http://+:8080' }
            { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
            { name: 'AZURE_CLIENT_ID', value: userAssignedClientId }
            // AI stays Mock by default in Azure too — real provider is an explicit later gate.
            { name: 'AiProvider__Mode', value: 'Mock' }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 2
        rules: [
          {
            name: 'http-scale'
            http: {
              metadata: {
                concurrentRequests: '20'
              }
            }
          }
        ]
      }
    }
  }
}

output fqdn string = app.properties.configuration.ingress.fqdn
output name string = app.name
