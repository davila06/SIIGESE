# Manual Técnico — SIINSEG
## Sistema Integral de Gestión de Seguros
**Versión:** 1.0 | **Fecha:** Marzo 2026 | **Confidencial**

---

## Tabla de Contenidos

1. [Visión General de la Arquitectura](#1-visión-general-de-la-arquitectura)
2. [Stack Tecnológico](#2-stack-tecnológico)
3. [Estructura del Proyecto](#3-estructura-del-proyecto)
4. [Backend — .NET 8 Clean Architecture](#4-backend--net-8-clean-architecture)
5. [Frontend — Angular 17](#5-frontend--angular-17)
6. [Base de Datos — SQL Server](#6-base-de-datos--sql-server)
7. [Autenticación y Seguridad](#7-autenticación-y-seguridad)
8. [API REST — Referencia Completa de Endpoints](#8-api-rest--referencia-completa-de-endpoints)
9. [Chat en Tiempo Real — SignalR](#9-chat-en-tiempo-real--signalr)
10. [Sistema de Emails](#10-sistema-de-emails)
11. [Motor de Cobros Automáticos](#11-motor-de-cobros-automáticos)
12. [Sistema de Logging](#12-sistema-de-logging)
13. [Rate Limiting](#13-rate-limiting)
14. [Configuración de Entornos](#14-configuración-de-entornos)
15. [Despliegue en Azure](#15-despliegue-en-azure)
16. [Migraciones de Base de Datos](#16-migraciones-de-base-de-datos)
17. [Variables de Entorno Requeridas](#17-variables-de-entorno-requeridas)
18. [Modelo de Datos Completo](#18-modelo-de-datos-completo)
19. [Flujos de Negocio](#19-flujos-de-negocio)
20. [Guía de Desarrollo Local](#20-guía-de-desarrollo-local)

---

## 1. Visión General de la Arquitectura

SIINSEG es una aplicación web empresarial **single-tenant** construida con Clean Architecture en el backend y una SPA Angular en el frontend. Está desplegada en Microsoft Azure.

```
┌─────────────────────────────────────────────────────────────────┐
│                        INTERNET                                  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
            ┌───────────────┴──────────────┐
            │                              │
    ┌───────▼──────┐              ┌────────▼────────┐
    │  Azure Static │              │  Azure Container │
    │   Web App     │              │     App /        │
    │  (Angular)    │◄────REST────►│  App Service     │
    │  Port: 443    │              │  (.NET 8 API)    │
    └───────────────┘              └────────┬────────┘
                                            │
                                   ┌────────▼────────┐
                                   │  Azure SQL       │
                                   │  Server          │
                                   │  (SiinadsegDB)   │
                                   └─────────────────┘
```

### Flujo principal de una petición
```
Browser → Angular SPA → HTTP Interceptor (agrega JWT) → .NET API
       → Controller → Application Service → Repository → EF Core → SQL Server
```

### Patrones de diseño utilizados
- **Clean Architecture** — Domain / Application / Infrastructure / WebApi
- **Repository Pattern** — abstracción sobre EF Core
- **Unit of Work** — transacciones consistentes
- **CQRS Light** — DTOs separados para lectura y escritura
- **Dependency Injection** — toda la capa de servicios
- **AutoMapper** — mapeo DTO ↔ Entity

---

## 2. Stack Tecnológico

### Backend
| Componente | Tecnología | Versión |
|---|---|---|
| Framework | ASP.NET Core | 8.0 |
| Lenguaje | C# | 12 |
| ORM | Entity Framework Core | 8.x |
| BD | SQL Server / Azure SQL | 2019+ |
| Autenticación | JWT Bearer | via `Microsoft.IdentityModel` |
| Tiempo real | SignalR | ASP.NET Core SignalR |
| Logging | Serilog | con sink File + Console |
| Validación | FluentValidation | 11.x |
| Mapeo | AutoMapper | 12.x |
| Rate Limiting | ASP.NET Core 8 built-in | `Microsoft.AspNetCore.RateLimiting` |
| Cache distribuida | Redis (prod) / MemoryCache (dev) | `StackExchange.Redis` |
| Swagger | Swashbuckle | OpenAPI 3.0 |

### Frontend
| Componente | Tecnología | Versión |
|---|---|---|
| Framework | Angular | 17 |
| Lenguaje | TypeScript | 5.x |
| UI Components | Angular Material | 17.x |
| Routing | Angular Router | standalone + lazy loading |
| HTTP | Angular HttpClient | con interceptors |
| State | BehaviorSubject (RxJS) | sin NgRx |
| Tiempo real | @microsoft/signalr | cliente web |
| Charts | — | (preparado) |
| Estilos | SCSS | tema neon cyberpunk |

### Infraestructura Azure
| Recurso | Servicio Azure |
|---|---|
| Frontend | Azure Static Web Apps |
| Backend | Azure Container Apps / Azure App Service |
| BD | Azure SQL Database |
| Registry | Azure Container Registry (ACR) |
| Cache | Azure Cache for Redis (opcional) |

---

## 3. Estructura del Proyecto

```
enterprise-web-app/
├── backend/
│   └── src/
│       ├── Domain/                    # Entidades, interfaces, enums
│       │   ├── Entities/
│       │   │   ├── User.cs
│       │   │   ├── Poliza.cs
│       │   │   ├── Cobro.cs
│       │   │   ├── Cotizacion.cs
│       │   │   ├── Reclamo.cs
│       │   │   ├── Cliente.cs
│       │   │   ├── EmailConfig.cs
│       │   │   ├── ChatSession.cs
│       │   │   ├── ChatMessage.cs
│       │   │   ├── Enums.cs
│       │   │   └── Entity.cs          # Base: Id, CreatedAt, UpdatedAt, IsDeleted
│       │   └── Interfaces/            # IRepository, IUnitOfWork, etc.
│       │
│       ├── Application/               # Lógica de negocio
│       │   ├── Services/
│       │   │   ├── AuthService.cs
│       │   │   ├── PolizaService.cs
│       │   │   ├── CobrosService.cs   # Motor de facturación
│       │   │   ├── ReclamoService.cs
│       │   │   ├── UserService.cs
│       │   │   ├── ClienteService.cs
│       │   │   ├── EmailConfigService.cs
│       │   │   └── EmailDashboardService.cs
│       │   ├── DTOs/                  # Data Transfer Objects
│       │   ├── Interfaces/            # IAuthService, IPolizaService, etc.
│       │   ├── Validators/            # FluentValidation validators
│       │   └── Mappings/              # AutoMapper profiles
│       │
│       ├── Infrastructure/            # Acceso a datos e implementaciones externas
│       │   ├── Data/
│       │   │   ├── ApplicationDbContext.cs
│       │   │   └── Repositories/
│       │   └── Services/
│       │       ├── CotizacionService.cs
│       │       ├── ExcelService.cs
│       │       ├── EmailService.cs
│       │       ├── NotificationService.cs
│       │       ├── TokenBlacklistService.cs
│       │       └── ChatService.cs
│       │
│       └── WebApi/                    # Capa de presentación
│           ├── Controllers/
│           │   ├── ApiController.cs   # Base controller
│           │   ├── PolizasController.cs
│           │   ├── CobrosController.cs
│           │   ├── CotizacionesController.cs
│           │   ├── ReclamosController.cs
│           │   ├── UsersController.cs
│           │   ├── EmailController.cs
│           │   ├── EmailConfigController.cs
│           │   ├── ChatController.cs
│           │   └── NotificationsController.cs
│           ├── Hubs/
│           │   └── ChatHub.cs         # SignalR hub
│           ├── Program.cs             # Entry point + DI setup
│           └── appsettings.json
│
├── frontend-new/
│   └── src/
│       └── app/
│           ├── auth/                  # Login, guards, interceptors
│           ├── polizas/               # Módulo pólizas
│           ├── cobros/                # Módulo cobros (lazy)
│           ├── cotizaciones/          # Módulo cotizaciones
│           ├── reclamos/              # Módulo reclamos (lazy)
│           ├── usuarios/              # Módulo usuarios (Admin only)
│           ├── emails/                # Módulo emails (lazy)
│           ├── configuracion/         # Módulo configuración (lazy, Admin)
│           ├── chat/                  # Módulo chat (lazy)
│           ├── services/              # Servicios Angular
│           ├── shared/                # Componentes compartidos, constantes
│           ├── models/                # Interfaces TypeScript
│           └── app-routing.module.ts
│
├── SQL scripts/                       # Migrations manuales (01_...10_...)
├── docs/                              # Esta documentación
└── roadmap-mcp-bot.md
```

---

## 4. Backend — .NET 8 Clean Architecture

### 4.1 Capa Domain

Contiene las entidades de negocio y sus contratos. **No depende de ninguna otra capa.**

#### Entidad base `Entity.cs`
```csharp
public abstract class Entity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
```

#### Enums del dominio
| Enum | Valores |
|---|---|
| `EstadoCobro` | Pendiente, Pagado, Cobrado, Vencido, Cancelado |
| `MetodoPago` | NoDefinido, Efectivo, Tarjeta, Transferencia, Cheque, Otros |
| `EstadoReclamo` | Pendiente, Abierto, EnRevision, EnProceso, Aprobado, Rechazado, Resuelto, Cerrado |
| `TipoReclamo` | Siniestro, Queja, Sugerencia, Reclamo, Otros |
| `PrioridadReclamo` | Baja, Media, Alta, Critica |
| `ChatMessageType` | User=1, Bot=2, System=3 |
| `ChatMessageStatus` | Sent=1, Read=2, Error=3 |

### 4.2 Capa Application

Orquesta la lógica de negocio. **Depende solo de Domain.**

Servicios clave:
- `AuthService` — login, JWT, token blacklist, reset de contraseña, cambio de contraseña
- `CobrosService` — **motor de facturación**: generación automática, cálculo de fechas, registro de pagos
- `PolizaService` — CRUD de pólizas, búsqueda por múltiples criterios
- `ReclamoService` — gestión completa del ciclo de vida de reclamos
- `UserService` — gestión de usuarios y roles
- `EmailDashboardService` — envío individual, masivo, notificaciones de cobros vencidos

### 4.3 Capa Infrastructure

Implementaciones concretas de los repositorios y servicios externos.

- `ApplicationDbContext` — EF Core DbContext con todos los DbSets
- Repositorios: `UserRepository`, `PolizaRepository`, `CobroRepository`, etc.
- `ExcelService` — importación masiva de pólizas desde `.xlsx`
- `EmailService` — envío SMTP con reintentos configurable
- `TokenBlacklistService` — gestión de tokens invalidados (Redis o MemoryCache)
- `ChatService` — persistencia de sesiones y mensajes de chat
- `NotificationService` — notificaciones en tiempo real vía SignalR

### 4.4 Capa WebApi

Controllers REST + SignalR Hub + configuración del pipeline.

#### Registro del pipeline en `Program.cs`
1. Serilog → logging
2. FluentValidation → validación automática de DTOs
3. AutoMapper → mapeo de perfiles
4. EF Core → `ApplicationDbContext` con SQL Server
5. JWT Bearer → autenticación
6. Authorization Policies → `AdminOnly`, `DataLoaderOnly`
7. CORS → orígenes configurables por entorno
8. Dependency Injection → todos los servicios
9. SignalR → hub de chat
10. Redis / MemoryCache → blacklist de tokens
11. Rate Limiter → ventanas fijas para `auth` y `api`
12. Swagger/OpenAPI → documentación interactiva

---

## 5. Frontend — Angular 17

### 5.1 Módulos y rutas

| Ruta | Componente / Módulo | Guard | Rol requerido |
|---|---|---|---|
| `/login` | `LoginComponent` | `loginGuard` (redirige si ya autenticado) | — |
| `/change-password` | `ChangePasswordComponent` | — | — |
| `/polizas` | `PolizasComponent` | `authGuard` | Cualquier usuario autenticado |
| `/polizas/upload` | `UploadPolizasComponent` | `authGuard` + `dataLoaderGuard` | Admin, DataLoader |
| `/cotizaciones` | `CotizacionesComponent` | `authGuard` | Cualquier usuario autenticado |
| `/cobros` | `CobrosModule` (lazy) | `authGuard` | Cualquier usuario autenticado |
| `/reclamos` | `ReclamosModule` (lazy) | `authGuard` | Cualquier usuario autenticado |
| `/emails` | `EmailsModule` (lazy) | `authGuard` | Cualquier usuario autenticado |
| `/configuracion` | `ConfiguracionModule` (lazy) | `authGuard` + `adminGuard` | Admin |
| `/usuarios` | `UsuariosComponent` | `authGuard` + `adminGuard` | Admin |
| `/chat` | `ChatModule` (lazy) | `authGuard` | Cualquier usuario autenticado |
| `/**` | Redirige a `/login` | — | — |

### 5.2 Guards

| Guard | Descripción |
|---|---|
| `authGuard` | Verifica que `AuthService.getCurrentUser() !== null`. Redirige a `/login`. |
| `adminGuard` | Verifica `AuthService.isAdmin()`. Redirige a `/polizas`. |
| `dataLoaderGuard` | Verifica que el usuario tenga rol `Admin` o `DataLoader`. |
| `loginGuard` | Si ya está autenticado, redirige fuera del login. |

### 5.3 HTTP Interceptor

`auth.interceptor.ts` agrega automáticamente el header `Authorization: Bearer {token}` a todas las peticiones a la API, leyendo el token de `sessionStorage`.

### 5.4 Gestión de estado

Sin estado global (NgRx no usado). Estado con `BehaviorSubject`:
- `AuthService.currentUser$` — usuario autenticado actual
- Cada componente gestiona su propio estado local con llamadas al servicio

### 5.5 Persistencia de sesión

El token JWT y el usuario se guardan en `sessionStorage` (no `localStorage`). Se invalidan automáticamente al cerrar la pestaña/navegador. Al recargar la página, se verifica la expiración del token antes de restaurar la sesión.

### 5.6 Tema visual

SCSS con variables CSS globales (neon cyberpunk):
- `--color-primary` — color principal (headings, headers de tabla)
- `--color-primary-glow` — efecto hover en filas
- `--color-bg` — fondo general
- `--color-surface` — fondo de cards/panels

### 5.7 Constantes del sistema (frontend)

Ubicadas en `frontend-new/src/app/shared/constants/currency.constants.ts`:
```typescript
MONEDAS_SISTEMA = [
  { value: 'CRC', label: 'Colones Costarricenses', symbol: '₡', locale: 'es-CR' },
  { value: 'USD', label: 'Dólares Americanos', symbol: '$', locale: 'en-US' },
  { value: 'EUR', label: 'Euros', symbol: '€', locale: 'es-ES' }
]

ASEGURADORAS_SISTEMA = [INS, SAGICOR, ASSA, BCR_SEGUROS, MAPFRE, OTROS]
```

Ubicadas en `frontend-new/src/app/models/cotizacion.model.ts`:
```typescript
TIPOS_SEGURO = [AUTO, VIDA, HOGAR, EMPRESARIAL]
MODALIDADES = [BASICO, PLUS, PREMIUM, TOTAL]
FRECUENCIAS = [DM/MENSUAL, BIMESTRAL, TRIMESTRAL, SEMESTRAL, ANUAL, QUINCENAL, SEMANAL]
```

---

## 6. Base de Datos — SQL Server

### 6.1 Base de datos

**Nombre:** `SiinadsegDB` (producción Azure) / `SinsegAppDb` (local)

### 6.2 Scripts de migración (en orden)

| Script | Descripción |
|---|---|
| `01_CreateDatabase.sql` | Crea la base de datos |
| `02_CreateTables.sql` | Tablas principales: Users, Roles, Polizas, etc. |
| `03_CreateIndexes.sql` | Índices de performance |
| `04_CreateForeignKeys.sql` | Restricciones de integridad referencial |
| `05_InsertInitialData.sql` | Datos semilla: roles, usuario admin inicial |
| `06_CreateCobrosTable.sql` | Tabla `Cobros` con todos sus campos |
| `07_MigrateCobroClienteNombreCompleto.sql` | Migración de campo de nombre completo en cobros |
| `08_MigrateReclamoClienteNombreCompleto.sql` | Migración de campo de nombre completo en reclamos |
| `09_AddObservacionesToPolizas.sql` | Campo `Observaciones` en Pólizas |
| `10_AddCobroEmailTemplate.sql` | Campos de template en `EmailConfig` |
| `10_CreateChatTables.sql` | Tablas `ChatSessions` y `ChatMessages` |

### 6.3 Tablas principales

| Tabla | Filas esperadas | Descripción |
|---|---|---|
| `Users` | 2–50 | Usuarios del sistema |
| `Roles` | 3–5 | Admin, Agent, DataLoader, etc. |
| `UserRoles` | N×M | Relación muchos-a-muchos |
| `Polizas` | 100–50,000 | Pólizas de seguros |
| `Cobros` | >> Polizas | Cobros generados automáticamente |
| `Cotizaciones` | Similar a Polizas | Cotizaciones/propuestas |
| `Reclamos` | Variable | Reclamos asociados a pólizas |
| `Clientes` | Similar a Polizas | Clientes del sistema |
| `EmailConfigs` | 1–5 | Configuraciones SMTP |
| `ChatSessions` | Por usuario | Sesiones de chat |
| `ChatMessages` | Por sesión | Mensajes individuales |
| `PasswordResetTokens` | Transitoria | Tokens de reset (expiran) |

### 6.4 Tipos de datos importantes

- Todos los campos monetarios: `decimal(18,2)`
- Fechas: `datetime2` (UTC en backend, DD-MM-YYYY en API response)
- IDs: `int IDENTITY(1,1)` (autonumérico)
- Strings variables: `nvarchar(max)` o `nvarchar(N)`
- Soft delete: campo `IsDeleted BIT DEFAULT 0` en todas las tablas principales

---

## 7. Autenticación y Seguridad

### 7.1 JWT

- **Algoritmo:** HMAC-SHA256 (HS256)
- **Expiración:** 8 horas (configurable en `Jwt:ExpirationHours`)
- **Claims incluidos:** `NameIdentifier` (userId), `Email`, `Name`, Roles como claims múltiples
- **Issuer/Audience:** configurable en `appsettings.json`
- **ClockSkew:** `TimeSpan.Zero` — sin tolerancia de tiempo

### 7.2 Token Blacklist

Al hacer logout, el token se agrega a la blacklist. En producción usa **Redis** para compartir entre instancias; en desarrollo usa `IDistributedMemoryCache`.

### 7.3 Password Hashing

Contraseñas hasheadas con `PBKDF2` (BCrypt-style) — nunca almacenadas en texto plano.

### 7.4 Rate Limiting

| Política | Límite | Ventana | Usado en |
|---|---|---|---|
| `auth` | 10 peticiones | 1 minuto | Endpoints de login/auth |
| `api` | 300 peticiones | 1 minuto | Endpoints generales |

Código de respuesta al superar el límite: `HTTP 429 Too Many Requests`.

### 7.5 CORS

Orígenes permitidos configurados en `appsettings.json`:
- `http://localhost:4200` (desarrollo)
- `https://localhost:4200` (desarrollo HTTPS)
- `https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net` (producción)

### 7.6 Roles del sistema

| Rol | Descripción | Acceso especial |
|---|---|---|
| `Admin` | Administrador completo | Usuarios, Configuración, todos los módulos |
| `Agent` | Agente de seguros | Cotizaciones, Pólizas, Cobros, Reclamos |
| `DataLoader` | Cargador de datos | Upload masivo de pólizas desde Excel |

### 7.7 RequiresPasswordChange

Al crear un usuario nuevo por el admin, el campo `RequiresPasswordChange = true`. En el primer login, el sistema redirige al componente `ChangePasswordComponent` antes de acceder al sistema.

---

## 8. API REST — Referencia Completa de Endpoints

> Base URL: `https://{backend-url}/api`  
> Todos requieren: `Authorization: Bearer {token}`  
> Fechas: formato `DD-MM-YYYY` en request/response

### 8.1 Auth — `/api/auth`

| Método | Endpoint | Auth | Body | Descripción |
|---|---|---|---|---|
| POST | `/auth/login` | ❌ | `{email, password}` | Login — devuelve JWT |
| POST | `/auth/logout` | ✅ | — | Invalida el token actual |
| POST | `/auth/forgot-password` | ❌ | `{email}` | Envía email de reset |
| POST | `/auth/reset-password` | ❌ | `{token, newPassword}` | Aplica nuevo password |
| POST | `/auth/change-password` | ✅ | `{currentPassword, newPassword, confirmPassword}` | Cambio por el propio usuario |

### 8.2 Pólizas — `/api/polizas`

| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| GET | `/polizas` | Todos | Lista todas las pólizas activas |
| GET | `/polizas/{id}` | Todos | Póliza por ID |
| GET | `/polizas/numero/{numero}` | Todos | Póliza por número |
| GET | `/polizas/buscar?termino=` | Todos | Búsqueda por número, nombre o cédula |
| GET | `/polizas/perfil/{perfilId}` | Todos | Pólizas de un perfil |
| GET | `/polizas/aseguradora/{aseg}` | Todos | Pólizas por aseguradora |
| POST | `/polizas` | Todos | Crear nueva póliza |
| PUT | `/polizas/{id}` | Todos | Actualizar póliza |
| DELETE | `/polizas/{id}` | Admin | Desactivar póliza (soft delete) |
| POST | `/polizas/upload` | Admin, DataLoader | Importar desde Excel (.xlsx) |

### 8.3 Cobros — `/api/cobros`

| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| GET | `/cobros` | Todos | Lista todos los cobros |
| GET | `/cobros/{id}` | Todos | Cobro por ID |
| GET | `/cobros/recibo/{numero}` | Todos | Cobro por número de recibo |
| GET | `/cobros/poliza/{polizaId}` | Todos | Cobros de una póliza |
| GET | `/cobros/stats` | Todos | Estadísticas: totales, pendientes, montos |
| POST | `/cobros` | Todos | Crear cobro manual |
| PUT | `/cobros/{id}/registrar` | Todos | Registrar pago de un cobro |
| POST | `/cobros/generar` | Admin | Trigger generación automática |
| DELETE | `/cobros/{id}` | Admin | Cancelar cobro |

### 8.4 Cotizaciones — `/api/cotizaciones`

| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| GET | `/cotizaciones` | Admin, DataLoader, Agent | Lista todas |
| GET | `/cotizaciones/{id}` | Admin, DataLoader, Agent | Por ID |
| POST | `/cotizaciones` | Admin, DataLoader, Agent | Crear cotización |
| PUT | `/cotizaciones/{id}` | Admin, DataLoader, Agent | Actualizar |
| PUT | `/cotizaciones/{id}/aprobar` | Admin, DataLoader, Agent | Aprobar → APROBADA |
| PUT | `/cotizaciones/{id}/rechazar` | Admin, DataLoader, Agent | Rechazar → RECHAZADA |
| POST | `/cotizaciones/{id}/convertir` | Admin, DataLoader, Agent | Convertir en Póliza |
| DELETE | `/cotizaciones/{id}` | Admin | Eliminar |

### 8.5 Reclamos — `/api/reclamos`

| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| GET | `/reclamos` | Todos | Lista todos |
| GET | `/reclamos/{id}` | Todos | Por ID |
| GET | `/reclamos/numero/{numero}` | Todos | Por número de reclamo |
| POST | `/reclamos` | Todos | Crear reclamo |
| PUT | `/reclamos/{id}` | Todos | Actualizar |
| PUT | `/reclamos/{id}/estado` | Todos | Cambiar estado |
| DELETE | `/reclamos/{id}` | Admin | Eliminar |

### 8.6 Usuarios — `/api`

| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| GET | `/users` | Admin | Lista todos los usuarios |
| GET | `/users/{id}` | Admin | Usuario por ID |
| POST | `/users` | Admin | Crear usuario |
| PUT | `/users/{id}` | Admin | Actualizar usuario |
| DELETE | `/users/{id}` | Admin | Desactivar usuario |
| GET | `/roles` | Admin | Lista de roles |

### 8.7 Email — `/api/email`

| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| GET | `/email/stats` | Todos | Estadísticas de envíos |
| POST | `/email/send` | Todos | Enviar email individual |
| POST | `/email/send-bulk` | Todos | Envío masivo |
| POST | `/email/send-cobro-vencido` | Todos | Notificación cobro vencido |
| GET | `/email/history` | Todos | Historial de emails enviados |
| GET | `/email/templates` | Todos | Plantillas disponibles |

### 8.8 Configuración Email — `/api/emailconfig`

| Método | Endpoint | Rol | Descripción |
|---|---|---|---|
| GET | `/emailconfig` | Admin | Lista configuraciones SMTP |
| POST | `/emailconfig` | Admin | Crear configuración |
| PUT | `/emailconfig/{id}` | Admin | Actualizar |
| POST | `/emailconfig/{id}/test` | Admin | Probar configuración SMTP |
| PUT | `/emailconfig/{id}/default` | Admin | Marcar como predeterminada |

### 8.9 Chat — `/api/chat`

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/chat/sessions` | Sesiones del usuario autenticado |
| POST | `/chat/sessions` | Crear nueva sesión |
| DELETE | `/chat/sessions/{id}` | Eliminar sesión |
| GET | `/chat/sessions/{id}/messages` | Mensajes de una sesión (paginado) |
| POST | `/chat/sessions/{id}/messages` | Enviar mensaje |
| PUT | `/chat/sessions/{id}/messages/{msgId}/read` | Marcar como leído |
| POST | `/chat/sessions/{id}/messages/{msgId}/react` | Like/Dislike a un mensaje |

---

## 9. Chat en Tiempo Real — SignalR

### Hub URL
```
/hubs/chat
```

### Métodos del cliente → servidor (invoke)

| Método | Parámetros | Descripción |
|---|---|---|
| `JoinSession` | `sessionId: string` | Unirse al grupo de una sesión |
| `LeaveSession` | `sessionId: string` | Salir del grupo |
| `SendTypingIndicator` | `sessionId: string, isTyping: bool` | Indicador de escritura |

### Eventos servidor → cliente (on)

| Evento | Payload | Descripción |
|---|---|---|
| `TypingIndicator` | `{sessionId, isTyping, timestamp}` | Indicador de escritura de otro usuario |
| `NewMessage` | `MessageDto` | Nuevo mensaje recibido en tiempo real |

### Grupos SignalR

- `session-{sessionId}` — todos los conectados a esa sesión
- `user-{userId}` — conexiones del usuario (para notificaciones personales)

---

## 10. Sistema de Emails

### Configuración SMTP

Almacenada en tabla `EmailConfigs`. Soporta:
- `SmtpServer` + `SmtpPort`
- SSL / TLS configurable
- `FromEmail` + `FromName`
- Reintentos: `MaxRetries` (default: 3)
- Timeout: `TimeoutSeconds` (default: 30s)
- Test de conectividad desde el panel admin
- Datos de empresa: `CompanyName`, `CompanyAddress`, `CompanyPhone`, `CompanyWebsite`, `CompanyLogo`

### Template de cobros (`CobroEmailBody`)

Campos de sustitución disponibles en el template:
- `{NombreCliente}` — nombre del asegurado
- `{NumeroPoliza}` — número de póliza
- `{NumeroRecibo}` — número de recibo
- `{MontoTotal}` — monto a cobrar con símbolo de moneda
- `{FechaVencimiento}` — fecha límite de pago
- `{CompanyName}`, `{CompanyPhone}` — datos de la agencia

---

## 11. Motor de Cobros Automáticos

Clase: `CobrosService.GenerarCobrosAutomaticosAsync(int mesesAdelante = 3)`

### Algoritmo de generación

```
1. SELECT todas las pólizas WHERE EsActivo = true
2. Para cada póliza:
   a. Si Prima <= 0 → SKIP
   b. Obtener cobros existentes → HashSet<DateTime> de fechas ya creadas
   c. fechas = CalcularFechasVencimiento(FechaVigencia, Frecuencia, ventana)
   d. Para cada fecha NOT IN HashSet:
      → nuevo Cobro { MontoTotal = Prima, Estado = Pendiente }
      → NumeroRecibo = "REC-{YYYYMM}-{counter++}"
3. SaveChanges()
```

### Ventana adaptativa mínima

```csharp
ventana = frecuencia switch {
    "ANUAL"          => Math.Max(mesesAdelante, 13),
    "SEMESTRAL"      => Math.Max(mesesAdelante, 7),
    "CUATRIMESTRAL"  => Math.Max(mesesAdelante, 5),
    "TRIMESTRAL"     => Math.Max(mesesAdelante, 4),
    "BIMESTRAL"      => Math.Max(mesesAdelante, 3),
    _                => mesesAdelante   // MENSUAL/DM default
}
```

### Mapeo de períodos `AgregarPeriodo()`

| Valores (case-insensitive) | Período |
|---|---|
| DM, DEBITO MENSUAL, MENSUAL, MONTHLY, MES | +1 mes |
| BIMESTRAL, BIMONTHLY, 2 MESES | +2 meses |
| TRIMESTRAL, QUARTERLY, 3 MESES | +3 meses |
| CUATRIMESTRAL, 4 MESES | +4 meses |
| SEMESTRAL, SEMIANNUAL, 6 MESES | +6 meses |
| ANUAL, ANNUAL, YEARLY, AÑO, YEAR | +1 año |
| QUINCENAL, BIWEEKLY, 15 DIAS | +15 días |
| SEMANAL, WEEKLY, SEMANA | +7 días |
| *(cualquier otro)* | +1 mes |

---

## 12. Sistema de Logging

Usa **Serilog** configurado en `Program.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)  // appsettings.json Serilog section
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

- Logs rotativos diarios en `logs/app-YYYY-MM-DD.txt`
- En Azure Container: logs accesibles vía Log Stream del portal
- Niveles: Debug (dev), Information (prod), Warning para eventos de seguridad
- Eventos de seguridad logueados: intentos de login fallidos, usuarios inactivos, contraseñas incorrectas

---

## 13. Rate Limiting

Configurado con el Rate Limiter nativo de ASP.NET Core 8:

```csharp
// Auth endpoints: máx 10 req/min (anti brute-force)
options.AddFixedWindowLimiter("auth", opt => {
    opt.PermitLimit = 10;
    opt.Window = TimeSpan.FromMinutes(1);
});

// API general: máx 300 req/min
options.AddFixedWindowLimiter("api", opt => {
    opt.PermitLimit = 300;
    opt.Window = TimeSpan.FromMinutes(1);
});
```

---

## 14. Configuración de Entornos

### `appsettings.json` (base)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{DB_CONNECTION_STRING}#",
    "LocalDbConnection": "Server=(localdb)\\mssqllocaldb;Database=SinsegAppDb;...",
    "Redis": ""
  },
  "Jwt": {
    "Secret": "#{JWT_SECRET}#",
    "Issuer": "MiApp",
    "Audience": "MiApp",
    "ExpirationHours": 8
  },
  "FileUpload": {
    "MaxFileSizeInMB": 10,
    "AllowedExtensions": [".xlsx", ".xls"]
  }
}
```

Los placeholders `#{...}#` son reemplazados por el pipeline de CI/CD de Azure.

### `appsettings.Development.json`

Sobreescribe solo lo necesario para desarrollo local (connection string local, logging verbose, HTTPS desactivado).

### `appsettings.Production.json`

Valores de producción Azure, HTTPS obligatorio, logging reducido.

---

## 15. Despliegue en Azure

### Frontend — Azure Static Web Apps

```bash
# Build
cd frontend-new
npm run build -- --configuration production

# Deploy automático vía GitHub Actions (configurado en .github/workflows)
# O manual:
az staticwebapp deploy --source ./dist/frontend-new --name {swa-name}
```

### Backend — Azure Container App / App Service

```bash
# Build Docker image
docker build -t siinseg-backend .

# Push a ACR
az acr build --registry {acr-name} --image siinseg-backend:latest .

# Update container app
az containerapp update --name {app-name} --resource-group {rg} \
  --image {acr-name}.azurecr.io/siinseg-backend:latest
```

Scripts disponibles:
- `deploy-backend.ps1` — deploy completo
- `deploy-backend-quick.ps1` — build + push + update rápido
- `actualizar-backend-container.ps1` — actualizar solo la imagen

### Conexión strings Azure

Configurar en **App Service Configuration** o **Container App Environment Variables**:
```
DB_CONNECTION_STRING = Server=tcp:{server}.database.windows.net,...
JWT_SECRET = {secret-de-al-menos-32-chars}
```

---

## 16. Migraciones de Base de Datos

El proyecto usa **EF Core Migrations** para la estructura principal y scripts SQL manuales para migraciones de datos.

### Crear nueva migración EF Core

```bash
cd backend/src
dotnet ef migrations add NombreMigracion --project Infrastructure --startup-project WebApi
dotnet ef database update --project Infrastructure --startup-project WebApi
```

### Aplicar scripts SQL manualmente

```bash
# Script PowerShell disponible:
./apply-migrations-azure.ps1

# O directamente con sqlcmd:
sqlcmd -S {server} -d SiinadsegDB -U {user} -P {pass} -i 06_CreateCobrosTable.sql
```

---

## 17. Variables de Entorno Requeridas

| Variable | Descripción | Requerida en |
|---|---|---|
| `DB_CONNECTION_STRING` | Connection string SQL Server / Azure SQL | Producción |
| `JWT_SECRET` | Secret key para firmar JWT (mín. 32 caracteres) | Producción |
| `JWT_ISSUER` | Issuer del JWT | Opcional (default: "MiApp") |
| `JWT_AUDIENCE` | Audience del JWT | Opcional (default: "MiApp") |
| `CORS_ALLOWED_ORIGINS` | Orígenes permitidos (JSON array) | Producción |
| `ConnectionStrings__Redis` | Redis connection string | Producción multi-instancia |
| `Email__SmtpServer` | Servidor SMTP por defecto | Opcional |
| `Email__Username` | Usuario SMTP | Opcional |
| `Email__Password` | Contraseña SMTP | Opcional |

---

## 18. Modelo de Datos Completo

### Diagrama de relaciones (simplificado)

```
Users ──< UserRoles >── Roles
  │
  └──< Reclamos (UsuarioAsignado)

Polizas ──< Cobros
        ──< Reclamos (NumeroPoliza FK)

Cotizaciones (independiente, se convierte en Poliza)

Clientes (perfil de cliente, referenciado por PerfilId en Polizas)

EmailConfigs (singleton de configuración)

ChatSessions ──< ChatMessages
    └── UserId → Users
```

### Campos clave por entidad

**Poliza**
```
NumeroPoliza, Modalidad, NombreAsegurado, NumeroCedula,
Prima (decimal), Moneda, FechaVigencia, Frecuencia,
Aseguradora, Placa, Marca, Modelo, Año,
Correo, NumeroTelefono, Observaciones,
PerfilId, EsActivo
```

**Cobro**
```
NumeroRecibo (REC-YYYYMM-NNNN), PolizaId,
NombreAsegurado, ClienteNombreCompleto,
MontoTotal (decimal), MontoCobrado (decimal), Moneda,
Estado (EstadoCobro), MetodoPago,
FechaCobro, FechaVencimiento, FechaPago,
Observaciones
```

**Cotizacion**
```
NumeroCotizacion (COT-YYYY-NNN), TipoSeguro, Modalidad,
NombreCliente, EmailCliente, TelefonoCliente,
MontoAsegurado, PrimaCotizada, Moneda, Frecuencia,
Estado (PENDIENTE/APROBADA/RECHAZADA/CONVERTIDA),
Aseguradora, FechaCreacion, FechaVigenciaInicio, FechaVigenciaFin,
Observaciones, UsuarioCreadorId,
[AUTO] Placa, Marca, Modelo, Año, Cilindraje,
[VIDA] FechaNacimiento, Genero, Ocupacion,
[HOGAR] DireccionInmueble, TipoInmueble, ValorInmueble
```

**Reclamo**
```
NumeroReclamo, NumeroPoliza,
TipoReclamo (enum), Descripcion,
FechaReclamo, FechaLimiteRespuesta, FechaResolucion,
Estado (EstadoReclamo), Prioridad (PrioridadReclamo),
MontoReclamado, MontoAprobado, Moneda,
NombreAsegurado, ClienteNombreCompleto,
Observaciones, DocumentosAdjuntos,
UsuarioAsignadoId
```

---

## 19. Flujos de Negocio

### Flujo completo: Cotización → Póliza → Cobros

```
1. Agente crea Cotización (estado: PENDIENTE)
   └── POST /api/cotizaciones
   
2. Admin/Agente aprueba
   └── PUT /api/cotizaciones/{id}/aprobar → APROBADA

3. Se convierte en Póliza
   └── POST /api/cotizaciones/{id}/convertir
   └── Crea Poliza con los datos de la cotización
   └── Cotización queda en estado: CONVERTIDA

4. Sistema genera cobros automáticos
   └── POST /api/cobros/generar
   └── Para cada período futuro (hasta ventana mínima) → crea Cobro con Estado: Pendiente

5. Agente registra pago
   └── PUT /api/cobros/{id}/registrar
   └── Estado → Pagado, FechaPago = now, MontoCobrado = input
```

### Flujo de reclamo

```
1. Cliente/Agente crea Reclamo (Pendiente)
2. Admin asigna a usuario (Abierto/EnRevision)
3. Usuario gestiona el caso (EnProceso)
4. Resolución: Aprobado o Rechazado
5. Cierre: Resuelto → Cerrado
```

### Flujo de autenticación

```
1. POST /auth/login { email, password }
2. Sistema verifica: ¿usuario activo? ¿contraseña válida?
3. Si RequiresPasswordChange=true → redirigir a /change-password
4. Si OK → JWT de 8h + sessionStorage
5. Todas las peticiones: Authorization: Bearer {token}
6. POST /auth/logout → token agregado a blacklist
```

---

## 20. Guía de Desarrollo Local

### Prerequisitos

- .NET 8 SDK
- Node.js 18+ y npm
- SQL Server LocalDB o SQL Server Express
- Visual Studio 2022 o VS Code

### Setup backend

```bash
cd backend/src/WebApi
# Opcional: editar appsettings.Development.json con tu connection string local
dotnet restore
dotnet ef database update --project ../Infrastructure
dotnet run
# API disponible en: http://localhost:5000
# Swagger en: http://localhost:5000/swagger
```

### Setup frontend

```bash
cd frontend-new
npm install
ng serve
# App en: http://localhost:4200
```

### Credenciales de desarrollo

El script `05_InsertInitialData.sql` crea:
- **Admin:** `admin@siinseg.com` / contraseña definida en el script

### Testing del API

Con Swagger UI: `http://localhost:5000/swagger`

1. Click en `POST /api/auth/login`
2. Execute con credenciales
3. Copiar el `token` del response
4. Click "Authorize" (candado) en la parte superior de Swagger
5. Pegar: `Bearer {token}`
6. Ahora todos los endpoints están disponibles

---

## Apéndice A — Formato de Fechas

El backend serializa/deserializa fechas en formato `DD-MM-YYYY` mediante un `JsonConverter` personalizado (`JsonDateTimeConverter.cs`). El frontend usa `parseBackendDate()` en `currency.constants.ts` para parsear estos formatos.

Formatos soportados en el parser:
- `DD-MM-YYYY`
- `DD-MM-YYYY HH:mm:ss`
- ISO 8601 (`YYYY-MM-DDTHH:mm:ss`)

## Apéndice B — Numeración de Documentos

| Documento | Formato | Ejemplo |
|---|---|---|
| Recibo de Cobro | `REC-{YYYYMM}-{NNNN}` | `REC-202603-0042` |
| Cotización | `COT-{YYYY}-{NNN}` | `COT-2026-001` |
| Reclamo | `REC-{YYYY}-{NNN}` o similar | configurable |

## Apéndice C — Importación Excel de Pólizas

El `ExcelService` procesa archivos `.xlsx` mediante `POST /api/polizas/upload`.
- Tamaño máximo: 10 MB (configurable en `FileUpload:MaxFileSizeInMB`)
- Extensiones permitidas: `.xlsx`, `.xls`
- El archivo debe tener headers en la primera fila con los nombres de campos esperados
- Filas con errores se reportan sin detener el proceso completo
- Rol requerido: Admin o DataLoader
