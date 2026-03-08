@echo off
echo ============================================
echo  SIIGESE - Inicio de Desarrollo Local
echo ============================================
echo.
echo Configurando entorno local...
echo.

cd /d "%~dp0frontend-new"

echo Verificando dependencias de Node.js...
call npm install

echo.
echo Iniciando servidor de desarrollo local...
echo URL: http://localhost:4200
echo Configuracion: Local (Mock API)
echo.
echo Credenciales de prueba:
echo Usuario: admin@sinseg.com
echo Password: password123
echo.
echo Presiona Ctrl+C para detener el servidor
echo.

call npm run start:local

pause