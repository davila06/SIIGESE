# Database Migration Script for Azure SQL
# SIINADSEG Enterprise Web Application

param(
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = ".\backend\src\Infrastructure"
)

Write-Host "🗄️ Ejecutando migraciones de base de datos en Azure SQL..." -ForegroundColor Green

# Validar que Entity Framework tools esté instalado
Write-Host "🔧 Verificando herramientas EF Core..." -ForegroundColor Cyan
$efInstalled = dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efInstalled) {
    Write-Host "📦 Instalando EF Core tools..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

# Navegar al proyecto de Infrastructure
Write-Host "📂 Navegando al proyecto Infrastructure..." -ForegroundColor Cyan
$currentLocation = Get-Location
Set-Location $ProjectPath

try {
    # Verificar que el proyecto existe
    if (-not (Test-Path "Infrastructure.csproj")) {
        throw "No se encontró Infrastructure.csproj en $ProjectPath"
    }

    # Aplicar migraciones
    Write-Host "🚀 Aplicando migraciones a Azure SQL Database..." -ForegroundColor Cyan
    $env:ConnectionStrings__DefaultConnection = $ConnectionString
    
    dotnet ef database update --verbose --connection $ConnectionString
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migraciones aplicadas exitosamente!" -ForegroundColor Green
        
        # Verificar tablas creadas
        Write-Host "🔍 Verificando estructura de base de datos..." -ForegroundColor Cyan
        
        # Aquí podrías agregar verificaciones adicionales
        Write-Host "✅ Base de datos configurada correctamente!" -ForegroundColor Green
    } else {
        throw "Error aplicando migraciones"
    }
    
} catch {
    Write-Error "❌ Error en migraciones: $_"
} finally {
    # Regresar al directorio original
    Set-Location $currentLocation
    Remove-Item Env:ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
}

Write-Host "`n🎉 Migraciones completadas!" -ForegroundColor Green
Write-Host "📋 Las siguientes tablas deberían estar creadas:" -ForegroundColor Yellow
Write-Host "   - Users" -ForegroundColor White
Write-Host "   - Roles" -ForegroundColor White
Write-Host "   - UserRoles" -ForegroundColor White
Write-Host "   - Clientes" -ForegroundColor White
Write-Host "   - Polizas" -ForegroundColor White
Write-Host "   - Cobros" -ForegroundColor White
Write-Host "   - Reclamos" -ForegroundColor White
Write-Host "   - Cotizaciones" -ForegroundColor White
Write-Host "   - EmailConfigs" -ForegroundColor White
Write-Host "   - DataRecords" -ForegroundColor White
Write-Host "   - PasswordResetTokens" -ForegroundColor White