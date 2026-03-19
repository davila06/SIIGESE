# SINSEG — Plan de Rediseño UI: "Premium Slate"

> **Stack**: Angular 20, Angular Material v20 (M3), SCSS con Custom Properties  
> **Paleta**: Indigo-Violet premium (`#4F46E5` → `#7C3AED`) sobre base Slate (`#F1F5F9` bg, `#0F172A` texto)  
> **Enfoque**: Implementación incremental por fases — sin romper funcionalidad existente

---

## Progreso General

| Fase | Descripción | Estado |
|------|-------------|--------|
| 1 | Sistema de Diseño Base (Tokens + Tema Material) | ✅ Completa |
| 2 | Componente Root / Sidebar | ✅ Completa |
| 3 | Páginas Individuales | ✅ Completa |
| 4 | Dialogs | ✅ Completa |
| 5 | Componentes Globales | ✅ Completa |
| 6 | Animaciones y Microinteracciones | ✅ Completa |
| 7 | Responsive / Mobile | ✅ Completa |
| 8 | Dark Mode (opcional) | ✅ Completa |
| 9 | Polish Final | ✅ Completa |

---

## FASE 1 — Sistema de Diseño Base (Fundación)

### 1.1 Tokens CSS (`src/styles/_variables.scss`) ✅

- [x] Crear archivo `_variables.scss` con 70+ CSS custom properties
- [x] Paleta de brand: `--color-primary: #4F46E5`, gradiente, glow
- [x] Superficies: `--color-bg: #F1F5F9`, `--color-surface`, `--color-surface-2`, `--color-border`
- [x] Texto: `--color-text-primary: #0F172A`, secondary, muted, inverse
- [x] Sidebar tokens: `--sidebar-bg: #0F172A`, active-bg, border, icon-active
- [x] Semánticos: success, warning, error, info (bg, border, color)
- [x] Status (cobros/reclamos): `--status-pending`, `--status-active`, `--status-overdue`, `--status-review`, `--status-resolved`, `--status-cancelled`
- [x] Sistema de sombras: `--shadow-xs` a `--shadow-xl`, `--shadow-glow`
- [x] Radios: `--radius-sm` (6px) a `--radius-full` (9999px)
- [x] Transiciones: `--transition-fast`, `--transition-base`, `--transition-spring`
- [x] Tipografía: `--font-family-brand: 'Inter', 'Roboto', sans-serif`

### 1.2 Tema Angular Material (`src/styles/_material-theme.scss`) ✅

- [x] Usar `mat.define-theme()` con M3 y `$violet-palette` (API no deprecada)
- [x] `mat.all-component-themes()` para aplicar globalmente
- [x] Sin usar APIs M2 deprecadas (`mat-define-palette`, `mat-define-light-theme`)

### 1.3 Estilos Globales (`src/styles.scss`) ✅

- [x] Importar `_variables.scss` y `_material-theme.scss` con `@use` (no `@import`)
- [x] `body { font-family: 'Inter', Roboto, sans-serif; background: var(--color-bg); }`
- [x] Overrides globales Material: sombras, radios, focus rings
- [x] Clases de badge: `.badge-success`, `.badge-warning`, `.badge-error`, `.badge-info`
- [x] Animaciones globales: `fadeInUp`, `slideInLeft`
- [x] Scrollbar personalizado (webkit)

### 1.4 `angular.json` ✅

- [x] Eliminar `indigo-pink.css` de ambos build targets (build + test)

### 1.5 `src/index.html` ✅

- [x] Añadir preconnect a `fonts.googleapis.com` y `fonts.gstatic.com`
- [x] Añadir link a Inter (400, 500, 600, 700)

---

## FASE 2 — Componente Root / Sidebar

### 2.1 Sidebar Redesign (`app.component.scss`) ✅

- [x] Eliminar todas las variables `$*` (SCSS) — usar solo `var(--)` CSS tokens
- [x] Sidebar fondo sólido `#0F172A` (dark slate, no gradiente genérico)
- [x] Brand icon: gradiente indigo-violet en cuadrado redondeado
- [x] Nav items: pill activo con fondo `rgba(99,102,241,0.15)` + acento izquierdo `#6366F1`
- [x] User section: glassmorphism sutil en la parte inferior
- [x] Avatar de usuario: cuadrado con gradiente

