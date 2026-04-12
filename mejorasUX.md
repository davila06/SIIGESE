# SINSEG — Plan de Mejoras UI/UX
> Generado: Abril 2026 | Ordenado por impacto visual descendente

---

## FASE 1 — Crítico (visualmente roto)

### 1.1 Login — sistema visual desconectado del resto de la app
**Archivo:** `frontend-new/src/app/auth/login.component.scss`  
**Problema:** El login usa `$brand-accent: #4F46E5` (indigo), fondo `#0d1226 → #111827`, logo `shield` — completamente diferente al tema neon cyan (`#00E5FF`) del resto. Parece otra aplicación.  
**Fix:**
- Reemplazar todas las variables SCSS locales (`$brand-accent`, `$brand-dark`, `$brand-mid`) con `var(--color-primary)`, `var(--color-bg)`, `var(--color-surface)`
- Cambiar `$brand-accent2: #818cf8` → `var(--color-primary-light)`
- Cambiar el ícono de logo de `shield` → `security` (igual que el sidebar)
- Reemplazar todos los `rgba(99,102,241,...)` con `var(--color-primary-glow)` / `var(--color-border)`
- Actualizar `© 2025 SINSEG` → `© {{ currentYear }} SINSEG` (TS: `currentYear = new Date().getFullYear()`)

---

### 1.2 cobro-detalle — stylesheet escrito para modo claro
**Archivo:** `frontend-new/src/app/cobros/components/cobro-detalle/cobro-detalle.component.scss`  
**Problema:** Usa `color: rgba(0,0,0,0.87)`, `color: rgba(0,0,0,0.6)`, `background-color: #f5f5f5`, `color: #2e7d32`, `color: #d32f2f`. En dark mode el texto es negro sobre `#111122` — completamente ilegible.  
**Fix:** Reemplazar todos los valores hardcoded con tokens:
- `rgba(0,0,0,0.87)` → `var(--color-text-primary)`
- `rgba(0,0,0,0.6)` → `var(--color-text-secondary)`
- `#f5f5f5` → `var(--color-surface-2)`
- `#2e7d32` → `var(--color-success)`
- `#d32f2f` → `var(--color-error)`

---

### 1.3 cobro-detalle — sin estado de error
**Archivo:** `frontend-new/src/app/cobros/components/cobro-detalle/cobro-detalle.component.html`  
**Problema:** Solo existen dos estados: `*ngIf="loading"` y `*ngIf="!loading && cobro"`. Si la API devuelve 404/500, la pantalla queda en blanco.  
**Fix:** Agregar tercer bloque:
```html
<div *ngIf="!loading && !cobro" class="empty-state">
  <mat-icon>error_outline</mat-icon>
  <span class="empty-title">Cobro no encontrado</span>
  <span class="empty-message">No se pudo cargar la información del cobro.</span>
  <button mat-stroked-button (click)="volver()">Volver al listado</button>
</div>
```

---

### 1.4 email-dashboard — textos invisibles en dark mode
**Archivo:** `frontend-new/src/app/emails/email-dashboard/email-dashboard.component.scss`  
**Problema:** `.header h1 { color: #333 }`, `.header p { color: #666 }`, `.loading-container p { color: #666 }` — hardcoded para modo claro.  
**Fix:** Reemplazar con tokens:
- `#333` → `var(--color-text-primary)`
- `#666` → `var(--color-text-secondary)`

---

## FASE 2 — Importante (UX rota o inconsistente)

### 2.1 Emoji como iconos primarios (4 páginas)
**Archivos:**
- `emails/email-dashboard/email-dashboard.component.html` línea ~4: `<h1>📧 Sistema...`
- `emails/send-email/send-email.component.html` línea ~4: `<h1>📝 Redactar...`
- `emails/email-history/email-history.component.html` línea ~4: `<h1>📋 Historial...`
- `polizas/upload-polizas.component.html` línea ~10: `<h1>📊 Subir Pólizas...`

