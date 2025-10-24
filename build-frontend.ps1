# Frontend Build and Deployment Preparation Script
# SIINADSEG Enterprise Web Application

param(
    [Parameter(Mandatory=$true)]
    [string]$BackendUrl,
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = ".\frontend-new",
    [Parameter(Mandatory=$false)]
    [string]$Environment = "production"
)

Write-Host "🌐 Preparando Frontend para deployment..." -ForegroundColor Green
Write-Host "🔗 Backend URL: $BackendUrl" -ForegroundColor Yellow
Write-Host "🏗️ Environment: $Environment" -ForegroundColor Yellow

$currentLocation = Get-Location

try {
    # Navegar al proyecto frontend
    Write-Host "`n📂 Navegando al proyecto frontend..." -ForegroundColor Cyan
    Set-Location $ProjectPath

    # Verificar que el proyecto existe
    if (-not (Test-Path "package.json")) {
        throw "No se encontró package.json en $ProjectPath"
    }

    # Verificar que Node.js está instalado
    Write-Host "🔧 Verificando Node.js..." -ForegroundColor Cyan
    $nodeVersion = node --version 2>$null
    if (-not $nodeVersion) {
        throw "Node.js no está instalado. Instálalo desde https://nodejs.org/"
    }
    Write-Host "✅ Node.js version: $nodeVersion" -ForegroundColor Green

    # Verificar Angular CLI
    Write-Host "🔧 Verificando Angular CLI..." -ForegroundColor Cyan
    $ngVersion = ng version --version 2>$null
    if (-not $ngVersion) {
        Write-Host "📦 Instalando Angular CLI..." -ForegroundColor Yellow
        npm install -g @angular/cli
    }

    # Actualizar archivo de environment para producción
    Write-Host "⚙️ Actualizando configuración de environment..." -ForegroundColor Cyan
    $environmentFile = "src\environments\environment.prod.ts"
    
    if (Test-Path $environmentFile) {
        $environmentContent = @"
export const environment = {
  production: true,
  apiUrl: '$BackendUrl/api',
  version: '1.0.0',
  appName: 'SIINADSEG',
  features: {
    enableLogging: false,
    enableDebug: false
  }
};
"@
        $environmentContent | Out-File -FilePath $environmentFile -Encoding UTF8
        Write-Host "✅ Environment actualizado: $environmentFile" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Archivo environment.prod.ts no encontrado, creándolo..." -ForegroundColor Yellow
        New-Item -Path "src\environments" -ItemType Directory -Force
        $environmentContent | Out-File -FilePath $environmentFile -Encoding UTF8
    }

    # Instalar dependencias
    Write-Host "📦 Instalando dependencias npm..." -ForegroundColor Cyan
    npm ci --legacy-peer-deps

    if ($LASTEXITCODE -ne 0) {
        throw "Error instalando dependencias npm"
    }

    # Ejecutar linting
    Write-Host "🔍 Ejecutando linting..." -ForegroundColor Cyan
    npm run lint --if-present
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️ Linting encontró advertencias, continuando..." -ForegroundColor Yellow
    }

    # Compilar para producción
    Write-Host "🏗️ Compilando aplicación para producción..." -ForegroundColor Cyan
    ng build --configuration=production --output-hashing=all --source-map=false

    if ($LASTEXITCODE -ne 0) {
        throw "Error en compilación de Angular"
    }

    # Verificar que el build se generó
    $distPath = "dist\frontend-new"
    if (-not (Test-Path $distPath)) {
        throw "Directorio dist no generado correctamente"
    }

    # Crear archivo de configuración para Static Web App
    Write-Host "📝 Creando configuración para Static Web App..." -ForegroundColor Cyan
    $staticWebAppConfig = @"
{
  "routes": [
    {
      "route": "/api/*",
      "redirect": "$BackendUrl/api/"
    },
    {
      "route": "/*",
      "serve": "/index.html",
      "statusCode": 200
    }
  ],
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/images/*.{png,jpg,gif,ico,svg}", "/css/*", "/js/*"]
  },
  "mimeTypes": {
    ".json": "text/json"
  },
  "defaultHeaders": {
    "Content-Security-Policy": "default-src 'self' $BackendUrl; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;"
  }
}
"@
    $staticWebAppConfig | Out-File -FilePath "$distPath\staticwebapp.config.json" -Encoding UTF8

    # Crear archivo web.config para Azure App Service (como alternativa)
    Write-Host "📝 Creando web.config para Azure App Service..." -ForegroundColor Cyan
    $webConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Angular Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".woff" mimeType="application/font-woff" />
      <mimeMap fileExtension=".woff2" mimeType="application/font-woff2" />
    </staticContent>
    <httpProtocol>
      <customHeaders>
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-Frame-Options" value="DENY" />
        <add name="X-XSS-Protection" value="1; mode=block" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
"@
    $webConfig | Out-File -FilePath "$distPath\web.config" -Encoding UTF8

    # Crear archivo ZIP para deployment manual
    Write-Host "🗜️ Creando archivo ZIP para deployment..." -ForegroundColor Cyan
    $zipPath = "frontend-deployment.zip"
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    Compress-Archive -Path "$distPath\*" -DestinationPath $zipPath -Force

    # Mostrar estadísticas del build
    Write-Host "`n📊 Estadísticas del build:" -ForegroundColor Cyan
    $buildFiles = Get-ChildItem -Path $distPath -Recurse -File
    $totalSize = ($buildFiles | Measure-Object -Property Length -Sum).Sum
    $totalSizeMB = [math]::Round($totalSize / 1MB, 2)
    
    Write-Host "📁 Archivos generados: $($buildFiles.Count)" -ForegroundColor White
    Write-Host "📏 Tamaño total: $totalSizeMB MB" -ForegroundColor White
    Write-Host "📂 Ubicación: $distPath" -ForegroundColor White
    Write-Host "📦 ZIP deployment: $zipPath" -ForegroundColor White

} catch {
    Write-Error "❌ Error en build frontend: $_"
    exit 1
} finally {
    Set-Location $currentLocation
}

Write-Host "`n🎉 ¡Build del Frontend completado!" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "📁 Archivos listos en: $ProjectPath\dist\frontend-new" -ForegroundColor Yellow
Write-Host "📦 ZIP para deployment: $ProjectPath\frontend-deployment.zip" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

Write-Host "`n📋 Opciones de deployment:" -ForegroundColor Magenta
Write-Host "1️⃣ Azure Static Web Apps (Recomendado)" -ForegroundColor White
Write-Host "   - Crea desde Azure Portal" -ForegroundColor Gray
Write-Host "   - Conecta con tu repositorio GitHub" -ForegroundColor Gray
Write-Host "   - Deployment automático con CI/CD" -ForegroundColor Gray
Write-Host "`n2️⃣ Azure App Service" -ForegroundColor White
Write-Host "   - Sube el archivo ZIP manualmente" -ForegroundColor Gray
Write-Host "   - Configura como aplicación Node.js" -ForegroundColor Gray
Write-Host "`n3️⃣ Azure Storage Static Website" -ForegroundColor White
Write-Host "   - Copia archivos a Blob Storage" -ForegroundColor Gray
Write-Host "   - Habilita static website hosting" -ForegroundColor Gray

Write-Host "`n🔗 Próximos pasos:" -ForegroundColor Magenta
Write-Host "1️⃣ Crear Static Web App en Azure Portal" -ForegroundColor White
Write-Host "2️⃣ Conectar con repositorio GitHub" -ForegroundColor White
Write-Host "3️⃣ Configurar build automático" -ForegroundColor White
Write-Host "4️⃣ Configurar dominio personalizado (opcional)" -ForegroundColor White