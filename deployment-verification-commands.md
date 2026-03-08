# 🔍 Comandos de Verificación Post-Deployment

## Una vez completado el deployment, ejecuta estos comandos en Cloud Shell:

### 1. Obtener la URL del backend
```bash
BACKEND_URL=$(az containerapp show --name app-siinadseg-backend --resource-group rg-siinadseg --query "properties.configuration.ingress.fqdn" --output tsv)
echo "Backend URL: https://$BACKEND_URL"
```

### 2. Probar Health Check
```bash
curl -s https://$BACKEND_URL/health
```

### 3. Probar API Base
```bash
curl -s https://$BACKEND_URL/api
```

### 4. Ver el estado del Container App
```bash
az containerapp show --name app-siinadseg-backend --resource-group rg-siinadseg --query "properties.runningStatus" --output tsv
```

### 5. Ver logs si hay problemas
```bash
az containerapp logs show --name app-siinadseg-backend --resource-group rg-siinadseg --tail 20
```

## 🎯 URLs finales esperadas:
- **Backend**: https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io
- **Health**: https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io/health
- **Swagger**: https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io/swagger

## ⚡ Si todo funciona correctamente:
1. Tu backend estará desplegado con el código más reciente
2. Todas las APIs estarán disponibles
3. El frontend podrá conectarse sin problemas