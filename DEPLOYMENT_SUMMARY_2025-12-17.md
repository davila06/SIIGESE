# RESUMEN DE DEPLOYMENT - SIINADSEG
Fecha: 2025-12-17

## 1. RECURSOS AZURE CREADOS ✅

### SQL Server y Base de Datos
- **Resource Group**: rg-siinadseg-prod-2025
- **SQL Server**: siinadseg-sql-3376.database.windows.net
- **Database**: SiinadsegDB
- **Usuario SQL**: siinadsegadmin
- **Contraseña SQL**: n-IC*6GNdiKvuk#P
- **Tier**: Standard S0
- **Región**: East US 2

### Container Registry
- **ACR Name**: acrsiinadseg7512
- **Login Server**: acrsiinadseg7512.azurecr.io
- **Tier**: Basic
- **Admin Enabled**: Yes

## 2. BASE DE DATOS CONFIGURADA ✅

### Migraciones Aplicadas
- ✅ InitialCreate - Tablas básicas (Users, Roles, Polizas, etc.)
- ✅ AddMonedaAndCotizaciones - Módulo de cotizaciones
- ✅ AddReclamosModule - Módulo de reclamos
- ✅ AddEmailConfigTable - Configuración de email
- ✅ **AddCorreoElectronicoToCobro** - Campo de email en cobros (NUEVO)

### Roles Creados
1. **Admin** (ID: 1) - Administrador del sistema
2. **DataLoader** (ID: 2) - Cargador de datos
3. **User** (ID: 3) - Usuario estándar

### Usuario Administrador
- **Email**: admin@sinseg.com
- **Password**: admin123
- **Rol**: Admin
- **Estado**: Activo

## 3. BACKEND DEPLOYMENT 🔄

### En Proceso
- ✅ Código fuente actualizado con campo CorreoElectronico
- ✅ Azure Container Registry creado
- 🔄 Build de imagen Docker en ACR (en progreso, tarda 3-5 minutos)
- ⏳ Creación de Azure Container Instance (pendiente)

### Configuración Backend
```json
{
  "ConnectionString": "Server=tcp:siinadseg-sql-3376.database.windows.net,1433;Initial Catalog=SiinadsegDB;...",
  "ASPNETCORE_ENVIRONMENT": "Production",
  "Jwt__Secret": "MySecretKey123456789MySecretKey123456789",
  "Jwt__Issuer": "SiinadsegApp",
  "Jwt__Audience": "SiinadsegApp",
  "Jwt__ExpirationHours": "8"
}
```

## 4. CAMBIOS IMPLEMENTADOS EN ESTE DEPLOYMENT

### Nuevo Campo: CorreoElectronico en Cobros

#### Backend
1. **Entidad** (`Cobro.cs`):
   ```csharp
   public string? CorreoElectronico { get; set; }
   ```

2. **DTOs** (`CobroDto.cs`, `CobroDtos.cs`):
   - Agregado `CorreoElectronico` en CobroDto
   - Agregado `CorreoElectronico` en CobroRequestDto

3. **Servicio** (`CobrosService.cs`):
   ```csharp
   CorreoElectronico = request.CorreoElectronico ?? poliza.Correo
   ```
   - Usa el email del request o hereda de la póliza

4. **NotificationService** (`NotificationService.cs`):
   ```csharp
   ClienteEmail = c.CorreoElectronico ?? ""
   ```
   - Ahora usa el email real del cobro

5. **Migración**:
   - Migration: `20251217060042_AddCorreoElectronicoToCobro`
   - Column: `ALTER TABLE [Cobros] ADD [CorreoElectronico] nvarchar(max) NULL`

#### Frontend
1. **Interface** (`cobro.interface.ts`):
   ```typescript
   correoElectronico?: string;
   ```

2. **Formulario** (`agregar-cobro-dialog.component.ts`):
   ```typescript
   correoElectronico: ['', [Validators.email]]
   ```
   - Validación de formato de email

3. **Template** (`agregar-cobro-dialog.component.html`):
   - Input con ícono de email
   - Mensaje de error para email inválido

4. **Tabla** (`cobros-dashboard.component.html`):
   - Nueva columna "Correo Electrónico"
   - Muestra ícono + email o "-" si está vacío

5. **Estilos** (`cobros-dashboard.component.scss`):
   - `.email-cell` - Para el ícono y texto
   - `.no-email` - Para cuando no hay email

## 5. PRÓXIMOS PASOS

### Inmediato (Automatizado)
1. ⏳ Esperar finalización del build de imagen en ACR
2. ⏳ Crear Azure Container Instance con la imagen
3. ⏳ Configurar DNS y obtener URL pública del backend

### Manual (Requiere acción)
1. 🔲 Deploy del frontend a Azure Static Web Apps
2. 🔲 Actualizar `environment.prod.ts` con la URL del backend
3. 🔲 Configurar CORS en el backend con la URL del frontend
4. 🔲 Probar login con usuario administrador
5. 🔲 Verificar que el campo email funciona en el módulo de cobros

## 6. COMANDOS ÚTILES

### Verificar Estado de Recursos
```powershell
# Ver estado del build en ACR
az acr task logs --registry acrsiinadseg7512

# Ver estado del SQL Server
az sql server show --name siinadseg-sql-3376 --resource-group rg-siinadseg-prod-2025

# Listar recursos en el grupo
az resource list --resource-group rg-siinadseg-prod-2025 --output table
```

### Conectarse a la Base de Datos
```powershell
# Con Azure Data Studio o SQL Server Management Studio
Server: siinadseg-sql-3376.database.windows.net
Database: SiinadsegDB
Authentication: SQL Server Authentication
Username: siinadsegadmin
Password: n-IC*6GNdiKvuk#P
```

### Verificar Migraciones
```bash
cd backend/src/Infrastructure
dotnet ef migrations list --startup-project ../WebApi/WebApi.csproj
```

## 7. INFORMACIÓN DE SEGURIDAD

⚠️ **IMPORTANTE**: Guarda esta información en un lugar seguro:

- **SQL Password**: n-IC*6GNdiKvuk#P
- **Admin User Password**: admin123
- **Jwt Secret**: MySecretKey123456789MySecretKey123456789

## 8. CONTACTO Y SOPORTE

Para cualquier problema con el deployment:
1. Verificar logs de ACR build
2. Revisar firewall rules de SQL Server
3. Confirmar que tu IP está permitida
4. Verificar que todos los recursos están en la misma región

## STATUS GENERAL

- ✅ Base de Datos: **OPERACIONAL**
- ✅ Migraciones: **APLICADAS**
- ✅ Usuario Admin: **CREADO**
- ✅ ACR: **CREADO**
- 🔄 Backend Build: **EN PROGRESO**
- ⏳ Container Instance: **PENDIENTE**
- ⏳ Frontend: **PENDIENTE**

---
*Generado automáticamente el 2025-12-17 06:20 UTC*
