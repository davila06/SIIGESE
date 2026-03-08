# Script rápido para deploy del backend
# Actualiza solo los archivos compilados sin rebuild de Docker

Write-Host "=== DEPLOY RÁPIDO BACKEND ===" -ForegroundColor Green
Write-Host ""

$ResourceGroup = "rg-siinadseg-prod-2025"
$ContainerAppName = "siinadseg-backend"

# Verificar que esté logueado
Write-Host "1. Verificando sesión de Azure..." -ForegroundColor Cyan
$account = az account show 2>$null
if (-not $account) {
    Write-Host "No estás logueado en Azure. Iniciando sesión..." -ForegroundColor Yellow
    az login
}

Write-Host "Sesion activa" -ForegroundColor Green
Write-Host ""

# Listar container apps
Write-Host "2. Buscando Container App..." -ForegroundColor Cyan
$apps = az containerapp list --resource-group $ResourceGroup --query "[].name" -o tsv

if ($apps -match $ContainerAppName) {
    Write-Host "Container App encontrado: $ContainerAppName" -ForegroundColor Green
} else {
    Write-Host "Container Apps disponibles:" -ForegroundColor Yellow
    az containerapp list --resource-group $ResourceGroup --query "[].{Name:name}" -o table
    $ContainerAppName = Read-Host "Ingresa el nombre del Container App"
}

Write-Host ""

# Reiniciar container app para aplicar cambios
Write-Host "3. Reiniciando Container App..." -ForegroundColor Cyan
Write-Host "Esto reiniciará el servicio con la última configuración" -ForegroundColor Yellow

az containerapp revision restart `
    --name $ContainerAppName `
    --resource-group $ResourceGroup `
    --revision-name (az containerapp revision list --name $ContainerAppName --resource-group $ResourceGroup --query "[0].name" -o tsv)

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Container App reiniciado exitosamente" -ForegroundColor Green
    Write-Host ""
    Write-Host "NOTA: Si realizaste cambios en el codigo, necesitas:" -ForegroundColor Yellow
    Write-Host "  1. Reconstruir la imagen Docker" -ForegroundColor White
    Write-Host "  2. Pushear al registry" -ForegroundColor White
    Write-Host "  3. Actualizar el container app con la nueva imagen" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "Error al reiniciar Container App" -ForegroundColor Red
}
