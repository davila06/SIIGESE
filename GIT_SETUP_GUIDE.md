# 🚀 Guía para Subir SIINADSEG a Git Repository

## 📋 **Opción 1: Usando GitHub Desktop (Recomendado para principiantes)**

### Paso 1: Instalar GitHub Desktop
1. Descargar desde: https://desktop.github.com/
2. Instalar y abrir la aplicación
3. Hacer login con tu cuenta de GitHub

### Paso 2: Crear el Repositorio
1. En GitHub Desktop: **File → New Repository**
2. Configurar:
   - **Name:** `siinadseg`
   - **Description:** `Sistema Integral de Administración de Seguros`
   - **Local Path:** `C:\Users\davil\SINSEG\enterprise-web-app`
   - ✅ **Initialize with README** (desmarcar, ya tenemos README)
   - ✅ **Git ignore:** Node (seleccionar)

### Paso 3: Publicar a GitHub
1. Clic en **"Publish repository"**
2. Elegir:
   - ✅ **Public** (para repositorio público)
   - ❌ **Private** (para repositorio privado)
3. Clic en **"Publish repository"**

---

## 📋 **Opción 2: Usando Línea de Comandos (Avanzado)**

### Paso 1: Instalar Git
1. Descargar desde: https://git-scm.com/download/windows
2. Instalar con configuración por defecto
3. Reiniciar PowerShell

### Paso 2: Configurar Git (Solo primera vez)
```powershell
git config --global user.name "Tu Nombre"
git config --global user.email "tu-email@gmail.com"
```

### Paso 3: Inicializar y Subir el Repositorio
```powershell
# Navegar al directorio del proyecto
cd "C:\Users\davil\SINSEG\enterprise-web-app"

# Inicializar repositorio Git
git init

# Agregar todos los archivos
git add .

# Crear primer commit
git commit -m "Initial commit: SIINADSEG v1.0 - Sistema completo con módulos de pólizas, cobros, reclamos y usuarios"

# Agregar origen remoto (reemplazar con tu URL)
git remote add origin https://github.com/TU-USUARIO/siinadseg.git

# Subir código
git push -u origin main
```

---

## 📋 **Opción 3: Usando Visual Studio Code (Integrado)**

### Paso 1: Abrir en VS Code
1. Abrir VS Code
2. **File → Open Folder** 
3. Seleccionar: `C:\Users\davil\SINSEG\enterprise-web-app`

### Paso 2: Inicializar Git
1. **View → Source Control** (Ctrl+Shift+G)
2. Clic en **"Initialize Repository"**
3. Aparecerán todos los archivos en "Changes"

### Paso 3: Hacer Commit
1. Escribir mensaje: `Initial commit: SIINADSEG v1.0`
2. Clic en **✓ Commit**
3. Clic en **"Publish to GitHub"**
4. Elegir público/privado
5. ¡Listo!

---

## 🔐 **Seguridad: Configuración de Credenciales**

### Para GitHub (Recomendado)
1. **Personal Access Token (PAT):**
   - Ir a GitHub.com → Settings → Developer settings → Personal access tokens
   - Generate new token (classic)
   - Scopes: `repo`, `workflow`
   - Usar token como contraseña

### Para Otros Servicios
1. **SSH Keys (Más seguro):**
   ```powershell
   ssh-keygen -t rsa -b 4096 -C "tu-email@gmail.com"
   ```
   - Agregar la clave pública al servicio Git

---

## 📁 **Archivos que se Subirán**

### ✅ **Incluidos:**
```
enterprise-web-app/
├── backend/src/                 # Código fuente .NET
├── frontend-new/src/           # Código fuente Angular
├── *.sql                      # Scripts de base de datos
├── README_NEW.md              # Documentación técnica
├── SIINADSEG_*.md             # Documentación comercial
├── .gitignore                 # Archivos a ignorar
└── docker-compose.yml         # Configuración Docker
```

### ❌ **Excluidos (por .gitignore):**
- `node_modules/` - Dependencias de Node.js
- `bin/`, `obj/` - Archivos compilados .NET
- `*.env` - Variables de entorno secretas
- `.vs/`, `.vscode/` - Configuración de IDE
- `*.log` - Archivos de log

---

## 🛡️ **Antes de Subir - Lista de Verificación**

### 1. Revisar Información Sensible
- [ ] Contraseñas en `appsettings.json`
- [ ] Claves de API en environment files
- [ ] Cadenas de conexión con credenciales
- [ ] Tokens de autenticación

### 2. Preparar Configuración
```json
// appsettings.json (ejemplo)
{
  "ConnectionStrings": {
    "DefaultConnection": "REPLACE_WITH_YOUR_CONNECTION_STRING"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_YOUR_SECRET_KEY",
    "Issuer": "SIINADSEG",
    "Audience": "SIINADSEG-Users"
  }
}
```

### 3. Documentación
- [ ] README actualizado
- [ ] Instrucciones de instalación claras
- [ ] Configuración de environment variables

---

## 🎯 **Comandos Útiles para Después**

### Actualizar Repositorio
```powershell
# Agregar cambios
git add .

# Commit con mensaje descriptivo
git commit -m "feat: Agregar funcionalidad de reportes"

# Subir cambios
git push
```

### Crear Ramas para Desarrollo
```powershell
# Crear rama nueva
git checkout -b feature/nueva-funcionalidad

# Subir rama
git push -u origin feature/nueva-funcionalidad
```

### Clonar en Otro PC
```powershell
git clone https://github.com/TU-USUARIO/siinadseg.git
cd siinadseg
```

---

## ⚠️ **Importantes Consideraciones de Seguridad**

### 🚫 **NUNCA Incluir:**
- Contraseñas de base de datos
- Claves de API de terceros
- Certificates privados
- Información de producción
- Datos de clientes reales

### ✅ **SÍ Incluir:**
- Código fuente
- Configuración de ejemplo
- Documentación
- Scripts de inicialización
- Estructura de base de datos (sin datos)

---

## 📞 **Si Necesitas Ayuda**

### Recursos de Aprendizaje
- **Git Tutorial:** https://learngitbranching.js.org/
- **GitHub Docs:** https://docs.github.com/
- **VS Code Git:** https://code.visualstudio.com/docs/editor/versioncontrol

### Servicios Git Populares
- **GitHub:** https://github.com (Más popular)
- **GitLab:** https://gitlab.com (Incluye CI/CD)
- **Bitbucket:** https://bitbucket.org (Integración con Atlassian)
- **Azure DevOps:** https://dev.azure.com (Microsoft)

---

**¡Tu código está listo para ser compartido de forma segura!** 🚀

*Recuerda: Siempre revisar qué archivos estás subiendo antes de hacer push al repositorio.*