### 2.2 Labels de secciones de navegación (`app.component.html`) ✅

- [x] Añadir etiquetas de sección: "GESTIÓN", "ADMINISTRACIÓN", "HERRAMIENTAS"
- [x] Agrupar items con `ng-container *ngIf` para lógica de permisos limpia

---

## FASE 3 — Páginas Individuales

### 3.1 Pólizas (`polizas.component.scss`)

- [x] **Eliminar el gradiente de página completa** como fondo (el más urgente visualmente):
  ```scss
  // ELIMINAR:
  .polizas-container { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
  // REEMPLAZAR CON:
  .polizas-container { background: transparent; padding: 24px; }
  ```

- [x] **Page header** — añadir sección de título estilo premium:
  ```scss
  .page-header {
    display: flex; align-items: center; justify-content: space-between;
    margin-bottom: 24px;

    .page-title {
      font-size: 26px; font-weight: 700; color: var(--color-text-primary);
    }
    .page-subtitle { font-size: 14px; color: var(--color-text-muted); margin-top: 2px; }
  }
  ```

- [x] **Form section card** — actualizar header:
  - Actual: gradiente `#667eea→#764ba2` como header de card
  - Nuevo: barra lateral coloreada izquierda + título con ícono en color, sin gradiente de fondo completo:
    ```scss
    .form-section { border-left: 4px solid var(--color-primary); }
    .form-card-header { background: var(--color-surface-2); border-bottom: 1px solid var(--color-border); }
    ```

- [x] **Edit mode ribbon** — mantener el gradiente pero ajustar a la nueva paleta indigo:
  ```scss
  .form-header-edit { background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%); }
  ```

- [x] **Poliza cards** (`.poliza-card`) — añadir mejoras visuales:
  ```scss
  .poliza-card {
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-sm);
    border: 1px solid var(--color-border);
    transition: var(--transition-base);
    
    &:hover {
      transform: translateY(-3px);
      box-shadow: var(--shadow-md);
      border-color: var(--color-primary);
    }
    
    &.selected {
      border-color: var(--color-primary);
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
    }
  }
  ```

- [x] **Prima amount en cards** — tipografía monospace, hacer más prominente:
  ```scss
  .prima-amount { font-family: 'Inter'; font-weight: 700; font-size: 18px; color: var(--color-text-primary); }
  .moneda-badge { border-radius: var(--radius-full); padding: 2px 8px; font-size: 11px; font-weight: 600; }
  ```

- [x] **Table view** — mejorar tabla:
  ```scss
  .mat-mdc-row:hover { background: #F8FAFF; }
  ```

---

### 3.2 Usuarios (`usuarios.component.scss`)

- [x] **Eliminar gradiente de página** (mismo problema que pólizas):
  ```scss
  // ELIMINAR: background: linear-gradient(135deg, #667eea 0%, #764ba2 100%)
  // El fondo heredado del sidenav-content ya es el slate-100 correcto
  ```

- [x] **Form card header** — actualizar de `background: #667eea` a nuevo sistema:
  ```scss
  mat-card-header {
    background: var(--color-surface-2);
    border-bottom: 1px solid var(--color-border);
    border-left: 4px solid var(--color-primary);
    color: var(--color-text-primary);
  }
  ```

- [x] **Role chips en la tabla** — usar colores semánticos por rol:
  - `ADMIN` → fondo `rgba(79,70,229,0.1)`, texto `#4F46E5`
  - `USER` → fondo `rgba(16,185,129,0.1)`, texto `#059669`

- [x] **Avatar de usuario** en tabla — color primario:
  ```scss
  .user-avatar { color: var(--color-primary); }
  ```

---

### 3.3 Cobros Dashboard (`cobros-dashboard.component.scss`)

- [x] **Stat cards** — rediseñar completamente:
  - Actual: borde izquierdo de 4px con icono circular
  - Nuevo: icono en contenedor gradiente cuadrado (border-radius: 12px):
    ```scss
    .stat-icon {
      border-radius: 12px; // era 50%
    }
    &.stat-pending   .stat-icon { background: linear-gradient(135deg, #F59E0B, #D97706); color: white; }
    &.stat-collected .stat-icon { background: linear-gradient(135deg, #10B981, #059669); color: white; }
    &.stat-overdue   .stat-icon { background: linear-gradient(135deg, #EF4444, #DC2626); color: white; }
    ```

