@echo off
echo ====================================
echo  SIIGESE - Iniciando Backend Local
echo ====================================
echo.

cd /d "C:\Users\davil\SINSEG\enterprise-web-app\backend\src\WebApi"

echo Verificando base de datos...
dotnet ef database update

echo.
echo Iniciando servidor API en http://localhost:5000...
echo Presiona Ctrl+C para detener el servidor
echo.

dotnet run --urls "http://localhost:5000"

pause