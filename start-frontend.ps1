# Script para iniciar el frontend Angular
Set-Location "C:\Users\davil\SINSEG\enterprise-web-app\frontend-new"
Write-Host "Directorio actual: $(Get-Location)"
Write-Host "Verificando package.json: $(Test-Path 'package.json')"

if (Test-Path 'package.json') {
    Write-Host "Iniciando servidor Angular con npm start..."
    npm start
} else {
    Write-Host "ERROR: No se encontró package.json en el directorio actual"
    Get-Location
    Get-ChildItem
}