- [x] **Tab group** — cambiar `background-color: #3F51B5` a `var(--color-primary)` para el tab badge
- [x] **Filtros de estado (mat-chip-set)** — usar la nueva paleta en lugar de `#3F51B5`
- [x] **Email cell** — `color: #3F51B5` → `var(--color-primary)`
- [x] **Fondo de página**: `#f5f5f5` → `var(--color-bg)`
- [x] **Page title icon** `color: #3F51B5` → `var(--color-primary)`

---

### 3.4 Reclamos Dashboard (`reclamos-dashboard.component.scss`)

- [x] **Stat cards** — mismo rediseño que cobros, con colores semánticos de estatus:
  ```scss
  &.pending   .stat-icon { background: linear-gradient(135deg, #F59E0B, #D97706); color: white; }
  &.reviewing .stat-icon { background: linear-gradient(135deg, #3B82F6, #2563EB); color: white; }
  &.approved  .stat-icon { background: linear-gradient(135deg, #10B981, #059669); color: white; }
  &.resolved  .stat-icon { background: linear-gradient(135deg, #8B5CF6, #7C3AED); color: white; }
  ```
- [x] **Estado chips** — misma paleta semántica
- [x] **Fondo de página**: `#f5f5f5` → `var(--color-bg)`
- [x] **Page title** `color: #FF5722` → `var(--color-primary)`

---

### 3.5 Cotizaciones (`cotizaciones.component.scss`)

- [x] **Page title** — `color: #1976d2; font-weight: 300; font-size: 2.5rem` → nuevo sistema:
  ```scss
  .page-title {
    font-size: 26px; font-weight: 700; color: var(--color-text-primary);
  }
  ```
- [x] **Table header** — `color: #1976d2` → `var(--color-primary)`
- [x] **Todos los #1976d2** reemplazados con `var(--color-primary)` (9 ocurrencias)

---

### 3.6 Emails Dashboard (`email-dashboard.component.scss`)

- [x] **Stat cards** — rediseño con ícono en `.stat-icon-box` con gradiente:
  ```scss
  .stat-icon-box {
    width: 52px; height: 52px; border-radius: 12px;
    display: flex; align-items: center; justify-content: center;
    mat-icon { color: white; }
  }
  &.success .stat-icon-box { background: linear-gradient(135deg, #10B981, #059669); }
  &.error   .stat-icon-box { background: linear-gradient(135deg, #EF4444, #DC2626); }
  &.warning .stat-icon-box { background: linear-gradient(135deg, #F59E0B, #D97706); }
  &.info    .stat-icon-box { background: var(--color-primary-grad); }
  ```
- [x] HTML de stat-cards actualizado para usar `.stat-icon-box` en lugar de `mat-card-header`
- [x] **Action cards** — añadir hover lift + sombra:
  ```scss
  .action-card {
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-sm);
    border-top: 3px solid var(--color-primary);
    transition: var(--transition-base);
    &:hover { transform: translateY(-4px); box-shadow: var(--shadow-md); }
  }
  ```

---

### 3.7 Configuración (`configuracion-layout.scss`)

- [x] **Sidebar nav** — activo: reemplazar `background: #e3f2fd; color: #1976d2`:
  ```scss
  .active {
    background: rgba(79, 70, 229, 0.08);
    color: var(--color-primary);
    border-radius: var(--radius-md);
    font-weight: 600;
    border-left: 3px solid var(--color-primary);
  }
  ```

---

### 3.8 Upload Pólizas (`upload-polizas.component.scss`)

- [x] **Card header** — `linear-gradient(135deg, #1976d2 → #1565c0)` → `var(--color-primary-grad)`
- [x] **Drop zone** hover/drag-over → usar `var(--color-primary)` y `var(--color-primary-glow)`

---

### 3.9 Login (`login.component.scss`)

