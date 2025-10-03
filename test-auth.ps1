$body = @{
    email = "admin@sinseg.com"
    password = "password123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $body -ContentType "application/json"
    Write-Host "✅ Login exitoso!" -ForegroundColor Green
    Write-Host "Token: $($response.token)" -ForegroundColor Yellow
    Write-Host "User: $($response.user.email)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Error en login:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Yellow
    }
}