@echo off
echo ====================================
echo  SIIGESE - Iniciando Frontend Local
echo ====================================
echo.

cd /d "C:\Users\davil\SINSEG\enterprise-web-app\frontend-new"

echo Verificando dependencias...
call npm install

echo.
echo Iniciando servidor Angular en http://localhost:4200...
echo El navegador se abrirá automáticamente
echo Presiona Ctrl+C para detener el servidor
echo.

call npm start

pause