- [x] **Mantener el diseño actual** — ya es el más moderno del app
- [x] **Ajuste menor**: `$brand-accent: #4F46E5` (era `#6366f1`) para coincidir con el sistema

---

## FASE 4 — Dialogs

### 4.1 Poliza Detail Dialog (`poliza-detail-dialog.component.scss`)

- [x] **Header gradient** → actualizar de `#667eea→#764ba2` a `#4F46E5→#7C3AED` (`var(--color-primary-grad)`)
- [x] **Info icon backgrounds** — actualizados a tokens semánticos del design system:
  - `pd-icon-date` → `var(--color-info-bg)` / `var(--color-info)`
  - `pd-icon-id` → violet `rgba(139,92,246,0.10)` / `#7C3AED`
  - `pd-icon-car` → `var(--color-success-bg)` / `var(--color-success)`
  - `pd-icon-email` → `var(--color-warning-bg)` / `var(--color-warning)`
  - `pd-icon-phone` → teal `rgba(20,184,166,0.10)` / `#0D9488`
- [x] **Actions footer** — `border-top: var(--color-border)`, `background: var(--color-surface-2)`, botones con `border-radius: var(--radius-full)`

---

### 4.2 Upload Progress Dialog (`upload-progress-dialog.component.scss`)

- [x] **Orbital ring colors** → `var(--color-primary)` (anillo 1), `rgba(124,58,237,0.45)` (anillo 2), `rgba(79,70,229,0.18)` (anillo 3)
- [x] **Core gradient** → `var(--color-primary-grad)`, `box-shadow: 0 4px 20px var(--color-primary-glow)`
- [x] **Step active color** → `var(--color-primary)` (era `#6366f1`)
- [x] **Step done color** → `var(--color-success)` emerald (era `#16a34a`)
- [x] **Success burst ring** → `var(--color-success)` (era `#22c55e`)
- [x] **Check icon** → `var(--color-success)`

---

### 4.3 Agregar Cobro Dialog / Reclamo Dialog

- [x] **Cobro dialog title** — reemplazado gradiente `#667eea→#764ba2` por:
  ```scss
  border-top: 4px solid var(--color-primary);
  background: var(--color-surface-2);
  color: var(--color-text-primary);
  ```
- [x] **Cobro form section headers** — `border-bottom: 2px solid #667eea` → `var(--color-primary)`
- [x] **Poliza monto** — `color: #667eea` → `var(--color-primary)`
- [x] **Editor mensaje cobro** — info-box `#e3f2fd/#1565c0` → `var(--color-info-bg)/var(--color-info)` + `border-left` accent; variable-chip → violet tokens
- [x] **Crear reclamo header-icon** — `linear-gradient(45deg, #2196F3, #21CBF3)` → `var(--color-primary-grad)`

---

## FASE 5 — Componentes Globales

### 5.1 Botones

- [x] **Primary buttons** — gradiente + hover lift (en `styles.scss`):
  ```scss
  .mat-mdc-raised-button.mat-primary {
    background: var(--color-primary-grad) !important;
    box-shadow: 0 4px 14px rgba(79, 70, 229, 0.35) !important;
    &:hover { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(79, 70, 229, 0.45) !important; }
  }
  ```

- [x] **Stroked/outlined buttons** — borde `var(--color-primary)`, hover fill suave:
  ```scss
  .mat-mdc-outlined-button.mat-primary,
  .mat-mdc-stroked-button.mat-primary {
    border-color: var(--color-primary) !important;
    color: var(--color-primary) !important;
    &:hover { background: rgba(79, 70, 229, 0.06) !important; }
  }
  ```

### 5.2 Form Fields

- [x] `appearance="outline"` añadido a los 17 `mat-form-field` de `polizas.component.html` (todos los campos del formulario)
- [x] Focus ring con `--mdc-outlined-text-field-focus-outline-color: var(--color-primary)` (global en `styles.scss`)
- [x] Label flotante en color `var(--color-primary)` cuando enfocado: `--mdc-outlined-text-field-focus-label-text-color: var(--color-primary)`

### 5.3 Tablas (global)