**Problema:** Los emoji escalan distinto por OS, no se pueden colorear con CSS, y son inconsistentes con el resto de páginas que usan `mat-icon`.  
**Fix:** Reemplazar cada emoji con `<mat-icon aria-hidden="true">` + icono equivalente:
- 📧 → `email`
- 📝 → `edit_note`
- 📋 → `history`
- 📊 → `upload_file`

---

### 2.2 usuarios — form fields sin `appearance="outline"`
**Archivo:** `frontend-new/src/app/usuarios/usuarios.component.html`  
**Problema:** Todos los `<mat-form-field>` no tienen `appearance="outline"` — renderizan como "fill" (estilo deprecated) mientras el resto de la app usa outline uniformemente.  
**Fix:** Agregar `appearance="outline"` a todos los `mat-form-field` del componente.

---

### 2.3 usuarios — campos de contraseña sin toggle show/hide
**Archivo:** `frontend-new/src/app/usuarios/usuarios.component.html`  
**Problema:** Los campos "Contraseña" y "Confirmar Contraseña" no tienen botón de visibilidad. El login y change-password sí lo implementan.  
**Fix:** Agregar a cada campo de password:
```html
<mat-suffix>
  <button mat-icon-button type="button" (click)="hidePassword = !hidePassword" tabindex="0"
          [attr.aria-label]="hidePassword ? 'Mostrar contraseña' : 'Ocultar contraseña'">
    <mat-icon>{{ hidePassword ? 'visibility_off' : 'visibility' }}</mat-icon>
  </button>
</mat-suffix>
```
Y en el TS: `hidePassword = true; hideConfirmPassword = true;`

---

### 2.4 polizas — botón submit activo con form inválido
**Archivo:** `frontend-new/src/app/polizas/polizas.component.html` línea ~211  
**Problema:** `[disabled]="isLoading"` solamente — el botón está activo aunque el formulario tenga errores, provocando errores de servidor innecesarios.  
**Fix:** `[disabled]="isLoading || polizaForm.invalid"`

---

### 2.5 polizas tabla — sin `*matNoDataRow`
**Archivo:** `frontend-new/src/app/polizas/polizas.component.html` dentro del `<table mat-table>`  
**Problema:** Cuando el filtro reduce resultados a 0, la tabla muestra solo el header con espacio vacío debajo — sin feedback.  
**Fix:** Agregar antes del cierre de `</table>`:
```html
<tr class="mat-row" *matNoDataRow>
  <td class="mat-cell" colspan="7">
    <div class="empty-state" style="padding: 32px 0">
      <mat-icon>search_off</mat-icon>
      <span class="empty-title">Sin resultados</span>
      <span class="empty-message">No se encontraron pólizas con ese criterio de búsqueda.</span>
    </div>
  </td>
</tr>
```

---

### 2.6 reclamos tabla — buscador usa `(keyup)` en lugar de `(input)`
**Archivo:** `frontend-new/src/app/reclamos/components/reclamos-dashboard/reclamos-dashboard.component.html` línea ~136  
**Problema:** `(keyup)` no captura Ctrl+V (paste), autofill del navegador, ni IME mobile.  
**Fix:** Cambiar `(keyup)="applyFilter($event)"` → `(input)="applyFilter($event)"`

---

### 2.7 reclamo-detalle — 2 tabs disabled anuncian funcionalidad ausente
**Archivo:** `frontend-new/src/app/reclamos/components/reclamo-detalle/reclamo-detalle.component.html` líneas ~137-165  
**Problema:** Tabs "Histórico" y "Documentos" con `disabled` son peor que no tenerlos — confunden al usuario sobre si la función existe.  
**Fix:** Reemplazar con `*ngIf="false"` o remover hasta tener implementación real.

---

