export const environment = {
  production: true,
  apiUrl: 'http://siinadseg-backend-1019.eastus.azurecontainer.io/api',
  version: '20251022-1750-azure-fixed-upload',
  enableLogging: true, // Habilitar logs para debugging
  azure: {
    sqlServer: 'siinadseg-sqlserver-1019.database.windows.net',
    database: 'SiinadsegDB',
    containerUrl: 'http://siinadseg-backend-1019.eastus.azurecontainer.io'
  }
};