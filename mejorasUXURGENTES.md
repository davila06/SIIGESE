# Mejoras UX Urgentes

## Estado
Finalizado.

## Tareas Críticas
- [x] Corregir texto corrupto (encoding) en navegacion principal.
- [x] Corregir dialogo de detalle de poliza para modo claro/oscuro con design tokens.
- [x] Corregir dialogo agregar cobro (inputs y superficies) para modo claro/oscuro.
- [x] Validar compilacion del frontend.

## Fase Extendida (Priorizada 1-14)
- [x] Estandarizar colores hardcodeados en modulos de reclamos, emails y configuracion.
- [x] Unificar snackbars semanticos a tokens globales.
- [x] Corregir contrastes bajos en login y chips analiticos.
- [x] Mejorar consistencia del header de profile menu en tema claro sin romper dark.
- [x] Normalizar dialogos compartidos (export y email preview) con tokens de tema.
- [x] Revalidar compilacion de frontend tras cambios extendidos.

## Cambios Implementados
- Sidebar y menu principal:
	- Correccion de textos corruptos por encoding (mojibake) usando entidades HTML seguras.
	- Normalizacion de labels y tooltips del menu de navegacion.
- Dialogo detalle de poliza:
	- Eliminados colores hardcodeados claros que rompian modo oscuro.
	- Reemplazo por tokens globales: superficies, bordes y colores de texto.
	- Ajuste de iconografia semantica para mantener contraste en ambos temas.
- Dialogo agregar cobro:
	- Eliminado fondo blanco forzado en campos Material.
	- Reemplazo de grises hardcodeados por tokens de tema.
	- Ajuste de superficie/borde de resumen de poliza y acciones.

## Validacion Tecnica
- Build ejecutado (fase critica): ng build --configuration development.
- Build ejecutado (fase extendida final): ng build --configuration production.
- Resultado: compilacion exitosa en ambas ejecuciones, sin errores bloqueantes.
- Observaciones: se mantienen 2 warnings NG8107 preexistentes en Predictive Analytics (no bloqueantes).

## Notas
Se corrigieron ademas errores de compilacion existentes en Cliente360 para asegurar salida estable de build.
La fase extendida incluyo refactor de contraste y tokens semanticos en modulos de polizas, reclamos, emails, configuracion, analytics y componentes compartidos.
