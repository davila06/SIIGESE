# TODOs PENDIENTES — Análisis Exhaustivo del Proyecto

> Actualizado: 2026-03-18 — Todos los ítems de código implementados. Solo quedan tareas OPS.

---

## Resumen Ejecutivo

| Categoría | Estado |
|-----------|--------|
| TODOs / código pendiente (Frontend + Backend) | 0 ✅ |
| `console.*` en producción | 0 ✅ |
| Código comentado (bloques grandes) | 0 ✅ |
| Archivos sin uso / mock huérfanos | 0 ✅ |
| Logger centralizado | ✅ `LoggingService` en 21 archivos |
| Gate de cobertura CI | ✅ 60% mínimo — `coverage.runsettings` + CI |
| **OPS pendiente** | **1 ⏳** |

---

## Pendientes

### [OPS] Redis en Producción — Azure Portal

El código backend ya soporta Redis (`appsettings.Production.json`, `appsettings.ProductionFinal.json`, `Program.cs`).
Falta provisionar y conectar en Azure:

**Arquitectura:** single-instance — Redis no es necesario por ahora.

- [ ] Documentar en el README que el token blacklist es in-memory y solo funciona en single-instance
- [ ] Si en el futuro escala a multi-instance: provisionar Azure Cache for Redis (Basic tier ~$17/mes) y agregar `ConnectionStrings__Redis` en App Service → Configuration → Application settings