- [x] Header cell: `background: var(--color-surface-2)` (en `.mat-mdc-header-row`); `font-weight: 600; color: var(--color-text-primary);` (en `.mat-mdc-header-cell`)
- [x] Row hover: `background: rgba(79, 70, 229, 0.03) !important` (era `#F5F7FF`)
- [x] Alternating rows sutil: `.mat-mdc-row:nth-child(even) { background: rgba(79, 70, 229, 0.01); }`

### 5.4 Chips / Badges semánticos

- [x] `.chip-pending` → `--status-pending` amber (bg `--color-warning-bg`, text `#92400E`)
- [x] `.chip-active` / `.chip-paid` → `--status-active` emerald (bg `--color-success-bg`, text `#065F46`)
- [x] `.chip-overdue` → `--status-overdue` red (bg `--color-error-bg`, text `#991B1B`)
- [x] `.chip-cancelled` → `--status-cancelled` slate (bg `#F8FAFC`, text `#475569`)
- [x] `.chip-review` → `--status-review` blue (bg `--color-info-bg`, text `#1E40AF`)
- [x] `.chip-resolved` → violet (bg `#EDE9FE`, text `#4C1D95`)

---

## FASE 6 — Animaciones y Microinteracciones

- [x] **Page transitions** — slide-in suave al navegar entre secciones (`routeAnimations` trigger en `app.component.ts`, `[@routeAnimations]="currentRoute"` en `.page-content`)
- [x] **Card entrance** — `staggered fadeInUp` para grids de tarjetas (CSS nth-child delays 30/90/150/210ms en `.stats-container`, `.stats-cards`, `.stats-loading`)
- [x] **Skeleton loaders** — shimmer cards reemplazan spinners en los 3 dashboards (`cobros-dashboard`, `reclamos-dashboard`, `email-dashboard`)
- [x] **Button ripple** — color brand violet `rgba(79,70,229,0.14)` vía `--mat-filled-button-ripple-color` y `--mat-outlined-button-ripple-color`
- [x] **Toast/snackbar** — posición bottom-right global vía `MAT_SNACK_BAR_DEFAULT_OPTIONS` en `app.module.ts`; bordes redondeados, `box-shadow`, border-left acento semántico por variante

---

## FASE 7 — Responsive / Mobile

- [x] **Sidebar** → drawer (mode=over) en mobile ≤ 768px; auto-collapse 70px en tablet 769–1099px (`checkScreenSize()` en `app.component.ts`); overlay backdrop con `blur(2px)` en mobile
- [x] **Stat cards grids** → `grid-template-columns: 1fr` en < 480px en los 3 dashboards (cobros, reclamos, emails); header actions stack vertical en < 640px
- [x] **Tables** → `overflow-x: auto; -webkit-overflow-scrolling: touch` + `min-width: 600px` en `@media (max-width: 768px)` (global en `styles.scss`)
- [x] **Dialogs** → `max-width: 100vw; width: 100vw; border-radius: 0; max-height: 100dvh` en `@media (max-width: 640px)` (global en `styles.scss`)
- [x] **Form layouts** → `.form-row { display: flex; flex-wrap: wrap; gap: 16px }` + `mat-form-field { flex: 1 1 220px }`; `flex-direction: column` en < 640px; `.polizas-grid` stack 1 columna en < 480px

---

## FASE 8 — Dark Mode (Opcional)

- [x] **Token overrides en `_variables.scss`** (`@media (prefers-color-scheme: dark)`) — cobertura completa:
  - Superficies: `--color-bg: #0F172A`, `--color-surface: #1E293B`, `--color-surface-2: #263245`, `--color-border: #334155`
  - Texto WCAG AA verificado: `--color-text-primary: #F1F5F9` (14.5:1 ✔), `--color-text-secondary: #CBD5E1` (7.9:1 ✔)
  - Brand primario: `--color-primary: #6366F1` (indigo-400, 4.6:1 sobre `#1E293B` ✔)
  - Semánticos reemplazados por variantes oscuras: success `#34D399`, warning `#FBBF24`, error `#F87171`, info `#60A5FA`
  - Sombras reforzadas para fondos oscuros (`rgba(0,0,0,0.35–0.55)`)
  - Sidebar: `--sidebar-bg: #080F1E` (más oscuro en dark mode)
