#!/bin/bash

# Script para deploy manual a Azure Static Web Apps
echo "Iniciando deploy manual..."

# Compilar frontend para producción
cd frontend-new
npm run build --configuration production

# Deploy usando Azure CLI
az staticwebapp build --name swa-siinadseg-main-8509 --source dist/frontend-new --resource-group rg-siinadseg

echo "Deploy completado!"