Param(
    [string]$SqlServer = "Karo\SQLEXPRESS",
    [string]$Database = "SinsegAppDb",
    [string]$BackendUrl = "http://localhost:5000",
    [string]$FrontendUrl = "http://localhost:4200",
    [string]$SmokeUser = "admin@sinseg.com",
    [SecureString]$SmokePassword = (ConvertTo-SecureString "password123" -AsPlainText -Force)
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
    if (-not $listeners) { return }

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

function Wait-HttpOk([string]$url, [int]$maxAttempts = 30, [int]$sleepSeconds = 2) {
    for ($i = 1; $i -le $maxAttempts; $i++) {
        try {
            $resp = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 8
            if ($resp.StatusCode -ge 200 -and $resp.StatusCode -lt 300) {
                return $true
            }
        }
        catch {
            # Continue retrying.
        }

        Write-Info "Esperando respuesta de $url (intento $i/$maxAttempts)..."
        Start-Sleep -Seconds $sleepSeconds
    }

    return $false
}

$root = $PSScriptRoot
$backendPath = Join-Path $root "backend\src\WebApi"
$frontendPath = Join-Path $root "frontend-new"
$chatScript = Join-Path $root "10_CreateChatTables.sql"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " SINSEG - Reinicio local + Smoke tests" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

Write-Info "Deteniendo procesos activos en puertos 5000 y 4200..."
Stop-ProcessByPort -port 5000
Stop-ProcessByPort -port 4200

Write-Info "Verificando/creando tablas de chat en $SqlServer/$Database..."
sqlcmd -S $SqlServer -E -d $Database -i $chatScript | Out-Null
Write-Ok "Script de tablas de chat aplicado (idempotente)."

Write-Info "Iniciando backend (.NET) en nueva consola..."
$backendCommand = "Set-Location '$backendPath'; dotnet run"
Start-Process -FilePath "powershell.exe" -ArgumentList @("-NoExit", "-Command", $backendCommand) | Out-Null

if (-not (Wait-HttpOk -url "$BackendUrl/health" -maxAttempts 40 -sleepSeconds 2)) {
    throw "Backend no respondió OK en $BackendUrl/health"
}
Write-Ok "Backend saludable en $BackendUrl/health"

Write-Info "Iniciando frontend (Angular) en nueva consola..."
$frontendCommand = "Set-Location '$frontendPath'; npm run start"
Start-Process -FilePath "powershell.exe" -ArgumentList @("-NoExit", "-Command", $frontendCommand) | Out-Null

if (-not (Wait-HttpOk -url $FrontendUrl -maxAttempts 60 -sleepSeconds 2)) {
    throw "Frontend no respondió OK en $FrontendUrl"
}
Write-Ok "Frontend disponible en $FrontendUrl"

Write-Info "Ejecutando smoke test de login..."
$passwordPtr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SmokePassword)
try {
    $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPtr)
}
finally {
    if ($passwordPtr -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPtr)
    }
}

$loginBody = @{ email = $SmokeUser; password = $plainPassword } | ConvertTo-Json
$loginResp = Invoke-RestMethod -Uri "$BackendUrl/api/auth/login" -Method Post -ContentType "application/json" -Body $loginBody

if (-not $loginResp -or -not $loginResp.token) {
    throw "Login smoke test no retornó token."
}
Write-Ok "Login smoke test exitoso para $SmokeUser"

Write-Host "" 
Write-Host "Servicios arriba y smoke tests completados." -ForegroundColor Green
Write-Host "Backend:  $BackendUrl" -ForegroundColor White
Write-Host "Frontend: $FrontendUrl" -ForegroundColor White
