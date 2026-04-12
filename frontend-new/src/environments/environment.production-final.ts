export const environment = {
  production: true,
  useMockApi: false,
  apiUrl: '/api',
  apiTimeout: 30000,
  enableLogging: false,
  version: '1.0.0',
  /**
   * Azure Application Insights connection string.
   * Set this to the real value from Azure Portal → Application Insights → Overview → Connection String.
   * Format: InstrumentationKey=<guid>;IngestionEndpoint=https://...
   */
  appInsightsConnectionString: 'InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://eastus2-3.in.applicationinsights.azure.com/'
};
