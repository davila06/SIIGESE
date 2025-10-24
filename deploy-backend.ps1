# Backend Deployment Script for Azure App Service
# SIINADSEG Enterprise Web Application

param(
    [Parameter(Mandatory=$true)]
    [string]$AppName,
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroup,
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = ".\backend\src\WebApi"
)

Write-Host "🚀 Desplegando Backend a Azure App Service..." -ForegroundColor Green
Write-Host "📱 App Name: $AppName" -ForegroundColor Yellow
Write-Host "📦 Resource Group: $ResourceGroup" -ForegroundColor Yellow

$currentLocation = Get-Location

try {
    # Navegar al proyecto WebApi
    Write-Host "`n📂 Navegando al proyecto WebApi..." -ForegroundColor Cyan
    Set-Location $ProjectPath

    # Verificar que el proyecto existe
    if (-not (Test-Path "WebApi.csproj")) {
        throw "No se encontró WebApi.csproj en $ProjectPath"
    }

    # Limpiar y restaurar dependencias
    Write-Host "🧹 Limpiando y restaurando dependencias..." -ForegroundColor Cyan
    dotnet clean
    dotnet restore

    if ($LASTEXITCODE -ne 0) {
        throw "Error en dotnet restore"
    }

    # Compilar el proyecto
    Write-Host "🔨 Compilando proyecto para Release..." -ForegroundColor Cyan
    dotnet build --configuration Release --no-restore

    if ($LASTEXITCODE -ne 0) {
        throw "Error en compilación"
    }

    # Publicar el proyecto
    Write-Host "📦 Publicando proyecto..." -ForegroundColor Cyan
    $publishPath = ".\bin\Release\net8.0\publish"
    dotnet publish --configuration Release --output $publishPath --no-build

    if ($LASTEXITCODE -ne 0) {
        throw "Error en publicación"
    }

    # Crear archivo ZIP para deployment
    Write-Host "🗜️ Creando archivo ZIP para deployment..." -ForegroundColor Cyan
    $zipPath = ".\deployment.zip"
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }

    # Comprimir archivos publicados
    Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath -Force

    if (-not (Test-Path $zipPath)) {
        throw "Error creando archivo ZIP"
    }

    # Desplegar a Azure
    Write-Host "☁️ Desplegando a Azure App Service..." -ForegroundColor Cyan
    az webapp deployment source config-zip `
        --resource-group $ResourceGroup `
        --name $AppName `
        --src $zipPath

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Deployment exitoso!" -ForegroundColor Green
        
        # Obtener URL de la aplicación
        $appUrl = az webapp show --resource-group $ResourceGroup --name $AppName --query "defaultHostName" -o tsv
        Write-Host "🌐 URL del Backend: https://$appUrl" -ForegroundColor Yellow
        
        # Probar endpoint de health
        Write-Host "🏥 Probando endpoint de health..." -ForegroundColor Cyan
        Start-Sleep -Seconds 30  # Esperar a que la app se inicie
        
        try {
            $healthResponse = Invoke-WebRequest -Uri "https://$appUrl/health" -Method GET -TimeoutSec 30 -ErrorAction Stop
            Write-Host "✅ Health check exitoso: $($healthResponse.StatusCode)" -ForegroundColor Green
        } catch {
            Write-Host "⚠️ Health check falló (normal si es el primer deployment): $($_.Exception.Message)" -ForegroundColor Yellow
        }
        
        # Probar endpoint de Swagger (si está habilitado)
        try {
            $swaggerResponse = Invoke-WebRequest -Uri "https://$appUrl/swagger" -Method GET -TimeoutSec 30 -ErrorAction Stop
            Write-Host "✅ Swagger disponible en: https://$appUrl/swagger" -ForegroundColor Green
        } catch {
            Write-Host "ℹ️ Swagger no disponible (normal en producción)" -ForegroundColor Blue
        }
        
    } else {
        throw "Error en deployment a Azure"
    }

    # Limpiar archivos temporales
    Write-Host "🧹 Limpiando archivos temporales..." -ForegroundColor Cyan
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    if (Test-Path $publishPath) {
        Remove-Item $publishPath -Recurse -Force
    }

} catch {
    Write-Error "❌ Error en deployment: $_"
    exit 1
} finally {
    Set-Location $currentLocation
}

Write-Host "`n🎉 ¡Deployment del Backend completado!" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "🌐 URL Backend: https://$appUrl" -ForegroundColor Yellow
Write-Host "🔧 Kudu Console: https://$AppName.scm.azurewebsites.net" -ForegroundColor Yellow
Write-Host "📊 Azure Portal: https://portal.azure.com" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

Write-Host "`n📋 Próximos pasos:" -ForegroundColor Magenta
Write-Host "1️⃣ Verificar que la aplicación esté funcionando" -ForegroundColor White
Write-Host "2️⃣ Configurar CI/CD para deployments automáticos" -ForegroundColor White
Write-Host "3️⃣ Configurar logging y monitoring" -ForegroundColor White
Write-Host "4️⃣ Desplegar el frontend" -ForegroundColor White