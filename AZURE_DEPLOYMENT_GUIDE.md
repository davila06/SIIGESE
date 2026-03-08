# 🚀 Guía Completa de Deployment en Azure
# SIINADSEG Enterprise Web Application

## 📋 Índice
1. [Recursos de Azure Necesarios](#recursos-de-azure-necesarios)
2. [Scripts de Deployment](#scripts-de-deployment)
3. [Configuración de Base de Datos](#configuración-de-base-de-datos)
4. [Configuración de Frontend](#configuración-de-frontend)
5. [Configuración de Backend](#configuración-de-backend)
6. [Variables de Entorno](#variables-de-entorno)
7. [Pasos de Deployment](#pasos-de-deployment)

## 🏗️ Recursos de Azure Necesarios

### 1. Resource Group
- **Nombre**: `rg-siinadseg-prod`
- **Ubicación**: `East US` o `West Europe`

### 2. SQL Database
- **Servidor**: `siinadseg-sql-server`
- **Base de Datos**: `SiinadsegDB`
- **Tier**: Standard S1 (para producción)

### 3. App Service Plan
- **Nombre**: `asp-siinadseg-backend`
- **Tier**: B1 Basic (escalable)

### 4. App Service (Backend)
- **Nombre**: `app-siinadseg-backend`
- **Runtime**: .NET 8
- **OS**: Windows

### 5. Static Web App (Frontend)
- **Nombre**: `swa-siinadseg-frontend`
- **Framework**: Angular
- **Build preset**: Angular

### 6. Application Insights
- **Nombre**: `ai-siinadseg-monitoring`
- **Tipo**: Web Application

### 7. Key Vault (Opcional pero recomendado)
- **Nombre**: `kv-siinadseg-secrets`
- **Para almacenar secrets de producción**

## 🎯 Estimación de Costos Mensuales
- SQL Database (S1): ~$30 USD
- App Service Plan (B1): ~$13 USD
- Static Web App: Gratis (hasta 100GB bandwidth)
- Application Insights: ~$5 USD
- **Total estimado**: ~$50 USD/mes

## 📝 Próximos Pasos
1. Ejecutar script de creación de recursos
2. Configurar pipelines de CI/CD
3. Configurar variables de entorno
4. Realizar primer deployment
5. Configurar dominios personalizados (opcional)

---
*Creado el 23-10-2025 para deployment en Azure*