export const environment = {
  production: true,
  apiUrl: '/api',  // Usar la API integrada de Static Web App
  version: '20251024-0245-mixed-content-fix',
  enableLogging: true, // Habilitar logs para debugging
  mockApi: false, // Deshabilitar Mock API en producción
  azure: {
    sqlServer: 'sql-siinadseg-7266.database.windows.net',
    database: 'SiinadsegDB',
    staticWebApp: 'https://gentle-dune-0a2edab0f.3.azurestaticapps.net'
  }
};