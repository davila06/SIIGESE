# SIIGESE - Desarrollo Local

## Requisitos del Sistema

Antes de ejecutar la aplicación localmente, asegúrate de tener instalado:

1. **.NET 8 SDK** - [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Node.js (versión 18 o superior)** - [Descargar aquí](https://nodejs.org/)
3. **SQL Server LocalDB** - Incluido con Visual Studio o [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)

## Inicio Rápido

### Opción 1: Script Automático (Recomendado)
Ejecuta el archivo `start-local.bat` desde la raíz del proyecto. Este script:
- Iniciará el backend en el puerto 5000
- Iniciará el frontend en el puerto 4200
- Abrirá automáticamente el navegador

### Opción 2: Inicio Manual

#### 1. Backend (API)
```bash
cd backend/src/WebApi
dotnet ef database update
dotnet run --urls "http://localhost:5000"
```

#### 2. Frontend (Angular)
```bash
cd frontend-new
npm install
npm start
```

## URLs de la Aplicación

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

## Configuración de Base de Datos

La aplicación usa SQL Server LocalDB por defecto. La cadena de conexión está configurada en:
```
backend/src/WebApi/appsettings.json
```

Para crear/actualizar la base de datos:
```bash
cd backend/src/WebApi
dotnet ef database update
```

## Usuarios de Prueba

Después de ejecutar las migraciones, tendrás estos usuarios disponibles:
- **Admin**: admin@sinseg.com / Admin123!
- **Usuario**: user@sinseg.com / User123!

## Estructura del Proyecto

```
enterprise-web-app/
├── backend/              # API .NET 8
│   └── src/WebApi/      # Proyecto principal
├── frontend-new/        # Aplicación Angular
├── start-local.bat     # Script de inicio automático
├── start-backend.bat   # Script solo backend
└── start-frontend.bat  # Script solo frontend
```

## Puertos Utilizados

- **5000**: Backend API
- **4200**: Frontend Angular (desarrollo)

## Resolución de Problemas

### Error de Base de Datos
Si tienes problemas con la base de datos:
1. Verifica que SQL Server LocalDB esté instalado
2. Ejecuta: `dotnet ef database update` desde `backend/src/WebApi`

### Error de Dependencias Frontend
Si hay problemas con las dependencias de Node:
1. Elimina la carpeta `node_modules`
2. Ejecuta: `npm install`

### Error de CORS
Si hay problemas de CORS, verifica que el frontend esté ejecutándose en el puerto 4200.

## Desarrollo

### Agregar Nuevas Migraciones
```bash
cd backend/src/WebApi
dotnet ef migrations add NombreDeLaMigracion
dotnet ef database update
```

### Build de Producción
```bash
cd frontend-new
npm run build:prod
```

## Contacto

Para problemas o dudas sobre el desarrollo local, contacta al equipo de desarrollo.