### 2.8 email/teléfono en poliza-detalle no son links clickeables
**Archivo:** `frontend-new/src/app/polizas/poliza-detail-dialog.component.html` líneas ~70-75  
**Problema:** Email y teléfono se muestran como `<span>` — no son accionables.  
**Fix:**
```html
<!-- Email -->
<a href="mailto:{{ poliza.correo }}" class="pd-info-value pd-info-link">{{ poliza.correo }}</a>

<!-- Teléfono -->
<a href="tel:{{ poliza.numeroTelefono }}" class="pd-info-value pd-info-link">{{ poliza.numeroTelefono }}</a>
```
Agregar al SCSS: `.pd-info-link { color: var(--color-primary); text-decoration: none; &:hover { text-decoration: underline; } }`

---

### 2.9 Login mobile — sin breakpoint responsive
**Archivo:** `frontend-new/src/app/auth/login.component.scss`  
**Problema:** `.brand-panel { flex: 0 0 45% }` sin ningún `@media`. En 375px el formulario tiene 206px de ancho — inutilizable.  
**Fix:** Agregar al final del SCSS:
```scss
@media (max-width: 768px) {
  .login-page { flex-direction: column; }
  .brand-panel { flex: 0 0 auto; padding: 32px 24px; min-height: 180px; }
  .brand-features { display: none; }
  .form-panel { flex: 1; }
}
```

---

### 2.10 Sidebar nav-item `slideIn` se re-dispara en cada navegación
**Archivo:** `frontend-new/src/app/app.component.scss` dentro de `.nav-item { animation: slideIn ... }`  
**Problema:** La animación `slideIn 0.3s` se re-ejecuta en cada cambio de ruta porque el `*ngFor` del nav re-renderiza todos los items. Produce efecto de parpadeo/glitch al navegar.  
**Fix:** Remover `animation: slideIn 0.3s ease-out` de `.nav-item` y moverlo a un `:host(:not(.nav-ready)) .nav-item` que solo aplique en la primera carga, o simplemente eliminar la animación por completa del nav-item (ya tiene `transition: var(--transition-fast)` para hover).

---

### 2.11 Año hardcodeado en login footer
**Archivo:** `frontend-new/src/app/auth/login.component.html` y `login.component.ts`  
**Fix HTML:** `© {{ currentYear }} SINSEG`  
**Fix TS:** `currentYear = new Date().getFullYear();`

---

### 2.12 Chips de filtro cobros/reclamos — `[selected]` binding faltante
**Archivos:** `cobros-dashboard.component.html` líneas ~100-115, `reclamos-dashboard.component.html` líneas ~139-175  
**Problema:** Solo se agrega clase CSS `.selected` pero `mat-chip` necesita `[selected]="condition"` para actualizar `aria-selected`.  
**Fix:** Cambiar `[class.selected]="..."` → agregar `[selected]="getFiltroEstado(i) === EstadoCobro.X"` como input binding junto a la clase CSS.

---

## FASE 3 — Gradiente de colores hardcoded → tokens

### 3.1 Stat card icon gradients — cobros-dashboard
**Archivo:** `frontend-new/src/app/cobros/components/cobros-dashboard/cobros-dashboard.component.scss` líneas ~64-95  
**Fix:**
```scss
// Antes:
&.stat-pending  { .stat-icon { background: linear-gradient(135deg, #F59E0B 0%, #D97706 100%); } }
// Después:
&.stat-pending  { .stat-icon { background: linear-gradient(135deg, var(--status-pending) 0%, var(--color-warning) 100%); } }
```
Mismo patrón para `stat-collected` (`--status-active`) y `stat-overdue` (`--status-overdue`).

---

### 3.2 Stat card icon gradients — reclamos-dashboard
**Archivo:** `frontend-new/src/app/reclamos/components/reclamos-dashboard/reclamos-dashboard.component.scss` líneas ~50-93  
**Fix:** Igual que 3.1 + reemplazar blue `#3B82F6` (sin token) → `var(--color-info)`.

---

### 3.3 Botones de acción hardcoded en cobros-dashboard
**Archivo:** `frontend-new/src/app/cobros/components/cobros-dashboard/cobros-dashboard.component.scss` líneas ~205-215  
**Fix:**
```scss
// Antes:
.btn-vencido  { color: #F59E0B !important; }
.btn-reactivar { color: #10B981 !important; }
// Después:
.btn-vencido  { color: var(--color-warning) !important; }
.btn-reactivar { color: var(--color-success) !important; }
```

