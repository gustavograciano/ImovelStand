// ImovelStand — IaC Azure
// Deploy:
//   az deployment group create --resource-group imovelstand-prod --template-file main.bicep \
//     --parameters sqlAdminPassword=<secret> jwtSecret=<secret>
targetScope = 'resourceGroup'

@description('Sufixo único para recursos (ex: prod, staging, dev).')
param environmentName string = 'prod'

@description('Região dos recursos.')
param location string = resourceGroup().location

@description('Senha do admin do Azure SQL.')
@secure()
param sqlAdminPassword string

@description('JWT SecretKey (min 32 chars).')
@secure()
param jwtSecret string

@description('Iugu API token (opcional).')
@secure()
param iuguApiToken string = ''

@description('SKU do App Service Plan.')
param appServicePlanSku string = 'B1'

@description('SKU do Azure SQL.')
param sqlDatabaseSku string = 'S0'

var prefix = 'imovelstand'
var sqlServerName = '${prefix}-sql-${environmentName}'
var sqlDbName = 'ImovelStandDb'
var storageAccountName = replace('${prefix}storage${environmentName}', '-', '')
var appServicePlanName = '${prefix}-plan-${environmentName}'
var apiAppName = '${prefix}-api-${environmentName}'
var webAppName = '${prefix}-web-${environmentName}'
var appInsightsName = '${prefix}-insights-${environmentName}'
var logWorkspaceName = '${prefix}-logs-${environmentName}'

// --- Observability ---

resource logWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logWorkspaceName
  location: location
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logWorkspace.id
  }
}

// --- Storage (fotos + contratos) ---

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
  }
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  name: '${storage.name}/default/imovelstand'
  properties: { publicAccess: 'None' }
}

// --- SQL ---

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: sqlDbName
  location: location
  sku: { name: sqlDatabaseSku, tier: 'Standard' }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    requestedBackupStorageRedundancy: 'Local'
  }
}

resource sqlFirewallAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllAzureServices'
  properties: { startIpAddress: '0.0.0.0', endIpAddress: '0.0.0.0' }
}

// --- App Service Plan ---

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: { name: appServicePlanSku, tier: 'Basic' }
  kind: 'linux'
  properties: { reserved: true }
}

// --- API ---

resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: apiAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      minTlsVersion: '1.2'
      healthCheckPath: '/api/health'
      alwaysOn: true
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
        { name: 'ConnectionStrings__DefaultConnection', value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${sqlDbName};User ID=sqladmin;Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;' }
        { name: 'Jwt__SecretKey', value: jwtSecret }
        { name: 'Jwt__Issuer', value: 'ImovelStand.Api' }
        { name: 'Jwt__Audience', value: 'ImovelStand.Api' }
        { name: 'Jwt__ExpirationInHours', value: '4' }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
        { name: 'ApplicationInsights__ConnectionString', value: appInsights.properties.ConnectionString }
        { name: 'Iugu__ApiToken', value: iuguApiToken }
        { name: 'FileStorage__Endpoint', value: '${storage.properties.primaryEndpoints.blob}' }
        { name: 'FileStorage__AccessKey', value: storage.listKeys().keys[0].value }
        { name: 'FileStorage__BucketName', value: 'imovelstand' }
      ]
    }
  }
}

// --- Web (Static Web App estilo / Linux App para SPA) ---

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'NODE|20-lts'
      minTlsVersion: '1.2'
      appSettings: [
        { name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE', value: 'false' }
      ]
    }
  }
}

// --- Alertas ---

resource errorAlertRule 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${prefix}-alert-5xx-${environmentName}'
  location: 'global'
  properties: {
    description: 'Erros 5xx > 10 em 5 minutos'
    severity: 2
    enabled: true
    scopes: [ apiApp.id ]
    windowSize: 'PT5M'
    evaluationFrequency: 'PT1M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'Http5xx'
          metricName: 'Http5xx'
          operator: 'GreaterThan'
          threshold: 10
          timeAggregation: 'Total'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
  }
}

// --- Outputs ---

output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output webUrl string = 'https://${webApp.properties.defaultHostName}'
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output appInsightsConnectionString string = appInsights.properties.ConnectionString
