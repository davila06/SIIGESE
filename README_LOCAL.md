# OmnIA - Desarrollo Local

## Requisitos del Sistema

Antes de ejecutar la aplicaciÃ³n localmente, asegÃºrate de tener instalado:

1. **.NET 8 SDK** - [Descargar aquÃ­](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Node.js (versiÃ³n 18 o superior)** - [Descargar aquÃ­](https://nodejs.org/)
3. **SQL Server LocalDB** - Incluido con Visual Studio o [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)

## Inicio RÃ¡pido

### OpciÃ³n 1: Script AutomÃ¡tico (Recomendado)
Ejecuta el archivo `start-local.bat` desde la raÃ­z del proyecto. Este script:
- IniciarÃ¡ el backend en el puerto 5000
- IniciarÃ¡ el frontend en el puerto 4200
- AbrirÃ¡ automÃ¡ticamente el navegador

### OpciÃ³n 2: Inicio Manual

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

## URLs de la AplicaciÃ³n

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

## ConfiguraciÃ³n de Base de Datos

La aplicaciÃ³n usa SQL Server LocalDB por defecto. La cadena de conexiÃ³n estÃ¡ configurada en:
```
backend/src/WebApi/appsettings.json
```

Para crear/actualizar la base de datos:
```bash
cd backend/src/WebApi
dotnet ef database update
```

## Usuarios de Prueba

DespuÃ©s de ejecutar las migraciones, tendrÃ¡s estos usuarios disponibles:
- **Admin**: admin@sinseg.com / Admin123!
- **Usuario**: user@sinseg.com / User123!

## Estructura del Proyecto

```
enterprise-web-app/
â”œâ”€â”€ backend/              # API .NET 8
â”‚   â””â”€â”€ src/WebApi/      # Proyecto principal
â”œâ”€â”€ frontend-new/        # AplicaciÃ³n Angular
â”œâ”€â”€ start-local.bat     # Script de inicio automÃ¡tico
â”œâ”€â”€ start-backend.bat   # Script solo backend
â””â”€â”€ start-frontend.bat  # Script solo frontend
```

## Puertos Utilizados

- **5000**: Backend API
- **4200**: Frontend Angular (desarrollo)

## ResoluciÃ³n de Problemas

### Error de Base de Datos
Si tienes problemas con la base de datos:
1. Verifica que SQL Server LocalDB estÃ© instalado
2. Ejecuta: `dotnet ef database update` desde `backend/src/WebApi`

### Error de Dependencias Frontend
Si hay problemas con las dependencias de Node:
1. Elimina la carpeta `node_modules`
2. Ejecuta: `npm install`

### Error de CORS
Si hay problemas de CORS, verifica que el frontend estÃ© ejecutÃ¡ndose en el puerto 4200.

## Desarrollo

### Agregar Nuevas Migraciones
```bash
cd backend/src/WebApi
dotnet ef migrations add NombreDeLaMigracion
dotnet ef database update
```

### Build de ProducciÃ³n
```bash
cd frontend-new
npm run build:prod
```

## Contacto

Para problemas o dudas sobre el desarrollo local, contacta al equipo de desarrollo.
