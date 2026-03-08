@echo off
echo ============================================
echo  PRUEBA RAPIDA - SIIGESE Local
echo ============================================
echo.

cd frontend-new

echo Verificando Node.js...
node --version
echo.

echo Verificando npm...
npm --version
echo.

echo Verificando Angular CLI...
npx ng version --quiet
echo.

echo Instalando dependencias...
npm install --silent
echo.

echo Configuracion local creada exitosamente!
echo.
echo Para iniciar el servidor de desarrollo:
echo   npm run start:local
echo.
echo Configuraciones disponibles:
echo   npm run start:local  - Desarrollo local con Mock API
echo   npm run build:local  - Build local
echo   npm run watch:local  - Build continuo local
echo.

pause