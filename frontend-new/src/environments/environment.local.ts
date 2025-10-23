export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  version: '20251020-local-dev',
  enableLogging: true,
  mockApi: true, // Usar Mock API interceptor en local
  local: {
    // Configuración para desarrollo local
    useLocalStorage: true,
    debugMode: true,
    skipAuth: false, // Cambiar a true para saltarse autenticación en desarrollo
    defaultUser: {
      email: 'admin@sinseg.com',
      password: 'password123'
    },
    database: {
      // Configuración para tu instancia local
      local: {
        connectionString: 'Server=Karo\\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;Connection Timeout=30;',
        server: 'Karo\\SQLEXPRESS',
        database: 'SinsegAppDb',
        authentication: 'Windows'
      },
      // Opción Docker (alternativa)
      docker: {
        connectionString: 'Server=localhost,1433;Database=MiAppDb;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;',
        port: 1433,
        database: 'MiAppDb'
      },
      // Opción Azure SQL (producción)
      azure: {
        connectionString: 'Server=tcp:siinadseg-sqlserver-1019.database.windows.net,1433;Database=SiinadsegDB;User ID=siinadseg_admin;Password=P@ssw0rd123!;Encrypt=True;TrustServerCertificate=False;',
        server: 'siinadseg-sqlserver-1019.database.windows.net',
        database: 'SiinadsegDB'
      }
    }
  }
};