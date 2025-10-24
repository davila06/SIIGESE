# Complete Azure Deployment Script
# SIINADSEG Enterprise Web Application
# This script orchestrates the entire deployment process

param(
    [Parameter(Mandatory=$false)]
    [string]$Location = "East US",
    [Parameter(Mandatory=$false)]
    [switch]$SkipResourceCreation = $false,
    [Parameter(Mandatory=$false)]
    [switch]$OnlyInfrastructure = $false
)

Write-Host "🚀 SIINADSEG - Deployment Completo a Azure" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📅 Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Yellow
Write-Host "📍 Ubicación: $Location" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan

# Variables globales
$resourceGroupName = "rg-siinadseg-prod"
$deploymentInfoFile = "azure-deployment-info.txt"

# Función para leer información de deployment previo
function Get-DeploymentInfo {
    if (Test-Path $deploymentInfoFile) {
        $content = Get-Content $deploymentInfoFile -Raw
        $info = @{}
        
        if ($content -match "Backend App: https://([^\.]+)\.azurewebsites\.net") {
            $info.BackendAppName = $matches[1]
        }
        if ($content -match "SQL Server: ([^\.]+)\.database\.windows\.net") {
            $info.SqlServerName = $matches[1]
        }
        if ($content -match "Base de Datos: (.+)") {
            $info.DatabaseName = $matches[1].Trim()
        }
        if ($content -match "Resource Group: (.+)") {
            $info.ResourceGroupName = $matches[1].Trim()
        }
        if ($content -match "Cadena de Conexión\s+(.+)") {
            $info.ConnectionString = $matches[1].Trim()
        }
        
        return $info
    }
    return $null
}

