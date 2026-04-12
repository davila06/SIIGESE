export const environment = {
  production: true,
  version: '1.0.0',
  apiUrl: 'https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io/api',
  useMockApi: false,
  /**
   * Azure Application Insights connection string.
   * Set this to the real value from Azure Portal → Application Insights → Overview → Connection String.
   * Format: InstrumentationKey=<guid>;IngestionEndpoint=https://...
   */
  appInsightsConnectionString: 'InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://eastus2-3.in.applicationinsights.azure.com/'
};