---

### 3.4 Semáforo dots hardcoded
**Archivo:** `frontend-new/src/app/cobros/components/cobros-dashboard/cobros-dashboard.component.scss` líneas ~227-240  
**Fix:** `#22C55E` → `var(--status-active)`, `#F59E0B` → `var(--status-pending)`, `#EF4444` → `var(--status-overdue)`, `#9CA3AF` → `var(--status-cancelled)`.

---

### 3.5 stat-amount / stat-time en reclamos
**Archivo:** `frontend-new/src/app/reclamos/components/reclamos-dashboard/reclamos-dashboard.component.scss` líneas ~103-110  
**Fix:** `#4CAF50` → `var(--color-success)`, `#9C27B0` → `var(--status-resolved)`.

---

### 3.6 Títulos de página con `color: white` hardcoded
**Archivos:**
- `polizas/polizas.component.scss` línea ~20
- `usuarios/usuarios.component.scss` línea ~14

**Fix:** `color: white` → `color: var(--color-text-primary)`

---

### 3.7 Box-shadow hardcoded en stat-cards / table-cards de dashboards
**Archivos:** `cobros-dashboard.component.scss` y `reclamos-dashboard.component.scss` líneas ~37-56  
**Fix:** `box-shadow: 0 2px 8px rgba(0,0,0,0.1)` → `box-shadow: var(--shadow-sm)`  
`border-radius: 12px` → `border-radius: var(--radius-lg)`

---

## FASE 4 — Accesibilidad

### 4.1 Login — inputs fuera de `mat-form-field` sin aria
**Archivo:** `frontend-new/src/app/auth/login.component.html` líneas ~45-68  
**Problema:** `<input>` custom sin `mat-form-field`, sin `aria-describedby` para errores, sin `for`/`id` entre label e input.  
**Fix:** Agregar `id="email-input"` al input, `for="email-input"` al label; agregar `id="email-error"` al span de error y `aria-describedby="email-error"` al input.

---

### 4.2 Login — botón toggle de contraseña con `tabindex="-1"`
**Archivo:** `frontend-new/src/app/auth/login.component.html` línea ~63  
**Problema:** Inaccessible por teclado.  
**Fix:** Remover `tabindex="-1"` o cambiar a `tabindex="0"`.

---

### 4.3 `mat-icon` decorativo en `<h1>` sin `aria-hidden`
**Archivo:** `frontend-new/src/app/reclamos/components/reclamo-detalle/reclamo-detalle.component.html` línea ~16  
**Fix:** Agregar `aria-hidden="true"` al `mat-icon` dentro del heading.

---

### 4.4 Semáforo dots sin alternativa de texto
**Archivo:** `frontend-new/src/app/cobros/components/cobros-dashboard/cobros-dashboard.component.html` línea ~125  
**Fix:** `<span class="semaforo-dot" [matTooltip]="getSemaforoTooltip(cobro)">` → agregar `role="img" [attr.aria-label]="getSemaforoTooltip(cobro)"`

---

### 4.5 Filas de tabla de usuarios — `Space` no activa la fila
**Archivo:** `frontend-new/src/app/usuarios/usuarios.component.html` línea ~175  
**Problema:** Solo `(keydown.enter)` manejado; `(keydown.space)` falta.  
**Fix:** Agregar `(keydown.space)="selectUser(row)"` junto al handler de Enter.

---

### 4.6 Encabezados de página inconsistentes (`h1` vs `h2`)
**Problema:** Polizas y Usuarios usan `<h2>` como heading de página; Cobros, Reclamos, Emails usan `<h1>`. Dentro de `<router-outlet>` todos deberían ser `<h1>`.  
**Archivos afectados:**
- `polizas/polizas.component.html` línea ~4: cambiar `<h2>` → `<h1>`
- `usuarios/usuarios.component.html` línea ~3: cambiar `<h2>` → `<h1>`

---

