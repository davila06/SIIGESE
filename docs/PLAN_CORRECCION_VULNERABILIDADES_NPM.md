# Plan de Correccion de Vulnerabilidades NPM

## Contexto
- Proyecto: frontend Angular en `frontend-new`.
- Estado actual: build exitoso (`npm run build`).
- Hallazgo pendiente: `npm` reporta vulnerabilidades en dependencias (`1 low`, `10 moderate`, `32 high`).

## Objetivo
Reducir el riesgo de seguridad sin romper el build ni el comportamiento funcional, priorizando:
1. Vulnerabilidades explotables en runtime de produccion.
2. Vulnerabilidades en dependencias directas.
3. Dependencias de desarrollo no explotables en runtime.

## Alcance
- Incluye: `frontend-new/package.json` y `frontend-new/package-lock.json`.
- No incluye (en esta fase): backend NuGet, contenedores, sistema operativo.

## Estrategia
Aplicar un enfoque por fases con control de regresiones.

## Fase 0 - Baseline y respaldo (30 min)
1. Capturar estado actual:
```powershell
Set-Location frontend-new
npm audit --json > ..\docs\audit-baseline-frontend.json
npm ls --depth=0 > ..\docs\npm-ls-baseline-frontend.txt
```
2. Confirmar build baseline:
```powershell
npm run build
```
3. Crear rama de trabajo sugerida:
```powershell
git checkout -b chore/security-npm-remediation
```

## Fase 1 - Remediacion automatica segura (30-60 min)
1. Intentar fix no disruptivo:
```powershell
npm audit fix
```
2. Validar:
```powershell
npm run build
```
3. Recolectar delta:
```powershell
npm audit --json > ..\docs\audit-after-fix-frontend.json
```

Criterio de avance:
- Build verde.
- Reduccion de vulnerabilidades sin upgrades mayores de framework.

## Fase 2 - Remediacion dirigida por severidad (1-2 h)
Trabajar por paquete vulnerable priorizando dependencias directas.

1. Listar vulnerabilidades de alto impacto:
```powershell
npm audit
```
2. Para cada paquete directo vulnerable:
- Actualizar a version segura compatible.
- Ejecutar build y smoke test.

Comando de ejemplo:
```powershell
npm i <paquete>@<version-segura>
npm run build
```

Regla:
- Evitar `npm audit fix --force` al inicio.
- Usar `--force` solo si el riesgo operacional es aceptable y con plan de rollback.

## Fase 3 - Dependencias de desarrollo (45-90 min)
1. Separar runtime vs dev:
```powershell
npm audit --omit=dev
npm audit
```
2. Si quedan hallazgos solo en devDependencies:
- Prioridad media.
- Corregir en ventana controlada.

## Fase 4 - Validacion funcional minima (30-60 min)
Checklist:
1. Build produccion:
```powershell
npm run build
```
2. Smoke visual del shell y rutas clave:
- Login
- Dashboard
- Analytics
- Modulo de polizas/cobros/reclamos
3. Verificar que no aparezcan errores en consola del navegador.

## Fase 5 - Cierre y endurecimiento CI (30 min)
1. Guardar reporte final:
```powershell
npm audit --json > ..\docs\audit-final-frontend.json
```
2. Documentar excepciones (si alguna vulnerabilidad no se puede corregir inmediatamente):
- Paquete
- Severidad
- Justificacion
- Fecha objetivo de correccion

3. Agregar politica en CI/CD (recomendado):
- Bloquear merge en `high`/`critical` de runtime.
- Permitir temporalmente algunas de dev con excepcion documentada.

## Plan de rollback
Si una actualizacion rompe build o funcionalidad:
```powershell
git restore frontend-new/package.json frontend-new/package-lock.json
npm ci
npm run build
```

## Criterios de exito
- Build de frontend exitoso.
- Vulnerabilidades `high` de runtime minimizadas o eliminadas.
- Excepciones documentadas para lo no corregible de inmediato.
- Evidencia en `docs/` con baseline y resultado final.

## Riesgos y mitigaciones
- Riesgo: upgrade mayor rompa Angular/tooling.
- Mitigacion: cambios incrementales, validacion por cada paquete, no usar `--force` de entrada.

- Riesgo: falsos positivos en dependencias de desarrollo.
- Mitigacion: evaluar con `npm audit --omit=dev` para priorizar riesgo real de produccion.

## Entregables esperados
- `docs/audit-baseline-frontend.json`
- `docs/audit-after-fix-frontend.json`
- `docs/audit-final-frontend.json`
- `docs/npm-ls-baseline-frontend.txt`
- Cambios en `frontend-new/package.json` y `frontend-new/package-lock.json` validados por build.
