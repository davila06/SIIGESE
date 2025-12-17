# Script para crear Container App con variables de entorno correctamente configuradas

Write-Host "Creando Container App con HTTPS..." -ForegroundColor Green

$acrPassword = az acr credential show --name acrsiinadseg7512 --query "passwords[0].value" -o tsv

az containerapp create `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --environment cae-siinadseg-prod `
  --image acrsiinadseg7512.azurecr.io/siinadseg-backend:latest `
  --target-port 8080 `
  --ingress external `
  --registry-server acrsiinadseg7512.azurecr.io `
  --registry-username acrsiinadseg7512 `
  --registry-password $acrPassword `
  --cpu 1 --memory 2.0Gi `
  --env-vars "ConnectionStrings__DefaultConnection=secretref:conn-string" "ASPNETCORE_ENVIRONMENT=Production" "ASPNETCORE_URLS=http://+:8080" `
  --secrets "conn-string=Server=siinadseg-sql-3376.database.windows.net;Database=SiinadsegDB;User Id=adminuser;Password=P@ssw0rd123!;TrustServerCertificate=True;"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n=== Container App creado exitosamente ===" -ForegroundColor Green
    $fqdn = az containerapp show `
      --name siinadseg-backend-app `
      --resource-group rg-siinadseg-prod-2025 `
      --query "properties.configuration.ingress.fqdn" -o tsv
    
    Write-Host "URL HTTPS: https://$fqdn" -ForegroundColor Cyan
} else {
    Write-Host "Error creando Container App" -ForegroundColor Red
}