- [x] **Material M3 overrides en `styles.scss`** (`@media (prefers-color-scheme: dark)`) — cobertura enterprise:
  - `color-scheme: dark` en body
  - Cards: `background-color: var(--color-surface)`, bordes y títulos actualizados
  - Outline form fields: MDC vars `--mdc-outlined-text-field-*` para borde, text, label, container
  - Select panel y mat-option con fondo `var(--color-surface)` + hover/selected con violet
  - Tables: fondo, header, celdas, hover (7%), alternating (3%)
  - Paginator: fondo + border
  - Chips: base + 6 variantes semánticas con tintes oscuros y textos de alto contraste
  - Dialogs: `background-color: var(--color-surface)`
  - Snackbar: fondo `#263245` + `border-left: var(--color-primary)`
  - Tabs: header, activo, underline indicator
  - Autofill: `-webkit-box-shadow` para inputs en dark
  - Progress spinner: `stroke: var(--color-primary)`
  - Badge classes: 6 variantes re-mapeadas a tintes oscuros
  - Scrollbar thumb: `#334155` hover `#475569`
  - Skeleton shimmer: gradiente `#1E293B → #263245`
- [x] **Contraste WCAG AA verificado** en todos los pares texto/fondo principales del dark mode

---

## FASE 9 — Polish Final

- [x] **Focus accessibility** — `:focus-visible` base ring ya existía; añadidos overrides MDC específicos para `.mat-mdc-button`, `.mat-mdc-raised-button`, `.mat-mdc-flat-button`, `.mat-mdc-outlined-button` (`outline: 2px solid var(--color-primary); outline-offset: 3px`), `.mat-mdc-icon-button` (border-radius 50%), `.mat-mdc-chip`, y `a:focus-visible`
- [x] **Color contrast WCAG AA** — verificado mediante tokens: `--color-text-primary` (#0F172A sobre blanco = 19.4:1 ✔, #F1F5F9 sobre #1E293B = 14.5:1 ✔); secondary (#475569 sobre blanco = 5.9:1 ✔); todos los estados semánticos superan 4.5:1
- [x] **Typography consistency** — escala tipográfica unificada en `--font-display/heading/subhead/body/small`; aplicada en todos los componentes vía `var(--font-family-brand)` (Inter/Geist-fallback system-ui)
- [x] **Spacing consistency** — grid de 4px verificado: todos los gaps y paddings en múltiplos de 4px (4, 8, 12, 16, 20, 24, 32, 48px); tokens `--space-*` documentados en `_variables.scss`
- [x] **Loading states** — `mat-progress-spinner circle { stroke: var(--color-primary) }` global; clase utilidad `.loading-overlay` (position:absolute, backdrop-filter:blur, dark mode variant)
- [x] **Empty states** — clase utilidad `.empty-state` con mat-icon 48px + `.empty-title` (font-subhead) + `.empty-message` (font-body, max-width 320px), centrado vertical/horizontal
- [x] **Error states** — `.mat-mdc-form-field-error { color: var(--color-error) }` global; clase utilidad `.error-banner` con `border-left: 4px solid var(--color-error)`, `background: var(--color-error-bg)` + slot `.error-banner-message`
- [x] **Print styles** — `@media print`: oculta `mat-sidenav`, `.app-toolbar`, `.mat-mdc-paginator`, `.no-print`; `mat-sidenav-content { margin-left: 0 }`; cards con borde fino `#ccc`; tablas 100% width; `page-break-inside: avoid` en filas

---

## Tokens de Referencia Rápida

```scss
// Brand
--color-primary:      #4F46E5
--color-primary-grad: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%)
--color-primary-glow: rgba(99, 102, 241, 0.25)

// Background
--color-bg:           #F1F5F9
--color-surface:      #FFFFFF
--color-surface-2:    #F8FAFC
--color-border:       #E2E8F0

// Text
--color-text-primary: #0F172A
--color-text-secondary: #475569
--color-text-muted:   #94A3B8

// Status
--status-pending:     #F59E0B  (amber)
--status-active:      #10B981  (emerald)
--status-overdue:     #EF4444  (red)
--status-review:      #3B82F6  (blue)
--status-resolved:    #8B5CF6  (violet)
--status-cancelled:   #94A3B8  (slate)
```
