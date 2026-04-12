Param(
    [string]$SqlServer = "Karo\SQLEXPRESS",
    [string]$Database = "SinsegAppDb",
    [string]$BackendUrl = "http://localhost:5000",
    [string]$FrontendUrl = "http://localhost:4200",
    [string]$SmokeUser = "admin@sinseg.com",
    [SecureString]$SmokePassword = (ConvertTo-SecureString "password123" -AsPlainText -Force),
    [switch]$SkipSmokeTests,
    [switch]$SkipNpmInstall
)

$ErrorActionPreference = "Stop"

function Write-Info([string]$message) {
    Write-Host "[INFO] $message" -ForegroundColor Cyan
}

function Write-Ok([string]$message) {
    Write-Host "[OK]   $message" -ForegroundColor Green
}

function Write-Warn([string]$message) {
    Write-Host "[WARN] $message" -ForegroundColor Yellow
}

function Stop-ProcessByPort([int]$port) {
    $listeners = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if (-not $listeners) {
        Write-Info "Puerto $port ya está libre."
        return
    }

    $processIds = $listeners | Select-Object -ExpandProperty OwningProcess -Unique
    foreach ($processId in $processIds) {
        try {
            $proc = Get-Process -Id $processId -ErrorAction Stop
            Stop-Process -Id $processId -Force
            Write-Warn "Proceso detenido en puerto ${port}: $($proc.ProcessName) (PID $processId)"
        }
        catch {
            Write-Warn "No se pudo detener PID $processId en puerto $port."
        }
    }
}

function Wait-HttpOk([string]$url, [int]$maxAttempts = 40, [int]$sleepSeconds = 2) {
    for ($i = 1; $i -le $maxAttempts; $i++) {
        try {
            $resp = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 8
            if ($resp.StatusCode -ge 200 -and $resp.StatusCode -lt 300) {
                return $true
            }
        }
        catch {
            # retry
        }

        Write-Info "Esperando respuesta de $url (intento $i/$maxAttempts)..."
        Start-Sleep -Seconds $sleepSeconds
    }

    return $false
}

function Convert-SecureToPlain([SecureString]$secure) {
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    }
    finally {
        if ($ptr -ne [IntPtr]::Zero) {
            [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
        }
    }
}

$root = $PSScriptRoot
$backendPath = Join-Path $root "backend\src\WebApi"
$frontendPath = Join-Path $root "frontend-new"
$chatScript = Join-Path $root "10_CreateChatTables.sql"
$polizasObservacionesScript = Join-Path $root "09_AddObservacionesToPolizas.sql"
$cobroEmailTemplateScript = Join-Path $root "10_AddCobroEmailTemplate.sql"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " SINSEG - Inicio local completo" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

if (-not (Test-Path $backendPath)) { throw "No se encontró backend en $backendPath" }
if (-not (Test-Path $frontendPath)) { throw "No se encontró frontend en $frontendPath" }
if (-not (Test-Path $chatScript)) { throw "No se encontró script SQL de chat en $chatScript" }
if (-not (Test-Path $polizasObservacionesScript)) { throw "No se encontró script SQL de observaciones en $polizasObservacionesScript" }
if (-not (Test-Path $cobroEmailTemplateScript)) { throw "No se encontró script SQL de plantilla de cobros en $cobroEmailTemplateScript" }

Write-Info "Validando conexión SQL local ($SqlServer / $Database)..."
& sqlcmd -S $SqlServer -E -d $Database -Q "SELECT DB_NAME() AS DbName;" | Out-Null
Write-Ok "Conexión SQL validada."

Write-Info "Deteniendo procesos activos en puertos 5000 y 4200..."
Stop-ProcessByPort -port 5000
Stop-ProcessByPort -port 4200

Write-Info "Aplicando script idempotente de tablas de chat..."
& sqlcmd -S $SqlServer -E -d $Database -i $chatScript | Out-Null
Write-Ok "Tablas de chat listas."

Write-Info "Aplicando script idempotente de observaciones de pólizas..."
& sqlcmd -S $SqlServer -E -d $Database -i $polizasObservacionesScript | Out-Null
Write-Ok "Esquema de pólizas listo."

Write-Info "Aplicando script idempotente de plantilla de correo para cobros..."
& sqlcmd -S $SqlServer -E -d $Database -i $cobroEmailTemplateScript | Out-Null
Write-Ok "Esquema de EmailConfigs listo."

if (-not $SkipNpmInstall) {
    Write-Info "Verificando dependencias npm (frontend)..."
    if (-not (Test-Path (Join-Path $frontendPath "node_modules"))) {
        Write-Info "node_modules no existe, ejecutando npm install..."
        Push-Location $frontendPath
        try {
            npm install
        }
        finally {
            Pop-Location
        }
    }
    else {
        Write-Info "node_modules ya existe, se omite npm install."
    }
}

Write-Info "Iniciando backend en nueva consola..."
$backendCommand = "Set-Location '$backendPath'; dotnet run"
Start-Process -FilePath "powershell.exe" -ArgumentList @("-NoExit", "-Command", $backendCommand) | Out-Null

if (-not (Wait-HttpOk -url "$BackendUrl/health" -maxAttempts 45 -sleepSeconds 2)) {
    throw "Backend no respondió OK en $BackendUrl/health"
}
Write-Ok "Backend arriba: $BackendUrl"

Write-Info "Iniciando frontend en nueva consola..."
$frontendCommand = "Set-Location '$frontendPath'; npx ng serve --proxy-config '$frontendPath\proxy.conf.json' --port 4200 --no-open"
Start-Process -FilePath "powershell.exe" -ArgumentList @("-NoExit", "-Command", $frontendCommand) | Out-Null

if (-not (Wait-HttpOk -url $FrontendUrl -maxAttempts 80 -sleepSeconds 2)) {
    throw "Frontend no respondió OK en $FrontendUrl"
}
Write-Ok "Frontend arriba: $FrontendUrl"

if (-not $SkipSmokeTests) {
    Write-Info "Ejecutando smoke tests (health + login por proxy)..."

    $pwd = Convert-SecureToPlain -secure $SmokePassword
    $loginBody = @{ email = $SmokeUser; password = $pwd } | ConvertTo-Json

    $health = Invoke-WebRequest -Uri "$BackendUrl/health" -UseBasicParsing -TimeoutSec 8
    if ($health.StatusCode -lt 200 -or $health.StatusCode -ge 300) {
        throw "Health backend no devolvió 2xx"
    }

    $login = Invoke-RestMethod -Uri "$FrontendUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $loginBody -TimeoutSec 20
    if (-not $login -or -not $login.token) {
        throw "Smoke test de login no retornó token"
    }

    Write-Ok "Smoke tests exitosos."
}
else {
    Write-Warn "Smoke tests omitidos por parámetro -SkipSmokeTests"
}

Write-Host ""
Write-Host "Servicios locales listos para pruebas." -ForegroundColor Green
Write-Host "Backend : $BackendUrl" -ForegroundColor White
Write-Host "Frontend: $FrontendUrl" -ForegroundColor White
Write-Host "Usuario : $SmokeUser" -ForegroundColor White
Write-Host ""
Write-Host "Tip: si quieres saltar smoke tests: .\start-local-completo.ps1 -SkipSmokeTests" -ForegroundColor DarkGray
