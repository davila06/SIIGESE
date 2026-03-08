# Fix: Eliminación de Usuarios

## Problema Identificado
Los usuarios eliminados aparecen en la lista porque el sistema usa "soft delete" (marca `IsDeleted = true`) pero el método `GetAllUsersAsync()` no filtraba usuarios eliminados.

## Solución Implementada

### Backend (C#)
**Archivo**: `backend/src/Application/Services/UserService.cs`

#### Cambios realizados:

1. **GetAllUsersAsync()** - Filtra usuarios eliminados:
```csharp
public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
{
    var users = await _userRepository.GetAllAsync();
    // Filtrar usuarios eliminados
    return users.Where(u => !u.IsDeleted).Select(MapToDto);
}
```

2. **GetUserByIdAsync()** - No retorna usuarios eliminados:
```csharp
public async Task<UserDto?> GetUserByIdAsync(int id)
{
    var user = await _userRepository.GetByIdAsync(id);
    // No retornar usuarios eliminados
    return user != null && !user.IsDeleted ? MapToDto(user) : null;
}
```

3. **GetByEmailAsync()** - No retorna usuarios eliminados:
```csharp
public async Task<UserDto?> GetByEmailAsync(string email)
{
    var user = await _userRepository.GetByEmailAsync(email);
    // No retornar usuarios eliminados
    return user != null && !u.IsDeleted ? MapToDto(user) : null;
}
```

## Estado del Deploy

### ✅ Cambios Guardados
- Código actualizado en `UserService.cs`
- Commit creado: `103ab87`
- Push a GitHub completado

### ⏳ Pendiente de Deploy
El backend en Azure Container Apps necesita ser actualizado con la nueva imagen.

**Opciones para desplegar:**

#### Opción 1: Rebuild automático (GitHub Actions)
Si tienes un workflow configurado, el push activará el rebuild automático.

#### Opción 2: Deploy manual
```powershell
# 1. Navegar al backend
cd backend/src/WebApi

# 2. Build de la imagen Docker
docker build -t siinadseg-backend:latest .

# 3. Tag para Azure Container Registry
docker tag siinadseg-backend:latest [YOUR_ACR].azurecr.io/siinadseg-backend:latest

# 4. Push al registry
docker push [YOUR_ACR].azurecr.io/siinadseg-backend:latest

# 5. Actualizar Container App
az containerapp update `
  --name app-siinadseg-backend `
  --resource-group rg-siinadseg `
  --image [YOUR_ACR].azurecr.io/siinadseg-backend:latest
```

#### Opción 3: Reinicio rápido (sin cambios de código)
Si el código ya está en la imagen actual:
```powershell
az containerapp revision restart `
  --name app-siinadseg-backend `
  --resource-group rg-siinadseg
```

## Verificación

Después del deploy, verifica que funciona:

```powershell
# Test endpoint de usuarios
curl https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io/api/users
```

Los usuarios con `IsDeleted = true` NO deberían aparecer en la respuesta.

## Resumen

- **Problema**: Usuarios eliminados seguían apareciendo
- **Causa**: Sin filtro de `IsDeleted` en queries
- **Solución**: Agregar `.Where(u => !u.IsDeleted)` en métodos GET
- **Estado**: ✅ Código actualizado | ⏳ Pendiente deploy a Container App
