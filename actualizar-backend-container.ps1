# Script para actualizar el backend en Azure Container Instance
# Fecha: 15 de diciembre, 2025

Write-Host "=== ACTUALIZAR BACKEND EN AZURE CONTAINER ===" -ForegroundColor Green

$containerName = "siinadseg-backend-1019"
$resourceGroup = "siinadseg-rg"
$backendPath = "C:\Users\davil\SINSEG\enterprise-web-app\backend\src\WebApi"

# 1. Compilar proyecto
Write-Host "`n1. Compilando proyecto..." -ForegroundColor Cyan
Set-Location $backendPath
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo la compilacion" -ForegroundColor Red
    exit 1
}

Write-Host "Compilacion exitosa" -ForegroundColor Green

# 2. Verificar si el container existe
Write-Host "`n2. Verificando container en Azure..." -ForegroundColor Cyan
$containerExists = az container show --name $containerName --resource-group $resourceGroup 2>$null

if (-not $containerExists) {
    Write-Host "WARNING: Container '$containerName' no existe en el resource group '$resourceGroup'" -ForegroundColor Yellow
    Write-Host "Listando containers disponibles..." -ForegroundColor Yellow
    az container list --resource-group $resourceGroup --query "[].{Name:name, State:instanceView.state}" --output table
    exit 1
}

# 3. Reiniciar el container
Write-Host "`n3. Reiniciando container para aplicar cambios..." -ForegroundColor Cyan
Write-Host "NOTA: Los cambios en el codigo requieren rebuild de la imagen Docker" -ForegroundColor Yellow
Write-Host "Este script solo reinicia el container existente." -ForegroundColor Yellow

az container restart --name $containerName --resource-group $resourceGroup

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n=== ACTUALIZACION COMPLETADA ===" -ForegroundColor Green
    Write-Host "El container se esta reiniciando..." -ForegroundColor Cyan
    Write-Host "Los cambios NO se aplicaran hasta que reconstruyas la imagen Docker" -ForegroundColor Yellow
    Write-Host "`nPara aplicar cambios de codigo, necesitas:" -ForegroundColor White
    Write-Host "1. Reconstruir imagen Docker" -ForegroundColor Gray
    Write-Host "2. Pushear al Azure Container Registry" -ForegroundColor Gray
    Write-Host "3. Recrear el container con la nueva imagen" -ForegroundColor Gray
} else {
    Write-Host "`nERROR: Fallo al reiniciar el container" -ForegroundColor Red
    exit 1
}
