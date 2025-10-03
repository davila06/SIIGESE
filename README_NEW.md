# Aplicación Web Empresarial - Sistema de Gestión de Pólizas de Seguros

## Descripción General

Aplicación web empresarial completa desarrollada con **Clean Architecture** para la gestión de pólizas de seguros. El sistema permite importar datos desde archivos Excel con estructura específica, gestionar clientes, y administrar pólizas de seguros con funcionalidades completas de CRUD.

## Arquitectura del Sistema

### Backend (.NET 6)
- **Clean Architecture** con separación clara de responsabilidades
- **Domain Layer**: Entidades y reglas de negocio
- **Application Layer**: DTOs, servicios de aplicación, validaciones
- **Infrastructure Layer**: Acceso a datos, servicios externos
- **WebAPI Layer**: Controladores, configuración, middleware

### Frontend (Angular 16)
- **Arquitectura modular** con lazy loading
- **Material Design** para componentes UI
- **Reactive Forms** con validaciones
- **RxJS** para programación reactiva

### Base de Datos
- **SQL Server** con Entity Framework Core
- **Code First** con migraciones automáticas
- **Auditoria completa** (Created/Modified timestamps y usuarios)

## Funcionalidades Principales

### 🔐 Autenticación y Autorización
- JWT Bearer Token authentication
- Role-based authorization (Admin, User)
- Refresh tokens para sesiones extendidas
- Guards para protección de rutas

### 📊 Gestión de Pólizas de Seguros
- **Importación masiva** desde Excel con estructura específica:
  - POLIZA, MOD, NOMBRE, PRIMA, MONEDA, FECHA, FRECUENCIA, ASEGURADORA, PLACA, MARCA, MODELO
- **CRUD completo** para pólizas individuales
- **Filtros avanzados** por aseguradora, moneda, fechas
- **Validaciones** de datos con FluentValidation
- **Exportación** a Excel

### 👥 Gestión de Clientes
- Registro y gestión de clientes
- Importación masiva desde Excel
- Perfiles de usuario asociados

### 🛡️ Características Técnicas
- **Logging** con Serilog
- **Health Checks** para monitoreo
- **CORS** configurado para desarrollo
- **Swagger/OpenAPI** para documentación
- **AutoMapper** para mapeo de DTOs
- **Validación** con FluentValidation

## Estructura del Proyecto

```
enterprise-web-app/
├── backend/
│   ├── src/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   ├── Role.cs
│   │   │   │   ├── Cliente.cs
│   │   │   │   ├── Poliza.cs
│   │   │   │   └── ...
│   │   │   └── Interfaces/
│   │   ├── Application/
│   │   │   ├── DTOs/
│   │   │   ├── Services/
│   │   │   ├── Mappings/
│   │   │   └── Validators/
│   │   ├── Infrastructure/
│   │   │   ├── Data/
│   │   │   ├── Repositories/
│   │   │   └── Services/
│   │   └── WebApi/
│   │       ├── Controllers/
│   │       ├── Program.cs
│   │       └── appsettings.json
│   └── tests/
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/
│   │   │   ├── features/
│   │   │   │   ├── dashboard/
│   │   │   │   └── polizas/
│   │   │   └── shared/
│   │   └── environments/
├── docker-compose.yml
└── README.md
```

## Configuración y Ejecución

### Requisitos Previos
- .NET 6 SDK
- Node.js 16+ y npm
- SQL Server (Local/Docker)
- Docker (opcional)

### Configuración del Backend

1. **Configurar cadena de conexión**:
   ```json
   // appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=EnterpriseWebApp;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```

2. **Ejecutar migraciones**:
   ```bash
   cd backend
   dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
   ```

3. **Ejecutar la aplicación**:
   ```bash
   cd backend/src/WebApi
   dotnet run
   ```

### Configuración del Frontend

1. **Instalar dependencias**:
   ```bash
   cd frontend
   npm install
   ```

