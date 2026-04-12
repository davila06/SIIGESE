# Despliegue Completo Azure (Frontend + Backend + Migraciones BD)

## Objetivo
Publicar una nueva versión del frontend en Azure Static Web Apps, desplegar una nueva imagen del backend en Azure Container Apps y aplicar todas las migraciones de base de datos en Azure SQL.

## Recursos objetivo
- Static Web App: `swa-siinadseg-frontend`
- URL frontend: `https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net`
- Container App backend: `siinadseg-backend-app`
- URL backend: `https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io`
- SQL Server: `siinadseg-sql-3376.database.windows.net`
- Base de datos: `SiinadsegDB`
- Resource Group: `rg-siinadseg-prod-2025`
- ACR: `acrsiinadseg7512`

## Prerrequisitos
1. Azure CLI instalado y sesión iniciada.
2. .NET SDK 8 instalado.
3. Node.js/NPM instalados.
4. Permisos en subscription para:
   - `Microsoft.Web/staticSites/*`
   - `Microsoft.App/containerApps/*`
   - `Microsoft.ContainerRegistry/registries/*`
   - `Microsoft.Sql/servers/firewallRules/*`

## 1) Verificar contexto Azure
```powershell
az account show --output table
az staticwebapp list --query "[].{name:name,rg:resourceGroup,hostname:defaultHostname}" --output table
az containerapp list --query "[].{name:name,rg:resourceGroup,fqdn:properties.configuration.ingress.fqdn,image:properties.template.containers[0].image}" -o table
```

## 2) Desplegar frontend (Static Web App)
Desde la raíz del repo:

```powershell
Set-Location .\frontend-new
npm run build

$token = az staticwebapp secrets list `
  --name "swa-siinadseg-frontend" `
  --resource-group "rg-siinadseg-prod-2025" `
  --query "properties.apiKey" -o tsv

npx @azure/static-web-apps-cli@latest deploy ./dist/frontend-new `
  --deployment-token $token `
  --env production
```

Validación:
```powershell
Invoke-WebRequest "https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net" -UseBasicParsing
```

## 3) Habilitar firewall SQL para la IP actual (si aplica)
Si la migración falla por IP no permitida, crear regla temporal:

```powershell
$myIp = (Invoke-RestMethod -Uri "https://api.ipify.org").Trim()
az sql server firewall-rule create `
  --resource-group "rg-siinadseg-prod-2025" `
  --server "siinadseg-sql-3376" `
  --name "AllowCurrentIP-Deploy" `
  --start-ip-address $myIp `
  --end-ip-address $myIp
```

## 4) Aplicar todas las migraciones EF a Azure SQL
Desde `backend/src/Infrastructure`:

```powershell
Set-Location .\backend\src\Infrastructure

# Opción recomendada: reutilizar el secreto de conexión del Container App
$conn = az containerapp secret list `
  --name "siinadseg-backend-app" `
  --resource-group "rg-siinadseg-prod-2025" `
  --show-values `
  --query "[?name=='connection-string'].value | [0]" -o tsv

dotnet ef database update `
  --startup-project ../WebApi/WebApi.csproj `
  --context ApplicationDbContext `
  --connection "$conn"

# Verificar historial en BD
 dotnet ef migrations list `
  --startup-project ../WebApi/WebApi.csproj `
  --context ApplicationDbContext `
  --connection "$conn"
```

Resultado esperado: deben aparecer al final las migraciones más recientes.
Ejemplo:
- `20260411054658_AddEmailLogs`
- `20260411055226_AddReclamoHistorial`

## 5) Desplegar backend nuevo en Container Apps
Desde la raíz del repo:

```powershell
$tag = Get-Date -Format "yyyyMMddHHmmss"
$image = "siinadseg-backend:$tag"

az acr build `
  --registry "acrsiinadseg7512" `
  --image $image `
  --file "backend/Dockerfile" `
  "backend"

$fullImage = "acrsiinadseg7512.azurecr.io/$image"
az containerapp update `
  --name "siinadseg-backend-app" `
  --resource-group "rg-siinadseg-prod-2025" `
  --image $fullImage

az containerapp show `
  --name "siinadseg-backend-app" `
  --resource-group "rg-siinadseg-prod-2025" `
  --query "{latestRevision:properties.latestRevisionName,runningStatus:properties.runningStatus,image:properties.template.containers[0].image,fqdn:properties.configuration.ingress.fqdn}" -o json
```

## 6) Verificación final end-to-end
```powershell
Invoke-WebRequest "https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io/health" -UseBasicParsing
Invoke-WebRequest "https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net" -UseBasicParsing
```

Checklist final:
- Frontend responde `200`.
- Backend `/health` responde `200`.
- Container App en estado `Running`.
- Imagen del backend actualizada al tag nuevo.
- Migraciones EF completas en `__EFMigrationsHistory`.

## Notas operativas
- Si `sqlcmd` falla por TLS local, usar validación con `dotnet ef migrations list --connection ...`.
- Si no se desea regla de firewall permanente, eliminar al final:

```powershell
az sql server firewall-rule delete `
  --resource-group "rg-siinadseg-prod-2025" `
  --server "siinadseg-sql-3376" `
  --name "AllowCurrentIP-Deploy"
```
