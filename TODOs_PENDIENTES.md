# TODOs PENDIENTES — SINSEG Enterprise Web App

> Actualizado: 11 abril 2026 (revisión completa) — Análisis completo de código, stubs, funciones vacías y features ocultas.  
> Scope: `frontend-new/src/` + `backend/src/`

---

## Checklist de progreso

### FRONTEND

#### 🔴 Crítico — Funciones vacías con botones activos en producción

- [x] **[FE-01]** `emails/send-email` — Botón **"Vista Previa"** implementado.  
  **Implementado:** `EmailPreviewDialogComponent` standalone (`shared/components/email-preview-dialog/`) abre un `MatDialog` en modo `preview` con el HTML del email renderizado mediante `DomSanitizer.bypassSecurityTrustHtml`, aislado con `isolation: isolate`.

- [x] **[FE-02]** `emails/email-history` — Botón **"Ver Detalles"** implementado.  
  **Implementado:** reutiliza `EmailPreviewDialogComponent` en modo `details` mostrando asunto, destinatario, fecha, tipo, estado y mensaje de error si aplica.

- [x] **[FE-03]** `emails/email-history` — Botón **"Reenviar"** implementado.  
  **Implementado:** llama a `EmailService.resendEmail(id)` → `POST /api/email/{id}/resend`; muestra snackbar con resultado real del servidor.

---

#### 🟠 Alto — Tabs ocultos con funcionalidad pendiente de implementar

- [x] **[FE-04]** `reclamos/reclamo-detalle` — Tab **"Histórico"** implementado con línea de tiempo.
  **HTML:** `frontend-new/src/app/reclamos/components/reclamo-detalle/reclamo-detalle.component.html`
  **Implementado:**
  1. Backend: endpoint `GET /api/reclamos/{id}/historial` con entidad `ReclamoHistorial`, migración EF Core.
  2. Auditoría automática en `CambiarEstadoAsync`, `CreateReclamoAsync`, `UpdateReclamoAsync`, `AsignarUsuarioAsync`, `ResolverReclamoAsync`.
  3. Frontend: timeline vertical con iconos por tipo de evento, valores anterior/nuevo, usuario y timestamp.

- [x] **[FE-05]** `reclamos/reclamo-detalle` — Tab **"Documentos"** implementado con upload/lista.
  **HTML:** `frontend-new/src/app/reclamos/components/reclamo-detalle/reclamo-detalle.component.html`
  **Implementado:**
  1. Backend: endpoints `GET/POST/DELETE /api/reclamos/{id}/documentos` + `GET /api/reclamos/{id}/documentos/{docId}` (descarga).
  2. Metadatos almacenados como JSON en `Reclamo.DocumentosAdjuntos` — sin nueva tabla.
  3. Archivos físicos en `{contentRoot}/uploads/reclamos/{id}/` con protección path-traversal.
  4. Frontend: lista de documentos con icono por tipo, tamaño, fecha/usuario, descarga y eliminación.
  5. Input file oculto + botón "Subir Documento", progress bar durante upload, validación de 20 MB.

---

#### 🟡 Medio — Logs crudos en `main.ts` exponen información en producción

- [x] **[FE-06]** `main.ts` — Todos los `console.log()` de arranque ahora están protegidos con guardia `if (!environment.production)`.  
  **Archivo:** `frontend-new/src/main.ts`  
  **Implementado:** Bloque `if (!environment.production)` envuelve los 4 console.log de diagnóstico. En producción no se emite ningún mensaje al navegador.

---

#### 🟡 Medio — Monitoreo de errores en producción no conectado

- [x] **[FE-07]** Monitoreo de producción implementado con `@microsoft/applicationinsights-web` v3.4.1.  
  **Archivos creados/modificados:**  
  1. `frontend-new/src/app/services/app-insights.service.ts` — Wrapper tipado del SDK: `trackException`, `trackTrace`, `trackEvent`, `trackPageView`, `setAuthenticatedUserContext`, `clearAuthenticatedUserContext`. Auto-enrutado de SPA, correlación AJAX, auto-colección de excepciones.  
  2. `frontend-new/src/app/error-handler/global-error-handler.ts` — `ErrorHandler` de Angular que clasifica errores: `HttpErrorResponse` (Warning/Error por código), `ChunkLoadError` (Warning + reload automático anti-loop), errores genéricos (Error). En desarrollo preserva `console.error`.  
  3. `frontend-new/src/app/services/logging.service.ts` — `log()` → `trackTrace(Information)`, `warn()` → `trackTrace(Warning)`, `error()` → `trackException` con stack trace preservado.  
  4. `frontend-new/src/app/app.module.ts` — `{ provide: ErrorHandler, useClass: GlobalErrorHandler }` registrado.  
  5. `frontend-new/src/app/services/auth.service.ts` — `setAuthenticatedUserContext(userId)` en login, `clearAuthenticatedUserContext()` en logout.  
  6. Todos los `environment.*.ts` — campo `appInsightsConnectionString` agregado.  
  **Activación:** Reemplazar `InstrumentationKey=00000000...` en `environment.prod.ts` y `environment.production-final.ts` con la cadena de conexión real de Azure Portal → Application Insights → Overview.

