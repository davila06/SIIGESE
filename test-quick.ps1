try {
    Write-Host "🔄 Probando login..." -ForegroundColor Yellow
    $body = '{"email":"admin@sinseg.com","password":"password123"}'
    $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/login' -Method POST -Body $body -ContentType 'application/json'
    Write-Host "✅ ¡LOGIN EXITOSO!" -ForegroundColor Green
    Write-Host "Email: $($response.user.email)" -ForegroundColor Cyan
    Write-Host "Role: $($response.user.role)" -ForegroundColor Magenta
    Write-Host "Token recibido: ✓" -ForegroundColor Green
} catch {
    Write-Host "❌ Error en login:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}