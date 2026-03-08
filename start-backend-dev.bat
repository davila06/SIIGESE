@echo off
echo Starting Backend Service...
echo.

cd /d "C:\Users\davil\SINSEG\enterprise-web-app\backend\src\WebApi"

echo Setting environment to Development...
set ASPNETCORE_ENVIRONMENT=Development

echo.
echo Starting .NET Web API on port 5000...
echo Backend will be available at: http://localhost:5000
echo API endpoint: http://localhost:5000/api/auth/login
echo.
echo Press Ctrl+C to stop the server
echo.

dotnet run --urls "http://localhost:5000"

pause