---

#### 🟢 Menor — Template DEMO usa asset estático

- [ ] **[FE-08]** `polizas/upload` — `downloadDemo()` descarga un Excel estático desde `/assets/demo/DEMO_polizas.xlsx`.
  **Archivo:** `frontend-new/src/app/polizas/upload-polizas.component.ts` línea 131  
  **Estado actual:** ⬜ Pendiente — sigue usando asset estático (confirmado en código, línea 133).  
  **Riesgo:** Si el schema de columnas cambia, el demo queda desactualizado sin ningún aviso.  
  **Fix sugerido:** Generar el demo dinámicamente desde el backend (endpoint `GET /api/polizas/demo`) usando ClosedXML con 10 filas de datos de ejemplo y las columnas actuales del schema.

---

### BACKEND

#### 🔴 Crítico — Carga masiva de Clientes por Excel sin implementar

- [x] **[BE-01]** `ClienteService.ProcesarExcelAsync()` implementado con flujo enterprise de importación Excel.  
  **Archivo:** `backend/src/Application/Services/ClienteService.cs`  
  **Implementado:**  
  1. Lectura con `ClosedXML` y mapeo flexible de encabezados (alias por columna).  
  2. Validación de columnas requeridas (RazonSocial/Nombre, NIT, Email, Teléfono, Dirección).  
  3. Validación por fila (obligatorios, formato de email, deduplicación en archivo).  
  4. Upsert en BD por NIT o Código con truncamiento seguro por límites de esquema.  
  5. Registro de auditoría en `DataRecords` con estado (`Completed`, `Completed with errors`, `Failed`, `No data`).  
  6. Retorno de `DataUploadResultDto` con totales, errores por fila y detalle de fallos.

---

#### 🟡 Medio — Endpoint de reenvío de email posiblemente inexistente

- [x] **[BE-02]** Endpoint de reenvío implementado y endurecido a nivel enterprise.  
  **Rutas soportadas:** `POST /api/email/{id}/resend` y `POST /api/emails/{id}/resend` (compatibilidad).  
  **Implementado:**  
  1. Persistencia de historial en tabla `EmailLogs` con repositorio dedicado.  
  2. `GetEmailHistoryAsync()` ahora devuelve historial real paginado desde BD.  
  3. `ResendEmailAsync(id)` ahora reenvía desde registro persistido (no stub), con validación de id/cuerpo y trazabilidad de resultado.  
  4. Registro de intentos exitosos/fallidos de envío y reenvío (`EmailLogId` retornado en respuesta).  
  5. Métricas de `GetStatsAsync()` conectadas al historial persistido (`TotalSent`, `TotalFailed`).

---

#### 🟡 Medio — Endpoints de historial y documentos de reclamos inexistentes

- [x] **[BE-03]** Endpoint de historial implementado a nivel enterprise.  
  **Ruta soportada:** `GET /api/reclamos/{id}/historial`.
  **Implementado:**
  1. Validación de existencia de reclamo (`404` cuando no existe).  
  2. Lectura de historial real desde `ReclamoHistoriales` ordenado por fecha descendente.  
  3. Respuesta tipada con trazabilidad de evento, valores antes/después, usuario y fecha del cambio.

  **Archivos clave:** `Domain/Entities/ReclamoHistorial.cs`, `Application/DTOs/ReclamoHistorialDtos.cs`, `Infrastructure/Data/Repositories/ReclamoHistorialRepository.cs`, migración `AddReclamoHistorial`.

- [x] **[BE-04]** Endpoints de documentos adjuntos implementados a nivel enterprise.  
  - `POST /api/reclamos/{id}/documentos` — Upload de archivo  
  - `GET /api/reclamos/{id}/documentos` — Listar adjuntos  
  - `DELETE /api/reclamos/{id}/documentos/{docId}` — Eliminar adjunto  
  **Implementado:**
  1. Upload seguro con validación de reclamo, tamaño máximo y extensiones permitidas.  
  2. Almacenamiento local configurable por `appsettings.json` en `FileUpload:Reclamos`.  
  3. Persistencia de metadata en `DocumentosAdjuntos` + auditoría de eventos (`DocumentoAgregado`/`DocumentoEliminado`).  
  4. Eliminación lógica (metadata) y física (archivo en disco) al borrar documento.

  **Almacenamiento:** filesystem local configurable por `appsettings.json`; arquitectura preparada para migrar a Azure Blob (campo `Ruta` desacoplado en metadata).

---

## Resumen por prioridad

