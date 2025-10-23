@echo off
echo ================================================
echo  SIIGESE - Sistema de Gestión de Seguros Local
echo ================================================
echo.
echo Este script iniciará:
echo 1. Backend API (Puerto 5000)
echo 2. Frontend Angular (Puerto 4200)
echo.
echo IMPORTANTE: Necesitas tener instalado:
echo - .NET 8 SDK
echo - Node.js y npm
echo - SQL Server LocalDB
echo.
pause

echo.
echo Iniciando Backend en una nueva ventana...
start "SIIGESE Backend" cmd /k "C:\Users\davil\SINSEG\enterprise-web-app\start-backend.bat"

echo.
echo Esperando 5 segundos para que el backend inicie...
timeout /t 5 /nobreak

echo.
echo Iniciando Frontend en una nueva ventana...
start "SIIGESE Frontend" cmd /k "C:\Users\davil\SINSEG\enterprise-web-app\start-frontend.bat"

echo.
echo ================================================
echo  Servicios iniciados exitosamente!
echo ================================================
echo.
echo Backend API: http://localhost:5000
echo Frontend:    http://localhost:4200
echo.
echo Para detener los servicios, cierra las ventanas
echo correspondientes o presiona Ctrl+C en cada una.
echo.
pause