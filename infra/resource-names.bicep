param appName string
param region string
param env string

output appInsightsName string = 'ai-${appName}-${region}-${env}'
output logAnalyticsWorkspaceName string = 'la-${appName}-${region}-${env}'
output storageAccountName string = toLower('sa${appName}${region}${env}')
output storageAccountInputContainerName string = 'input'
output storageAccountOutputContainerName string = 'output'
output appServicePlanName string = 'asp-${appName}-${region}-${env}'
output functionAppName string = 'func-${appName}-${region}-${env}'
output eventHubNamespaceName string = 'eh-${appName}-${region}-${env}'
output eventHubName string = 'input'
output managedIdentityName string = 'mi-${appName}-${region}-${env}'
output newBlobCreatedEventGridTopicName string = 'egt-NewInputBlobCreated-${appName}-${region}-${env}'