| ID | Prioridad | Área | Descripción |
|----|-----------|------|-------------|
| FE-01 | ✅ HECHO | Frontend / Emails | `previewEmail()` — dialog implementado |
| FE-02 | ✅ HECHO | Frontend / Emails | `viewEmailDetails()` — dialog de detalles implementado |
| FE-03 | ✅ HECHO | Frontend / Emails | `resendEmail()` + backend `POST /api/email/{id}/resend` implementados |
| BE-01 | ✅ HECHO | Backend / Clientes | `ProcesarExcelAsync` implementado con validación y upsert desde Excel |
| FE-04 | ✅ HECHO | Frontend / Reclamos | Tab "Histórico" con timeline de auditoría |
| FE-05 | ✅ HECHO | Frontend / Reclamos | Tab "Documentos" con upload/lista/descarga |
| FE-06 | ✅ HECHO | Frontend / App | `console.log` raw guardado con `if (!environment.production)` en `main.ts` |
| FE-07 | ✅ HECHO | Frontend / Ops | Application Insights integrado: `AppInsightsService`, `GlobalErrorHandler`, `LoggingService` forward |
| BE-02 | ✅ HECHO | Backend / Emails | Reenvío por ID implementado con historial persistido y rutas singular/plural |
| BE-03 | ✅ HECHO | Backend / Reclamos | Endpoint `GET /api/reclamos/{id}/historial` implementado con historial real |
| BE-04 | ✅ HECHO | Backend / Reclamos | Endpoints de documentos (`POST/GET/DELETE`) implementados con storage configurable |
| **M9** | **✅ HECHO** | **Analítica** | **Cliente 360°** — `GET /api/analytics/cliente360` + componente Angular standalone con tabs: Resumen (LTV chart + scores), Pólizas, Cobros, Reclamos |
| **M14** | **✅ HECHO** | **Analítica** | **Agenda Inteligente** — `GET /api/analytics/agenda` + componente Angular standalone con 5 secciones de prioridad |
| FE-08 | ⬜ PENDIENTE | Frontend / Upload | Demo Excel estático puede quedar desactualizado — fix: endpoint dinámico |

---

## Notas de implementación

### [BE-04] Migrar almacenamiento de documentos a Azure Blob Storage (completado)
Estado: ✅ Implementado y compilado sin errores.

Implementación realizada:
- NuGet agregado: `Azure.Storage.Blobs` en `Infrastructure.csproj`.
- Se creó `IBlobStorageService` con `UploadAsync`, `GetUrlAsync`, `DeleteAsync`.
- Se implementó `AzureBlobStorageService` con:
  - creación automática del contenedor si no existe,
  - reintentos exponenciales del SDK,
  - `ContentType` correcto por blob,
  - borrado idempotente con `DeleteIfExistsAsync`.
- Se migró `ReclamosController` para usar Blob Storage en upload/delete sin tocar el contrato de metadata (`Ruta` continúa desacoplada del backend de storage).
- Se registró DI en `Program.cs` (`IBlobStorageService` → `AzureBlobStorageService`).
- Se agregó configuración:
  - `ConnectionStrings:AzureBlob`
  - `AzureBlobStorage:ContainerName`
  - `AzureBlobStorage:ReclamosPrefix`

Configuración requerida en App Service → Configuration:
- `ConnectionStrings__AzureBlob` = `<azure-blob-connection-string>`


### [OPS] Redis en Producción — Azure Portal

El código backend ya soporta Redis (`appsettings.Production.json`, `appsettings.ProductionFinal.json`, `Program.cs`).
Falta provisionar y conectar en Azure:

**Arquitectura:** single-instance — Redis no es necesario por ahora.

- [x] Documentar en el README que el token blacklist es in-memory y solo funciona en single-instance  
  **Implementado:** `README.md` líneas 173–182 — tabla single-instance vs multi-instance + nota de operación.  
- [x] Si en el futuro escala a multi-instance: documentado en README con instrucción para provisionar Azure Cache for Redis (Basic C0 ~$17/mes) y agregar `ConnectionStrings__Redis` en App Service.

### [FE-07] Activar Application Insights en producción

El SDK está integrado y listo. Para conectarlo al recurso real de Azure:

1. **Crear el recurso** (si no existe): Azure Portal → Create a resource → Application Insights → Workspace-based.
2. **Obtener la Connection String**: Application Insights → Overview → "Connection String" (botón copiar).  
  Formato: `InstrumentationKey=<guid>;IngestionEndpoint=https://eastus2-3.in.applicationinsights.azure.com/`
3. **Pegar en** `frontend-new/src/environments/environment.prod.ts` → campo `appInsightsConnectionString`.
4. **Si se usan pipelines CI/CD**, inyectar como variable de entorno segura en lugar de hardcodear en el archivo.
5. **Verificar** en Azure Portal → Application Insights → Failures / Exceptions que los errores llegan tras el primer despliegue con la clave real.
