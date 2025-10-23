# Azure Static Web Apps Deployment Script
# Este script crea todos los recursos necesarios para SINSEG

param(
    [string]$ResourceGroup = "siinadseg-rg",
    [string]$Location = "eastus",
    [string]$AppName = "siinadseg-webapp",
    [string]$SqlServer = "siinadseg-sql-5307",
    [string]$SqlDatabase = "SiinadsegDB"
)

Write-Host "🚀 Iniciando creación de recursos Azure para SINSEG..." -ForegroundColor Green

# 1. Crear Azure Container Registry
Write-Host "📦 Creando Azure Container Registry..." -ForegroundColor Yellow
try {
    az acr create `
        --resource-group $ResourceGroup `
        --name "siinadsegacr" `
        --sku Basic `
        --admin-enabled true
    Write-Host "✅ Container Registry creado" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Container Registry ya existe o error: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 2. Construir y subir imagen Docker
Write-Host "🔨 Construyendo imagen Docker..." -ForegroundColor Yellow
Push-Location "backend"
try {
    # Build de la imagen
    docker build -t siinadseg-backend:latest .
    
    # Tag para ACR
    docker tag siinadseg-backend:latest siinadsegacr.azurecr.io/siinadseg-backend:latest
    
    # Login a ACR
    az acr login --name siinadsegacr
    
    # Push imagen
    docker push siinadsegacr.azurecr.io/siinadseg-backend:latest
    
    Write-Host "✅ Imagen Docker subida a ACR" -ForegroundColor Green
} catch {
    Write-Host "❌ Error construyendo/subiendo imagen: $($_.Exception.Message)" -ForegroundColor Red
}
Pop-Location

# 3. Crear Azure Container Instance
Write-Host "🚀 Creando Azure Container Instance..." -ForegroundColor Yellow
try {
    $acrPassword = az acr credential show --name siinadsegacr --query "passwords[0].value" --output tsv
    
    az container create `
        --resource-group $ResourceGroup `
        --name "siinadseg-backend" `
        --image "siinadsegacr.azurecr.io/siinadseg-backend:latest" `
        --cpu 1 `
        --memory 1 `
        --registry-login-server "siinadsegacr.azurecr.io" `
        --registry-username "siinadsegacr" `
        --registry-password $acrPassword `
        --dns-name-label "siinadseg-backend" `
        --ports 80 `
        --environment-variables `
            "ASPNETCORE_ENVIRONMENT=Production" `
            "ConnectionStrings__DefaultConnection=Server=tcp:siinadseg-sql-5307.database.windows.net,1433;Database=SiinadsegDB;User ID=siinadmin;Password=SiinAdmin123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    
    Write-Host "✅ Container Instance creado" -ForegroundColor Green
} catch {
    Write-Host "❌ Error creando Container Instance: $($_.Exception.Message)" -ForegroundColor Red
}

# 4. Obtener URL del backend
Write-Host "🔗 Obteniendo URL del backend..." -ForegroundColor Yellow
$backendUrl = az container show --resource-group $ResourceGroup --name "siinadseg-backend" --query "ipAddress.fqdn" --output tsv
Write-Host "🌐 Backend URL: https://$backendUrl" -ForegroundColor Cyan

# 5. Actualizar configuración del frontend
Write-Host "⚙️ Actualizando configuración del frontend..." -ForegroundColor Yellow
$envContent = @"
export const environment = {
  production: true,
  apiUrl: 'https://$backendUrl/api',
  version: '$(Get-Date -Format "yyyyMMdd-HHmm")-azure',
  enableLogging: false
};
"@

$envContent | Out-File -FilePath "frontend-new\src\environments\environment.prod.ts" -Encoding UTF8
Write-Host "✅ Configuración del frontend actualizada" -ForegroundColor Green

# 6. Build del frontend
Write-Host "🔨 Construyendo frontend..." -ForegroundColor Yellow
Push-Location "frontend-new"
try {
    npm run build --configuration=production
    Write-Host "✅ Frontend construido" -ForegroundColor Green
} catch {
    Write-Host "❌ Error construyendo frontend: $($_.Exception.Message)" -ForegroundColor Red
}
Pop-Location

# 7. Desplegar a Static Web Apps
Write-Host "🚀 Desplegando a Azure Static Web Apps..." -ForegroundColor Yellow
try {
    npx @azure/static-web-apps-cli deploy `
        "frontend-new/dist/frontend-new" `
        --deployment-token "ba9c8c8e6f2d1c04b52c77e1b5e47b7e2b0c5a8d9e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b"
    
    Write-Host "✅ Frontend desplegado a Static Web Apps" -ForegroundColor Green
} catch {
    Write-Host "❌ Error desplegando frontend: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 ¡Deployment completado!" -ForegroundColor Green
Write-Host "🌐 Frontend: https://agreeable-water-06170cf10.1.azurestaticapps.net/" -ForegroundColor Cyan
Write-Host "🔗 Backend: https://$backendUrl" -ForegroundColor Cyan
Write-Host "📊 Base de datos: $SqlServer.database.windows.net/$SqlDatabase" -ForegroundColor Cyan