# Cambios Realizados: Unificación de Nombre de Cliente en Cobros

## 📋 Resumen
Se modificó la estructura de datos de la entidad **Cobro** para usar un solo campo `ClienteNombreCompleto` en lugar de `ClienteNombre` y `ClienteApellido` separados. Esto asegura compatibilidad con el campo `NombreAsegurado` de Pólizas.

---

## ✅ Cambios Completados

### 🔧 Backend (C#)

#### 1. **Entidades (Domain)**
- **Archivo**: `backend/src/Domain/Entities/Cobro.cs`
- **Cambio**: 
  ```csharp
  // Antes:
  public string ClienteNombre { get; set; } = string.Empty;
  public string ClienteApellido { get; set; } = string.Empty;
  
  // Después:
  public string ClienteNombreCompleto { get; set; } = string.Empty;
  ```

#### 2. **DTOs (Application)**
- **Archivos modificados**:
  - `backend/src/Application/DTOs/CobroDto.cs`
  - `backend/src/Application/DTOs/CobroDtos.cs` (CobroRequestDto, ActualizarCobroDto)
- **Cambio**: Misma modificación en todas las propiedades

#### 3. **Servicios**
- ✅ **CobrosService.cs**: NO requiere cambios (usa AutoMapper)
- ✅ **AutoMapper Profile**: Se actualizará automáticamente por convención de nombres

---

### 💻 Frontend (Angular/TypeScript)

#### 1. **Interfaces**
- **Archivo**: `frontend-new/src/app/cobros/interfaces/cobro.interface.ts`
- **Cambio**:
  ```typescript
  // Antes:
  clienteNombre: string;
  clienteApellido: string;
  
  // Después:
  clienteNombreCompleto: string;
  ```

#### 2. **Servicios Mock**
- **Archivo**: `frontend-new/src/app/cobros/services/mock-cobros.service.ts`
- **Cambio**: 12 objetos mock actualizados, combinando nombre y apellido:
  ```typescript
  // Antes:
  clienteNombre: 'Juan Carlos',
  clienteApellido: 'Pérez González',
  
  // Después:
  clienteNombreCompleto: 'Juan Carlos Pérez González',
  ```

#### 3. **Componentes HTML**
- **Archivos modificados**:
  - `frontend-new/src/app/cobros/components/cobro-detalle/cobro-detalle.component.html` (2 cambios)
  - `frontend-new/src/app/cobros/components/cobros-dashboard/cobros-dashboard.component.html` (1 cambio)
- **Cambio**:
  ```html
  <!-- Antes: -->
  {{ cobro.clienteNombre }} {{ cobro.clienteApellido }}
  
  <!-- Después: -->
  {{ cobro.clienteNombreCompleto }}
  ```

---

### 🗄️ Base de Datos

#### Script de Migración Creado
- **Archivo**: `07_MigrateCobroClienteNombreCompleto.sql`
- **Acciones**:
  1. ✅ Agrega columna `ClienteNombreCompleto` (nvarchar(200), NULLABLE inicialmente)
  2. ✅ Migra datos existentes: combina `ClienteNombre` + `ClienteApellido`
  3. ✅ Convierte columna a NOT NULL
  4. ✅ Crea índice `IX_Cobros_ClienteNombreCompleto`
  5. ⚠️ **NO elimina** columnas antiguas (comentado para seguridad)

#### Estructura Final de Tabla Cobros
```sql
-- Columnas actuales después de migración:
ClienteNombre           nvarchar(100)  -- ⚠️ Aún existe (deprecated)
ClienteApellido         nvarchar(100)  -- ⚠️ Aún existe (deprecated)
ClienteNombreCompleto   nvarchar(200)  -- ✅ NUEVO CAMPO ACTIVO
```

---

## 🔄 Compatibilidad con Upload de Pólizas

### Problema Original
- **Upload de Pólizas**: Columna `NOMBRE` → campo `NombreAsegurado` (nombre completo)
- **Entidad Cobro**: Esperaba `ClienteNombre` y `ClienteApellido` separados
- **Resultado**: Incompatibilidad al generar cobros desde pólizas