### 4.7 Dialogs — botones de cierre sin `aria-label`
**Archivos:**
- `polizas/poliza-detail-dialog.component.html`
- `polizas/polizas.component.html` (ribbon de edición)

**Fix:** Cambiar `matTooltip="Cerrar"` → agregar también `aria-label="Cerrar diálogo"`.

---

## FASE 5 — Micro-interacciones

### 5.1 Count-up animation en stat cards
**Archivos:** `cobros-dashboard.component.html`, `reclamos-dashboard.component.html`  
**Idea:** Animación de 0 → valor real en 600ms al resolver la carga de datos. Implementar como directiva Angular `[countUp]="value"` o pipe con `RxJS interval`.

---

### 5.2 Stagger animation en grid de pólizas cards
**Archivo:** `frontend-new/src/app/polizas/polizas.component.html` + `.scss`  
**Fix:** Agregar `animation-delay: calc({{ i }} * 40ms)` a cada card vía `*ngFor index`.

---

### 5.3 Indicador "escribiendo..." en Chat
**Archivo:** `frontend-new/src/app/chat/components/chat.component.html`  
**Problema:** No hay indicador de que el AI está procesando (tres puntos animados).  
**Fix:** Agregar bloque con tres dots pulsantes cuando `isTyping === true`.

---

### 5.4 Estado de carga en cotizaciones
**Archivo:** `frontend-new/src/app/cotizaciones/cotizaciones.component.html`  
**Problema:** No hay ningún `*ngIf="isLoading"` — la tabla aparece y desaparece instantáneamente.  
**Fix:** Agregar skeleton de tabla o `mat-progress-bar` en la parte superior durante la carga.

---

### 5.5 Email dashboard — loading duplicado
**Archivo:** `frontend-new/src/app/emails/email-dashboard/email-dashboard.component.html` líneas ~47-62  
**Problema:** Skeleton cards Y texto "Cargando..." renderizan simultáneamente.  
**Fix:** Eliminar el bloque `<div class="loading-container" *ngIf="loading">` que es redundante.

---

## Resumen por archivo (para implementación)

| Archivo | Items | Prioridad |
|---|---|---|
| `auth/login.component.scss` + `.html` | 1.1, 2.9, 2.11, 4.1, 4.2 | 🔴 Alta |
| `cobros/cobro-detalle.component.scss` + `.html` | 1.2, 1.3 | 🔴 Alta |
| `emails/email-dashboard.component.scss` + `.html` | 1.4, 2.1, 5.5 | 🔴 Alta |
| `emails/send-email.component.html` | 2.1 | 🟠 Media-alta |
| `emails/email-history.component.html` | 2.1 | 🟠 Media-alta |
| `polizas/upload-polizas.component.html` | 2.1 | 🟠 Media-alta |
| `usuarios/usuarios.component.html` + `.scss` | 2.2, 2.3, 3.6, 4.6 | 🟠 Media-alta |
| `polizas/polizas.component.html` + `.scss` | 2.4, 2.5, 3.6, 4.6 | 🟠 Media-alta |
| `polizas/poliza-detail-dialog.component.html` | 2.8, 4.7 | 🟠 Media |
| `reclamos/reclamos-dashboard.component.html` + `.scss` | 2.6, 3.2, 3.5 | 🟠 Media |
| `reclamos/reclamo-detalle.component.html` | 2.7, 4.3 | 🟠 Media |
| `cobros/cobros-dashboard.component.html` + `.scss` | 2.12, 3.1, 3.3, 3.4, 4.4 | 🟠 Media |
| `app.component.scss` | 2.10 | 🟡 Baja-media |
| `cotizaciones/cotizaciones.component.html` | 5.4 | 🟡 Baja |
| `chat/chat.component.html` | 5.3 | 🟡 Baja |

---

> **Siguiente paso sugerido:** Iniciar con Fase 1 (críticos), luego Fase 2 módulo por módulo comenzando por Login → cobro-detalle → emails. Cada item tiene la información exacta necesaria para implementarlo directamente.