try {
    # PASO 1: Crear recursos de Azure (si no se especifica omitir)
    if (-not $SkipResourceCreation) {
        Write-Host "`n🏗️ PASO 1: Creando recursos de Azure..." -ForegroundColor Magenta
        Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray
        
        & ".\create-azure-resources.ps1"
        if ($LASTEXITCODE -ne 0) {
            throw "Error creando recursos de Azure"
        }
        Write-Host "✅ Recursos de Azure creados exitosamente" -ForegroundColor Green
    } else {
        Write-Host "`n⏭️ PASO 1: Omitiendo creación de recursos..." -ForegroundColor Yellow
    }

    # Leer información de deployment
    $deployInfo = Get-DeploymentInfo
    if (-not $deployInfo) {
        throw "No se pudo leer la información de deployment. Ejecuta primero la creación de recursos."
    }

    if ($OnlyInfrastructure) {
        Write-Host "`n✅ Solo infraestructura solicitada. Deployment completado." -ForegroundColor Green
        exit 0
    }

    # PASO 2: Ejecutar migraciones de base de datos
    Write-Host "`n🗄️ PASO 2: Ejecutando migraciones de base de datos..." -ForegroundColor Magenta
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray
    
    if ($deployInfo.ConnectionString) {
        & ".\migrate-azure-database.ps1" -ConnectionString $deployInfo.ConnectionString
        if ($LASTEXITCODE -ne 0) {
            Write-Host "⚠️ Error en migraciones, pero continuando..." -ForegroundColor Yellow
        } else {
            Write-Host "✅ Migraciones ejecutadas exitosamente" -ForegroundColor Green
        }
    } else {
        Write-Host "⚠️ No se encontró cadena de conexión, omitiendo migraciones" -ForegroundColor Yellow
    }

    # PASO 3: Desplegar Backend
    Write-Host "`n🖥️ PASO 3: Desplegando Backend..." -ForegroundColor Magenta
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray
    
    if ($deployInfo.BackendAppName -and $deployInfo.ResourceGroupName) {
        & ".\deploy-backend.ps1" -AppName $deployInfo.BackendAppName -ResourceGroup $deployInfo.ResourceGroupName
        if ($LASTEXITCODE -ne 0) {
            throw "Error desplegando backend"
        }
        Write-Host "✅ Backend desplegado exitosamente" -ForegroundColor Green
        $backendUrl = "https://$($deployInfo.BackendAppName).azurewebsites.net"
    } else {
        throw "No se encontró información del backend app"
    }

    # PASO 4: Construir Frontend
    Write-Host "`n🌐 PASO 4: Construyendo Frontend..." -ForegroundColor Magenta
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray
    
    & ".\build-frontend.ps1" -BackendUrl $backendUrl
    if ($LASTEXITCODE -ne 0) {
        throw "Error construyendo frontend"
    }
    Write-Host "✅ Frontend construido exitosamente" -ForegroundColor Green

    # PASO 5: Mostrar resumen final
    Write-Host "`n🎉 DEPLOYMENT COMPLETADO EXITOSAMENTE!" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "📋 RESUMEN FINAL:" -ForegroundColor White
    Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "🗂️  Resource Group: $($deployInfo.ResourceGroupName)" -ForegroundColor Yellow
    Write-Host "🗄️  SQL Database: $($deployInfo.SqlServerName).database.windows.net" -ForegroundColor Yellow
    Write-Host "🖥️  Backend URL: $backendUrl" -ForegroundColor Yellow
    Write-Host "🌐 Frontend: Listo para deployment a Static Web App" -ForegroundColor Yellow
    Write-Host "📦 Frontend ZIP: frontend-deployment.zip" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan

    # PASO 6: Próximos pasos manuales
    Write-Host "`n📋 PRÓXIMOS PASOS MANUALES:" -ForegroundColor Magenta
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray
    Write-Host "1️⃣ Crear Static Web App en Azure Portal:" -ForegroundColor White
    Write-Host "   • Ve a Azure Portal (portal.azure.com)" -ForegroundColor Gray
    Write-Host "   • Crea nuevo recurso > Static Web Apps" -ForegroundColor Gray
    Write-Host "   • Conecta con tu repositorio GitHub" -ForegroundColor Gray
    Write-Host "   • Configura build preset: Angular" -ForegroundColor Gray
    Write-Host "   • App location: /frontend-new" -ForegroundColor Gray
    Write-Host "   • Output location: dist/frontend-new" -ForegroundColor Gray
    
    Write-Host "`n2. Configurar dominio personalizado (opcional):" -ForegroundColor White
    Write-Host "   * En Static Web App > Custom domains" -ForegroundColor Gray
    Write-Host "   • Agregar tu dominio personalizado" -ForegroundColor Gray
    
    Write-Host "`n3️⃣ Configurar CI/CD automático:" -ForegroundColor White
    Write-Host "   • GitHub Actions se configura automáticamente" -ForegroundColor Gray
    Write-Host "   • Cada push a main triggereará deployment" -ForegroundColor Gray
    
    Write-Host "`n4️⃣ Monitoreo y logging:" -ForegroundColor White
    Write-Host "   • Application Insights está configurado" -ForegroundColor Gray
    Write-Host "   • Revisar logs en Azure Portal" -ForegroundColor Gray

    # Crear resumen de URLs importantes
    Write-Host "`n🔗 URLS IMPORTANTES:" -ForegroundColor Magenta
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Gray
    Write-Host "🌐 Azure Portal: https://portal.azure.com" -ForegroundColor Yellow
    Write-Host "🖥️  Backend API: $backendUrl" -ForegroundColor Yellow
    Write-Host "🔧 Kudu Console: https://$($deployInfo.BackendAppName).scm.azurewebsites.net" -ForegroundColor Yellow
    Write-Host "📊 Resource Group: https://portal.azure.com/#@/resource/subscriptions/{subscription}/resourceGroups/$($deployInfo.ResourceGroupName)" -ForegroundColor Yellow

    Write-Host "`n✅ ¡Deployment completado! Tu aplicación está lista para usar." -ForegroundColor Green

} catch {
    Write-Host "`n❌ ERROR EN DEPLOYMENT:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
} catch {
    Write-Host "`n❌ ERROR EN DEPLOYMENT:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor White
    Write-Host "`n🔧 TROUBLESHOOTING:" -ForegroundColor Yellow
    Write-Host "• Verifica que tienes permisos en tu suscripción de Azure" -ForegroundColor White
    Write-Host "• Asegúrate de estar logueado: az login" -ForegroundColor White
    Write-Host "• Revisa los logs detallados arriba" -ForegroundColor White
    Write-Host "• Ejecuta pasos individuales para debugging" -ForegroundColor White
    exit 1
}

Write-Host "`n🎯 DEPLOYMENT COMPLETADO - $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Green