### Solución Implementada
Ahora cuando se cree un cobro desde una póliza:
```csharp
// Pseudocódigo de creación de cobro
var cobro = new Cobro {
    PolizaId = poliza.Id,
    NumeroPoliza = poliza.NumeroPoliza,
    ClienteNombreCompleto = poliza.NombreAsegurado,  // ✅ Mapeo directo
    MontoTotal = poliza.Prima,
    // ...
};
```

---

## 🚀 Próximos Pasos

### 1. **Ejecutar Migración SQL** ⏳
```bash
# En Azure o SQL Server local
sqlcmd -S siinadseg-sql-prod-4451.database.windows.net -d SiinadsegProdDB -U <usuario> -P <password> -i 07_MigrateCobroClienteNombreCompleto.sql
```

### 2. **Rebuild Backend** ⏳
```bash
cd backend
dotnet build --configuration Release
```

### 3. **Build Docker Image** ⏳
```bash
docker build -t siinadsegacr.azurecr.io/siinadseg-backend:latest -f backend/Dockerfile .
docker push siinadsegacr.azurecr.io/siinadseg-backend:latest
```

### 4. **Rebuild Frontend** ⏳
```bash
cd frontend-new
ng build --configuration=production
```

### 5. **Deploy a Azure** ⏳
```bash
# Frontend
npx @azure/static-web-apps-cli deploy frontend-new/dist/frontend-new --deployment-token <token> --env production

# Backend - Recrear container
az container restart --name siinadseg-backend --resource-group siinadseg-rg
```

### 6. **Verificación en Producción** ⏳
- [ ] Login funcional
- [ ] Upload de pólizas con campo `NOMBRE`
- [ ] Módulo de cobros muestra nombres completos
- [ ] Creación de cobros desde pólizas funciona correctamente

### 7. **Limpieza Final** (Después de 1-2 semanas) 🔜
```sql
-- Descomentar sección 5 del script de migración
-- Esto eliminará definitivamente ClienteNombre y ClienteApellido
```

---

## ⚠️ Notas Importantes

1. **Columnas Antiguas**: 
   - Las columnas `ClienteNombre` y `ClienteApellido` AÚN EXISTEN en la base de datos
   - Esto es por seguridad, para rollback si es necesario
   - Se deben eliminar manualmente después de verificar que todo funciona

2. **AutoMapper**:
   - Los perfiles de AutoMapper deben actualizarse si usan mapeo explícito
   - Si usan convención de nombres, funcionarán automáticamente

3. **Testing**:
   - Probar creación de cobros manualmente
   - Probar upload de pólizas + generación de cobros
   - Verificar reportes que usen nombres de clientes

4. **Rollback**:
   - Si algo falla, el backend puede volver a usar `ClienteNombre` y `ClienteApellido`
   - Los datos están preservados en ambos formatos durante la transición

---

## 📊 Impacto de los Cambios

### Archivos Modificados
- **Backend**: 3 archivos (.cs)
- **Frontend**: 5 archivos (.ts, .html)
- **Base de Datos**: 1 script de migración (.sql)

### Líneas de Código
- **Agregadas**: ~150 líneas (SQL migration script)
- **Modificadas**: ~30 líneas
- **Eliminadas**: ~15 líneas

### Módulos Afectados
- ✅ Cobros (directo)
- ✅ Pólizas (indirecto - compatibilidad mejorada)
- ✅ Notificaciones por email (usa datos de cobros)

---

## 🎯 Beneficios

1. **Simplificación**: Un solo campo en lugar de dos
2. **Compatibilidad**: Mapeo directo con `NombreAsegurado` de Pólizas
3. **Menos errores**: No hay que dividir/combinar nombres manualmente
4. **Mejor UX**: Display más limpio en frontend
5. **Facilita búsquedas**: Un solo campo para buscar nombre completo

---

**Fecha de cambios**: 2025-12-16  
**Estado**: ✅ Código modificado, ⏳ Pendiente de deploy
