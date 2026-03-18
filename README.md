# SINSEG — Sistema de Gestión de Seguros

Aplicación web empresarial de gestión de seguros que cubre el ciclo completo: pólizas, cotizaciones, reclamos, cobros, usuarios y configuración de correo electrónico.

## Stack tecnológico

| Capa | Tecnología |
|------|-----------|
| Backend | ASP.NET Core 8 — Clean Architecture |
| Frontend | Angular 20 / TypeScript 5.8.3 |
| Base de datos | SQL Server / Azure SQL |
| Cache | Redis (StackExchange.Redis) |
| ORM | Entity Framework Core 8 |
| Autenticación | JWT Bearer (tokens en `sessionStorage`) |
| Validación | FluentValidation 11 |
| Mapeo | AutoMapper 12 |
| Logging | Serilog (archivo) |
| Documentación API | Swagger / Swashbuckle |
| Despliegue | Azure App Service (backend) + Azure Static Web Apps (frontend) |

---

## Estructura del proyecto

```
enterprise-web-app/
├── backend/
│   ├── src/
│   │   ├── Domain/          # Entidades, enums, interfaces de dominio
│   │   ├── Application/     # Servicios de negocio, DTOs, interfaces de repositorios
│   │   ├── Infrastructure/  # EF Core, repositorios, servicios externos
│   │   └── WebApi/          # Controladores REST, Program.cs, configuración
│   ├── tests/
│   │   ├── UnitTests/       # Pruebas unitarias de servicios
│   │   └── IntegrationTests/# Pruebas de integración de controladores
│   └── enterprise-web-app.sln
└── frontend-new/
    └── src/app/
        ├── auth/            # Login, guards de autenticación
        ├── polizas/         # Gestión de pólizas
        ├── reclamos/        # Gestión de reclamos
        ├── cobros/          # Gestión de cobros
        ├── cotizaciones/    # Cotizaciones
        ├── emails/          # Dashboard y plantillas de correo
        ├── configuracion/   # Configuración SMTP
        ├── usuarios/        # Administración de usuarios
        ├── services/        # Servicios compartidos
        └── interfaces/      # Tipos e interfaces TypeScript
```

### Módulos funcionales (frontend)

- **Pólizas** — Alta, edición, consulta y seguimiento de pólizas de seguro
- **Reclamos** — Ciclo de vida completo: apertura, asignación, resolución y rechazo
- **Cobros** — Registro y seguimiento de cobros asociados a pólizas
- **Cotizaciones** — Generación y gestión de cotizaciones
- **Emails** — Dashboard de estadísticas y gestión de plantillas de correo
- **Configuración SMTP** — Configuración de servidores de correo con prueba de conexión
- **Usuarios** — CRUD de usuarios con roles y permisos

### API REST (backend)

| Controlador | Ruta base |
|-------------|-----------|
| `UsersController` | `/api/users` |
| `PolizasController` | `/api/polizas` |
| `ReclamosController` | `/api/reclamos` |
| `CobrosController` | `/api/cobros` |
| `CotizacionesController` | `/api/cotizaciones` |
| `EmailConfigController` | `/api/emailconfig` |
| `NotificationsController` | `/api/notifications` |

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 20+](https://nodejs.org/) y npm
- SQL Server (local) o cadena de conexión a Azure SQL
- Redis (opcional, para caché)

---

## Desarrollo local

### Backend

```powershell
cd backend

# Restaurar dependencias
dotnet restore

# Aplicar migraciones y crear la base de datos
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi

# Ejecutar en modo desarrollo
dotnet run --project src/WebApi
```

La API queda disponible en `https://localhost:7001` (o el puerto configurado).  
Swagger UI: `https://localhost:7001/swagger`

### Frontend

```powershell
cd frontend-new

# Instalar dependencias
npm install

# Servidor de desarrollo (http://localhost:4200)
ng serve

# Build de producción
ng build --configuration production
```

---

## Variables de entorno / configuración

Crear `backend/src/WebApi/appsettings.Development.json` con:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SinsegDb;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "<clave-secreta>",
    "Issuer": "SinsegApi",
    "Audience": "SinsegApp",
    "ExpirationMinutes": 60
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

---

## Pruebas

```powershell
# Desde la raíz del backend
cd backend
dotnet test
```

Incluye pruebas unitarias (`UnitTests`) e integración (`IntegrationTests`).

---

## Despliegue en Azure

El proyecto cuenta con scripts de despliegue automatizado en la raíz:

| Script | Propósito |
|--------|-----------|
| `deploy-backend.ps1` | Despliega el backend en Azure App Service |
| `deploy-frontend-swa.ps1` | Despliega el frontend en Azure Static Web Apps |
| `apply-migrations-azure.ps1` | Aplica migraciones EF Core sobre Azure SQL |
| `actualizar-backend-container.ps1` | Actualiza el contenedor del backend |

Ver [AZURE_DEPLOYMENT_COMPLETE_GUIDE.md](AZURE_DEPLOYMENT_COMPLETE_GUIDE.md) para instrucciones detalladas.

---

## Arquitectura — Consideraciones de escala

### Token blacklist (logout / revocación de JWT)

El backend implementa `TokenBlacklistService` sobre `IDistributedCache`. El comportamiento en producción depende de la configuración:

| Escenario | Implementación activa | Notas |
|-----------|----------------------|-------|
| **Single-instance** (configuración actual) | `AddDistributedMemoryCache()` — caché en proceso | Los tokens revocados se pierden al reiniciar la instancia; aceptable en single-instance porque cada solicitud llega al mismo proceso. |
| **Multi-instance** (escalado horizontal) | `AddStackExchangeRedisCache()` — Azure Cache for Redis | Activar configurando `ConnectionStrings__Redis` en App Service → Configuration → Application settings. |

> **Nota de operación:** El despliegue actual es single-instance (Azure App Service, un solo plan). Si en el futuro se activa el escalado horizontal, debe provisionarse Azure Cache for Redis (Basic C0 ~$17/mes) **antes** de escalar, para evitar que tokens revocados sean aceptados por instancias que no comparten la caché en memoria.

---

## Calidad de código

- TypeScript estricto: `strict: true`, `strictTemplates: true`, `strictInjectionParameters: true`
- `<Nullable>enable</Nullable>` habilitado en todos los proyectos .NET
- Sin supresiones de errores (`@ts-ignore`, `#pragma warning disable`, `NoWarn`) en código de producción
- JWT almacenado en `sessionStorage` (no `localStorage`)

---

## Licencia

MIT License — ver [LICENSE](LICENSE) para más detalles.
