# 🚀 SIINADSEG - Comandos para Azure Cloud Shell Deployment

## Paso 1: Clonar el repositorio
```bash
# Clonar tu repositorio
git clone https://github.com/davila06/SIIGESE.git
cd SIIGESE

# Cambiar a la rama V1
git checkout V1
```

## Paso 2: Navegar al directorio del backend
```bash
cd backend
```

## Paso 3: Deployment directo con Container Apps
```bash
# Deployment usando Azure Container Apps
az containerapp up \
  --name app-siinadseg-backend \
  --resource-group rg-siinadseg \
  --environment env-siinadseg \
  --source . \
  --target-port 80 \
  --ingress external
```

## Paso 4: Verificar el deployment
```bash
# Obtener la URL del backend
az containerapp show \
  --name app-siinadseg-backend \
  --resource-group rg-siinadseg \
  --query "properties.configuration.ingress.fqdn" \
  --output tsv
```

## Paso 5: Probar endpoints
```bash
# Obtener la URL completa
BACKEND_URL=$(az containerapp show --name app-siinadseg-backend --resource-group rg-siinadseg --query "properties.configuration.ingress.fqdn" --output tsv)

# Probar health endpoint
curl https://$BACKEND_URL/health

# Probar swagger (si está habilitado)
curl https://$BACKEND_URL/swagger
```

## 🔧 Si hay errores, comandos de diagnóstico:

### Ver logs del Container App
```bash
az containerapp logs show \
  --name app-siinadseg-backend \
  --resource-group rg-siinadseg \
  --tail 50
```

### Ver estado del Container App
```bash
az containerapp show \
  --name app-siinadseg-backend \
  --resource-group rg-siinadseg \
  --query "properties.runningStatus"
```

### Reiniciar si es necesario
```bash
# Obtener el nombre de la revisión actual
REVISION=$(az containerapp revision list --name app-siinadseg-backend --resource-group rg-siinadseg --query "[0].name" --output tsv)

# Reiniciar la revisión
az containerapp revision restart \
  --name app-siinadseg-backend \
  --resource-group rg-siinadseg \
  --revision $REVISION
```

## 📋 URLs importantes después del deployment:
- **Backend API**: https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io
- **Frontend**: https://gentle-dune-0a2edab0f.3.azurestaticapps.net
- **Health Check**: https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io/health

## ⚠️ Notas importantes:
1. Cloud Shell tiene Docker y todas las herramientas necesarias preinstaladas
2. El comando `az containerapp up` automáticamente detecta el Dockerfile y hace el build
3. Si el deployment falla por ACR Tasks, intenta eliminar el Container App actual primero:
   ```bash
   az containerapp delete --name app-siinadseg-backend --resource-group rg-siinadseg --yes
   ```

## 🎉 Una vez completado:
Tu backend estará desplegado y funcionando. Después puedes actualizar el frontend para que apunte a la nueva URL del backend.