# ðŸ—„ï¸ DIRECCIONES DE BASE DE DATOS - OmnIA

# ðŸ—„ï¸ DIRECCIONES DE BASE DE DATOS - OmnIA

## ðŸ“‹ RESUMEN RÃPIDO

### ï¿½ï¸ **SQL SERVER EXPRESS LOCAL** (CONFIGURACIÃ“N ACTUAL)
```
Servidor: Karo\SQLEXPRESS
Base de Datos: SinsegAppDb
AutenticaciÃ³n: Windows Authentication
Connection String: Server=Karo\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;Connection Timeout=30;
```

### ï¿½ **DOCKER SQL SERVER** (Alternativa)
```
Servidor: localhost,1433
Usuario: sa
ContraseÃ±a: DevPassword123!
Base de Datos: MiAppDb
Connection String: Server=localhost,1433;Database=MiAppDb;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;
```

### â˜ï¸ **AZURE SQL DATABASE** (ProducciÃ³n)
```
Servidor: siinadseg-sqlserver-1019.database.windows.net
Usuario: siinadseg_admin
ContraseÃ±a: P@ssw0rd123!
Base de Datos: SiinadsegDB
Connection String: Server=tcp:siinadseg-sqlserver-1019.database.windows.net,1433;Database=SiinadsegDB;User ID=siinadseg_admin;Password=P@ssw0rd123!;Encrypt=True;TrustServerCertificate=False;
```

## ðŸš€ COMANDOS RÃPIDOS

### Iniciar Docker Database
```bash
docker-compose up db -d
```

### Probar Conexiones
```powershell
.\test-database-connections.ps1
```

### Acceso con SSMS
1. **Local**: `Karo\SQLEXPRESS` con Windows Authentication
2. **Docker**: `localhost,1433` con `sa/DevPassword123!`
3. **Azure**: `siinadseg-sqlserver-1019.database.windows.net` con `siinadseg_admin/P@ssw0rd123!`

## ðŸ“ ARCHIVOS RELACIONADOS
- `docker-compose.yml` - ConfiguraciÃ³n de Docker
- `EJECUTAR_COMPLETO.sql` - Script de creaciÃ³n de BD
- `test-database-connections.ps1` - Test de conexiones
- `README_DATABASE.md` - DocumentaciÃ³n completa

---
*Actualizado: 20 de octubre de 2025*
