# Frontend Accessibility Guide

## Objetivo
Estandarizar accesibilidad de interfaz para Angular + Material con foco en navegacion por teclado, icon buttons y consistencia semantica.

## Reglas obligatorias
1. Todo boton solo-icono debe incluir `aria-label` contextual y descriptivo.
2. Los textos de accesibilidad deben usar un unico idioma del producto (Espanol).
3. Todo control interactivo debe tener estado de foco visible (`:focus-visible`).
4. Tooltips no sustituyen `aria-label`; ambos pueden coexistir.
5. En acciones destructivas, `aria-label` debe incluir entidad objetivo (ej: numero de reclamo/cobro).

## Patrones recomendados
- Cambiar estado:
  - `aria-label="Cambiar estado del reclamo 12345"`
- Registrar pago:
  - `aria-label="Registrar pago del cobro RC-001"`
- Mostrar/Ocultar password:
  - `aria-label` dinamico segun estado (`Mostrar ...` / `Ocultar ...`)
  - `aria-pressed` acorde al estado visible.

## Checklist por PR
- [ ] No hay botones icon-only sin `aria-label`.
- [ ] No hay `aria-label` en ingles.
- [ ] Se valida foco visible en teclado en botones, links, chips y menu actions.
- [ ] Las vistas principales no muestran textos mojibake.

## Alcance inicial aplicado
- Change Password
- Cobros Dashboard
- Reclamos Dashboard
- Executive Dashboard y Polizas (correcciones de texto visible)

## Alcance ampliado (abril 2026)
- Predictive Analytics (normalización de textos y mojibake visible)
- App shell (aria-label en toggles de navegación)
- Chat (aria-label estandarizado en acciones icon-only)
- Agenda Inteligente / Cobros Analytics / Reclamos Analytics (acciones rápidas con aria-label contextual)