2. **Configurar environment**:
   ```typescript
   // src/environments/environment.ts
   export const environment = {
     production: false,
     apiUrl: 'https://localhost:5001/api'
   };
   ```

3. **Ejecutar la aplicación**:
   ```bash
   ng serve
   ```

### Ejecución con Docker

```bash
docker-compose up -d
```

## Formato de Archivo Excel para Importación

### Estructura Requerida
El archivo Excel debe contener las siguientes columnas en este orden:

| POLIZA | MOD | NOMBRE | PRIMA | MONEDA | FECHA | FRECUENCIA | ASEGURADORA | PLACA | MARCA | MODELO |
|--------|-----|--------|-------|--------|-------|------------|-------------|-------|-------|---------|
| POL001 | AUTO | Juan Pérez | 500000 | COP | 01/01/2024 | Anual | Seguros ABC | ABC123 | Toyota | Corolla |

### Validaciones Aplicadas
- **POLIZA**: Requerido, único, máximo 50 caracteres
- **MOD**: Requerido, máximo 100 caracteres
- **NOMBRE**: Requerido, máximo 200 caracteres
- **PRIMA**: Requerido, numérico, mayor a 0
- **MONEDA**: Requerido, código de 3 letras (COP, USD, EUR)
- **FECHA**: Requerido, formato fecha válido
- **FRECUENCIA**: Requerido, valores: Mensual, Trimestral, Semestral, Anual
- **ASEGURADORA**: Requerido, máximo 100 caracteres
- **PLACA, MARCA, MODELO**: Opcionales, máximo 50 caracteres

## API Endpoints

### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/refresh` - Renovar token

### Pólizas
- `GET /api/polizas` - Obtener todas las pólizas
- `GET /api/polizas/{id}` - Obtener póliza por ID
- `GET /api/polizas/numero/{numero}` - Obtener póliza por número
- `GET /api/polizas/perfil/{perfilId}` - Obtener pólizas por perfil
- `POST /api/polizas` - Crear nueva póliza
- `PUT /api/polizas/{id}` - Actualizar póliza
- `DELETE /api/polizas/{id}` - Eliminar póliza
- `POST /api/polizas/upload` - Importar pólizas desde Excel

### Clientes
- `GET /api/clientes` - Obtener todos los clientes
- `POST /api/clientes` - Crear nuevo cliente
- `POST /api/clientes/upload` - Importar clientes desde Excel

## Testing

### Backend (xUnit + Moq)
```bash
cd backend
dotnet test
```

### Frontend (Jasmine + Karma)
```bash
cd frontend
npm test
```

## Deployment

### Preparación para Producción

1. **Backend**:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Frontend**:
   ```bash
   npm run build --prod
   ```

### Variables de Entorno
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection`
- `JWT__SecretKey`
- `JWT__Issuer`
- `JWT__Audience`

## Monitoreo y Logs

### Serilog Configuration
- Logs estructurados en archivos
- Diferentes niveles: Information, Warning, Error
- Rotación automática de archivos

### Health Checks
- `/health` - Estado general de la aplicación
- `/health/ready` - Disponibilidad para recibir tráfico
- `/health/live` - Estado de vida de la aplicación

## Seguridad

### Medidas Implementadas
- JWT con tiempo de expiración configurable
- Validación de entrada en todos los endpoints
- Autorización basada en roles
- CORS configurado para dominios específicos
- Sanitización de datos Excel

### Recomendaciones Adicionales
- Implementar rate limiting
- Usar HTTPS en producción
- Configurar CSP headers
- Implementar logging de eventos de seguridad

## Contribución

1. Fork del repositorio
2. Crear rama para feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit de cambios (`git commit -am 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Crear Pull Request

## Licencia

Este proyecto está bajo la Licencia MIT. Ver archivo `LICENSE` para más detalles.

## Soporte

Para soporte técnico, crear un issue en el repositorio o contactar al equipo de desarrollo.

---

**Versión**: 1.0.0  
**Última actualización**: Diciembre 2024  
**Compatibilidad**: .NET 6, Angular 16, SQL Server 2019+