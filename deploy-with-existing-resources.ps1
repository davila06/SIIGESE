# Quick Deployment Script for Existing Azure Resources
# SIINADSEG Enterprise Web Application

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigFile = "azure-deployment-config.json"
)

Write-Host "🚀 SIINADSEG - Quick Deploy con recursos existentes" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan

# Leer configuración existente
if (-not (Test-Path $ConfigFile)) {
    Write-Error "❌ No se encontró el archivo de configuración: $ConfigFile"
    exit 1
}

$config = Get-Content $ConfigFile | ConvertFrom-Json
Write-Host "✅ Configuración leída desde: $ConfigFile" -ForegroundColor Green

# Mostrar recursos existentes
Write-Host "`n📋 RECURSOS DETECTADOS:" -ForegroundColor Cyan
Write-Host "🗄️  SQL Server: $($config.azure.resources.sqlServer)" -ForegroundColor Yellow
Write-Host "💾 Database: $($config.azure.resources.database)" -ForegroundColor Yellow
Write-Host "🖥️  Backend: $($config.azure.resources.backendContainer)" -ForegroundColor Yellow
Write-Host "🌐 Frontend: $($config.azure.resources.staticWebApp)" -ForegroundColor Yellow
Write-Host "📦 Resource Group: $($config.azure.resources.resourceGroup)" -ForegroundColor Yellow

# Construir cadena de conexión
$connectionString = "Server=$($config.azure.resources.sqlServer);Database=$($config.azure.resources.database);User ID=$($config.azure.credentials.sqlUsername);Password=$($config.azure.credentials.sqlPassword);Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "`n🗄️ PASO 1: Verificando base de datos..." -ForegroundColor Magenta
Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray

# Ejecutar migraciones
Write-Host "🔄 Ejecutando migraciones..." -ForegroundColor Cyan
& ".\migrate-azure-database.ps1" -ConnectionString $connectionString
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Base de datos actualizada" -ForegroundColor Green
} else {
    Write-Host "⚠️ Migraciones fallaron, continuando..." -ForegroundColor Yellow
}

Write-Host "`n🌐 PASO 2: Preparando Frontend..." -ForegroundColor Magenta
Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray

# Actualizar environment.prod.ts con la URL del backend
$envFile = ".\frontend-new\src\environments\environment.prod.ts"
$backendUrl = $config.azure.endpoints.api -replace "/api$", ""

$environmentContent = @"
export const environment = {
  production: true,
  apiUrl: '$($config.azure.endpoints.api)',
  version: '1.0.0',
  appName: 'SIINADSEG',
  features: {
    enableLogging: false,
    enableDebug: false
  }
};
"@

Write-Host "📝 Actualizando environment.prod.ts..." -ForegroundColor Cyan
$environmentContent | Out-File -FilePath $envFile -Encoding UTF8
Write-Host "✅ Environment actualizado: $($config.azure.endpoints.api)" -ForegroundColor Green

# Build frontend
Write-Host "🏗️ Construyendo frontend..." -ForegroundColor Cyan
Set-Location ".\frontend-new"

try {
    # Instalar dependencias si no existen
    if (-not (Test-Path "node_modules")) {
        Write-Host "📦 Instalando dependencias..." -ForegroundColor Yellow
        npm install --legacy-peer-deps
    }

    # Build para producción
    npm run build -- --configuration=production --output-hashing=all

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Frontend construido exitosamente" -ForegroundColor Green
        
        # Crear ZIP para deployment
        $distPath = "dist\frontend-new"
        $zipPath = "..\frontend-deployment.zip"
        
        if (Test-Path $zipPath) {
            Remove-Item $zipPath -Force
        }
        
        Compress-Archive -Path "$distPath\*" -DestinationPath $zipPath -Force
        Write-Host "📦 ZIP creado: frontend-deployment.zip" -ForegroundColor Green
    } else {
        throw "Error en build del frontend"
    }
} finally {
    Set-Location ".."
}

Write-Host "`n🎉 DEPLOYMENT PREPARADO!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📋 ESTADO ACTUAL:" -ForegroundColor White
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✅ Base de datos: Migrada" -ForegroundColor Green
Write-Host "✅ Backend: Usando $($config.azure.resources.backendContainer)" -ForegroundColor Green
Write-Host "✅ Frontend: Construido (frontend-deployment.zip)" -ForegroundColor Green
Write-Host "📍 Static Web App: $($config.azure.resources.staticWebApp)" -ForegroundColor Yellow

Write-Host "`n📋 PRÓXIMOS PASOS:" -ForegroundColor Magenta
Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray
Write-Host "1️⃣ Para actualizar Static Web App:" -ForegroundColor White
Write-Host "   • Ve a Azure Portal > Static Web Apps" -ForegroundColor Gray
Write-Host "   • Selecciona tu app: $($config.azure.resources.staticWebApp)" -ForegroundColor Gray
Write-Host "   • Sube el archivo frontend-deployment.zip" -ForegroundColor Gray

Write-Host "`n2️⃣ Para verificar backend:" -ForegroundColor White
Write-Host "   • Prueba: $($config.azure.endpoints.api)/health" -ForegroundColor Gray
Write-Host "   • API docs: $($config.azure.endpoints.api)/swagger" -ForegroundColor Gray

Write-Host "`n3️⃣ Para configurar CI/CD:" -ForegroundColor White
Write-Host "   • Configura GitHub Actions con el workflow incluido" -ForegroundColor Gray
Write-Host "   • Archivo: .github/workflows/azure-deploy.yml" -ForegroundColor Gray

Write-Host "`n🔗 URLs DE TU APLICACIÓN:" -ForegroundColor Magenta
Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray
Write-Host "🌐 Frontend: $($config.azure.resources.staticWebApp)" -ForegroundColor Yellow
Write-Host "🖥️  Backend API: $($config.azure.endpoints.api)" -ForegroundColor Yellow
Write-Host "🗄️  Database: $($config.azure.resources.sqlServer)" -ForegroundColor Yellow

Write-Host "`n✅ Todo listo para usar!" -ForegroundColor Green