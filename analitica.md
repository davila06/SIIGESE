# SINSEG — Plan de Analítica de Datos
> Generado: Abril 2026 | Sistema de brokeraje de seguros Costa Rica

---

## Estado de Implementación

| Módulo | Descripción | Estado | Fecha |
|--------|-------------|--------|-------|
| **M0** | Dashboard Ejecutivo | ✅ **COMPLETO** | Abr 2026 |
| **M1** | Analítica de Cobros | ✅ **COMPLETO** | Abr 2026 |
| **M2** | Analítica de Pólizas | ✅ **COMPLETO** | Abr 2026 |
| **M3** | Analítica de Reclamos | ✅ **COMPLETO** | Abr 2026 |
| **M4** | Analítica de Cotizaciones | ✅ **COMPLETO** | Abr 2026 |
| **M5** | Analítica de Emails | ✅ **COMPLETO** | Abr 2026 |
| **M6** | Analítica Operacional | ✅ **COMPLETO** | Abr 2026 |
| **M7** | Analítica Predictiva | ✅ **COMPLETO** | Abr 2026 |
| **M8** | Reportes Exportables | ✅ **COMPLETO** | Abr 2026 |

---

## Contexto del Dominio

SINSEG gestiona el ciclo de vida completo de seguros para un broker costarricense:
pólizas → cobros → reclamos → cotizaciones, con 6 aseguradoras (INS, SAGICOR, ASSA, BCR Seguros, MAPFRE, OTROS) y múltiples tipos de seguro (AUTO, VIDA, HOGAR, EMPRESARIAL). La moneda base es CRC (₡), con soporte USD/EUR.

**Estado actual:** Cero visualizaciones. Existen stat cards con conteos y totales, pero no hay líneas de tiempo, distribuciones, funnels ni predicciones. No hay librería de gráficos importada.

**Librería recomendada:** [`ngx-charts`](https://swimlane.github.io/ngx-charts/) — totalmente compatible con Angular, sin dependencias externas, temas personalizables con CSS custom properties, incluye todos los gráficos necesarios.

---

## MÓDULO 0 — Dashboard Ejecutivo ✅ IMPLEMENTADO
> Vista de 360° del negocio. Primera pantalla que ve el usuario al entrar al sistema.
> **Ruta:** `/analytics` | **Componente:** `executive-dashboard`
> **Build:** ✅ Backend 0 errores · ✅ Angular 0 errores (chunk 84 kB)

### ✅ 0.1 KPIs de Negocio en Tiempo Real — 23 KPIs implementados
Tarjetas grandes con tendencia respecto al mes anterior:

| KPI | Fórmula | Origen |
|-----|---------|--------|
| **Cartera Activa** | COUNT(polizas.EsActivo=true) | Poliza |
| **Prima Mensual Bruta** | SUM(prima) filtrado por frecuencia → normalizado a mensual | Poliza |
| **Tasa de Cobro** | MontoCobrado / MontoTotal × 100 (mes actual) | Cobro |
| **Monto en Riesgo** | SUM(MontoTotal WHERE estado IN [Pendiente, Vencido]) | Cobro |
| **Reclamos Activos** | COUNT(WHERE estado NOT IN [Resuelto, Cerrado]) | Reclamo |
| **Tasa de Conversión** | COUNT(Convertidas) / COUNT(Cotizaciones) × 100 | Cotizacion |
| **Emails Exitosos** | (totalSent - totalFailed) / totalSent × 100 | EmailLog |
| **SLA Reclamos** | % reclamos resueltos antes de FechaLimiteRespuesta | Reclamo |

**KPIs adicionales implementados (lote 2–4):**

| KPI | Descripción |
|-----|-------------|
| PolizasPorRenovar30d | Pólizas con vencimiento en ≤ 30 días |
| ReclamosSinAsignar | Reclamos activos sin agente asignado |
| TiempoPromedioResolucion | Días promedio en cerrar un reclamo |
| TasaMorosidad | % cobros vencidos / cobros totales mes actual |
| RatioPerdidas | MontoAprobado / PrimaMensualBruta × 100 |
| TicketPromedioCobro | MontoTotal promedio por cobro del mes |
| TasaReclamos | Reclamos activos / Pólizas activas × 100 |
| PolizasNuevasMes | Pólizas creadas en el mes actual |
| PrimaPromedioPorPoliza | Prima mensual normalizada / pólizas activas |
| CobrosCanceladosMes | Cobros cancelados en el mes actual |
| PorcentajeCobroUsd | % monto cobrado en USD vs total |
| ReclamosFueraDeSla | Reclamos que superaron FechaLimiteRespuesta |
| ValorPipeline | Suma de prima de cotizaciones pendientes |
| TiempoConversion | Días promedio desde cotización hasta póliza |
| PolizasChurn | Pólizas desactivadas este mes |

### ✅ 0.2 Gráficos del Dashboard Ejecutivo
- ✅ **Línea de cobros**: `ngx-charts-line-chart` multi-series — Cobrado (cyan) vs Esperado (amber) — últimos 12 meses. Groupby mensual desde `cobrosHistorico` con `MontoCobrado` y `MontoTotal`.
- ✅ **Donut aseguradoras**: `ngx-charts-pie-chart` — distribución de prima mensual bruta normalizada por aseguradora. Bug corregido: antes agrupaba por `Frecuencia`, ahora agrupa por `Aseguradora`.
- ✅ **Barras stacked**: `ngx-charts-bar-vertical-stacked` — cobros últimos 6 meses por estado (Cobrado/Pagado · Pendiente · Vencido · Cancelado). Datos en `CobrosEstadoMensual: MultiSeriesChartDto[]`.
- ✅ **Gauge meter**: `ngx-charts-gauge` — Tasa de cobro del mes actual (0-100%), ángulo 240°. Dato: `TasaCobro` ya existente.
- ✅ **Mini sparklines**: `ngx-charts-area-chart` embebidas en las KPI cards de _Tasa de Cobro_ y _Monto en Riesgo_. Datos en `SparklineTasaCobro` y `SparklineMontoRiesgo` (últimos 6 meses desde `cobrosHistorico`).

**Layout del grid:** 2 columnas (`2fr 1fr`), 3 filas:
- Fila 1: Cobros multi-línea (ancho) + Gauge (angosto)
- Fila 2: Barras stacked estado (ancho) + Donut aseguradoras (angosto)
- Fila 3: Panel de alertas (ancho completo)

### ✅ 0.3 Alertas Proactivas (panel lateral derecho)
Lógica de negocio que genera alertas automáticas:

```
🔴 CRÍTICO  — X cobros vencidos > 30 días sin gestión              ✅
🟠 ALERTA   — Y pólizas se vencen en los próximos 15 días          ✅
🟡 AVISO    — Z reclamos de prioridad CRITICA sin asignar           ✅
🟢 INFO     — N cotizaciones llevan > 48h en estado PENDIENTE       ✅
🟠 ALERTA   — W pólizas por renovar en 30 días (churn proactivo)   ✅
🔴 CRÍTICO  — V reclamos fuera de SLA (superaron FechaLimite)       ✅
🟡 AVISO    — U pólizas desactivadas este mes (detección de churn)  ✅
```

**Endpoint implementado:** `GET /api/analytics/dashboard` — consolida datos de Polizas, Cobros, Reclamos, Cotizaciones, EmailLogs.

---

## MÓDULO 1 — Analítica de Cobros ✅ IMPLEMENTADO

> El corazón del negocio: dinero pendiente, flujo de caja, morosidad.

**Estado:** Completamente implementado — backend + frontend compilados sin errores.

### Resumen de implementación

| # | Visualización | Tipo | Estado |
|---|---------------|------|--------|
| 1.1 | Curva de Cobro Mensual | `ngx-charts-line-chart` (Esperado / Cobrado / Vencido) | ✅ |
| 1.2 | Aging Report | `ngx-charts-bar-vertical` (5 buckets por rango de días) | ✅ |
| 1.3 | Distribución Métodos de Pago | `ngx-charts-pie-chart` | ✅ |
| 1.4 | Proyección Cashflow | `ngx-charts-bar-vertical-2d` (Esperado vs Proyectado por semana) | ✅ |
| 1.5 | Heatmap Horario de Pagos | `ngx-charts-heat-map` (7 días × 24 horas, `FechaCobro`) | ✅ |
| 1.6 | Top 10 Deudores | `mat-table` rankeable con routerLink | ✅ |
| 1.7 | Rendimiento por Agente | `ngx-charts-bar-horizontal` + tabla de detalle | ✅ |

**Endpoints agregados:** `GET /api/analytics/cobros/heatmap` · `GET /api/analytics/cobros/por-agente`

**Chunk Angular:** `cobros-analytics-component` 68.88 kB (lazy-loaded)

---

### 1.1 Curva de Cobro Mensual ✅
**Tipo:** Área con línea de referencia  
**Ejes:** X = mes/año (últimos 18 meses), Y = monto  
**Series:**
- Azul: Monto total esperado (generado automáticamente)
- Verde: Monto efectivamente cobrado
- Rojo: Monto que quedó vencido ese mes

**Insight derivado:** Meses con brecha grande entre esperado y cobrado → detectar patrones estacionales costarricenses (Semana Santa, navidad, inicio de año).

**Endpoint:** `GET /api/cobros/rango-fechas?fechaInicio=&fechaFin=` (ya existe, necesita agrupar por mes)

---

### 1.2 Análisis de Antigüedad de Deuda (Aging Report) ✅
**Tipo:** Barras horizontales  
**Lógica:** Cobros en estado Vencido agrupados por días transcurridos desde FechaVencimiento:

| Rango | Color |
|-------|-------|
| 1–15 días | Amarillo |
| 16–30 días | Naranja |
| 31–60 días | Rojo claro |
| 61–90 días | Rojo |
| +90 días | Rojo oscuro (irrecuperable) |

**Métricas secundarias:** N° de clientes en cada rango + monto total + % del total vencido  
**Valor:** Permite priorizar la gestión de cobro. El segmento +90 días requiere acción inmediata o castigo contable.

**Endpoint nuevo:** `GET /api/cobros/aging` — agrupa cobros vencidos por rango de días

---

### 1.3 Distribución por Método de Pago ✅
**Tipo:** Donut chart  
**Series:** Efectivo / Transferencia / Tarjeta Crédito / Tarjeta Débito / Cheque  
**Filtro:** Por rango de fechas, por aseguradora  
**Insight:** Si >80% es efectivo, el broker tiene riesgo operacional (reconciliación manual, riesgo de robo). Si hay muchos cheques, hay riesgo de devoluciones.

---

### 1.4 Proyección de Flujo de Caja (Forecast) ✅
**Tipo:** Barras agrupadas + línea de acumulado  
**Lógica:** Cobros con FechaVencimiento en los próximos 3 meses, agrupados por semana  

```
Semana 1: ₡450,000 esperado | ₡380,000 cobrado (actual)
Semana 2: ₡620,000 esperado | — (futuro)
Semana 3: ₡390,000 esperado | — (futuro)
```

**Valor:** Permite a la dirección prever necesidades de liquidez y planificar recursos de cobranza.

**Endpoint existente:** `GET /api/cobros/proximos-vencer?dias=90` (adaptar para agrupar por semana)

---

### 1.5 Mapa de Calor de Cobros por Día de la Semana × Hora ✅
**Tipo:** Heatmap (grid colorizado)  
**Ejes:** X = hora del día (0-23), Y = día de semana  
**Valor celdas:** COUNT de pagos registrados  
**Insight:** ¿Los lunes a las 9am hay más pagos? ¿Los viernes al mediodía? Optimiza cuándo enviar recordatorios por email.

---

### 1.6 Top 10 Deudores ✅
**Tipo:** Tabla rankeable  
**Columnas:** Cliente, N° pólizas, Monto vencido total, Antigüedad máxima, Último contacto (vía email)  
**Acción rápida:** Botón "Enviar recordatorio" en cada fila  
**Filtros:** Por aseguradora, por tipo de seguro

---

### 1.7 Tasa de Cobro por Agente ✅
**Tipo:** Barras horizontales  
**Series:** Usuario que registró el cobro (campo `UsuarioCobroNombre`) vs total cobrado  
**Métricas:** Monto cobrado / N° cobros procesados / Tiempo promedio entre vencimiento y cobro  
**Insight:** Identifica al agente más efectivo. Permite establecer benchmarks y detectar cuellos de botella.

---

## MÓDULO 2 — Analítica de Pólizas (Portfolio) ✅ IMPLEMENTADO

> Radiografía del portafolio: composición, valor, riesgo de vencimiento.

**Estado:** Completamente implementado — backend + frontend compilados sin errores.

### Resumen de implementación

| # | Visualización | Tipo | Estado |
|---|---------------|------|--------|
| 2.1 | Composición del Portafolio | Donut + Treemap + Barras + Donut + tabla dual | ✅ |
| 2.2 | Comparativa por Aseguradora | Matriz multidimensional + tabla de radar | ✅ |
| 2.3 | Timeline de Vencimientos | Barras apiladas + línea de prima en riesgo + detalle clickeable | ✅ |
| 2.4 | Retención / Renovación | Funnel + línea temporal + vistas por tipo/aseguradora | ✅ |
| 2.5 | Distribución de Prima | Histograma por buckets | ✅ |
| 2.6 | Concentración de Riesgo | Bubble chart + tabla de segmentos | ✅ |

**Endpoints agregados/extendidos:**
- `GET /api/analytics/portfolio` (composición completa, radar, retención, histograma, mapa de riesgo)
- `GET /api/analytics/portfolio/vencimientos?months=12` (timeline con desglose por tipo)
- `GET /api/analytics/portfolio/vencimientos/detalle?month=YYYY-MM` (detalle mensual accionable)

### 2.1 Composición del Portafolio ✅
Cuatro vistas simultáneas (pestañas o grid 2×2):

| Vista | Tipo | Dimensión |
|-------|------|-----------|
| Por Aseguradora | Donut | INS / SAGICOR / ASSA / BCR / MAPFRE |
| Por Tipo de Seguro | Treemap | AUTO / VIDA / HOGAR / EMPRESARIAL |
| Por Modalidad | Barras | BASICO / PLUS / PREMIUM / TOTAL |
| Por Frecuencia | Donut | MENSUAL / TRIMESTRAL / SEMESTRAL / ANUAL |

Cada gráfico muestra tanto N° de pólizas como prima mensual equivalente (normalizada).

---

### 2.2 Radar de Prima por Aseguradora ✅
**Tipo:** Radar/Spider chart  
**Ejes:** 5 dimensiones por aseguradora: prima promedio / N° pólizas / tasa reclamos / tasa cobro / antigüedad promedio  
**Insight:** Comparación multidimensional instantánea de qué aseguradora tiene mejor performance global.

---

### 2.3 Timeline de Vencimientos ✅
**Tipo:** Barras agrupadas por mes  
**X:** Próximos 12 meses  
**Y_izq:** N° de pólizas que vencen  
**Y_der:** Prima acumulada en riesgo de no renovarse  
**Color por tipo:** AUTO=azul, VIDA=verde, HOGAR=naranja, EMPRESARIAL=morado  
**Acciones en hover:** Ver lista de pólizas del mes → permite contactar proactivamente

**Implementación:** click en barra mensual para cargar listado detallado de pólizas por vencer del mes.

---

### 2.4 Análisis de Retención / Renovación ✅
**Tipo:** Funnel + Línea temporal  
**Lógica:**
```
Pólizas vencidas en período X
  → Renovadas (nueva póliza con mismo cliente) = Retención
  → No renovadas = Churn
```
**Métrica clave:** Tasa de retención mensual  
**Insight:** ¿Qué tipo de seguro se renueva más? ¿Qué aseguradora pierde más clientes?

---

### 2.5 Distribución de Prima (Histograma) ✅
**Tipo:** Histograma de frecuencias  
**Lógica:** Agrupa pólizas por rango de prima mensual normalizada:
- ₡0-50k, ₡50k-100k, ₡100k-200k, ₡200k-500k, ₡500k+
**Insight:** Perfil económico del portafolio. Si la mayoría está en el quintil bajo, es un negocio de volumen; si está en el alto, es un negocio de ticket grande.

---

### 2.6 Mapa de Concentración de Riesgo ✅
**Tipo:** Bubble chart  
**Ejes:** X = prima unitaria, Y = monto asegurado (cotizaciones), Tamaño = N° de clientes  
**Color:** Por tipo de seguro  
**Insight:** Identifica si hay sobre-exposición a una aseguradora o segmento específico.

**Nota técnica:** cuando no existe suma asegurada explícita en póliza, se usa proxy anualizado para completar el eje Y sin perder capacidad comparativa.

---

## MÓDULO 3 — Analítica de Reclamos ✅ IMPLEMENTADO

> SLA, aprobaciones, pérdidas, rendimiento del equipo.

**Estado:** Completamente implementado — backend + frontend compilados sin errores.

### Resumen de implementación

| # | Visualización | Tipo | Estado |
|---|---------------|------|--------|
| 3.1 | Funnel + Distribución por Estado | Funnel visual + `ngx-charts-pie-chart` | ✅ |
| 3.2 | Cumplimiento SLA | Gauge + `bar-vertical-stacked` + `bar-horizontal` | ✅ |
| 3.3 | Loss Ratio | `bar-vertical` y `bar-horizontal` segmentado | ✅ |
| 3.4 | Tiempo de Resolución | Barras por tipo, prioridad, agente y aseguradora | ✅ |
| 3.5 | Heatmap Mes x Tipo | Matriz heatmap custom (12+ meses) | ✅ |
| 3.6 | Monto Reclamado vs Aprobado | `ngx-charts-bubble-chart` | ✅ |
| 3.7 | Rendimiento por Agente + Alertas | `mat-table` con sparklines + alertas SLA | ✅ |

**Endpoint implementado:** `GET /api/analytics/reclamos/advanced?months={n}&agenteId={id}&aseguradora={nombre}`  
**Cobertura técnica:** DTOs de analítica avanzada, servicio de dominio, controlador y componente Angular lazy-loaded.

### 3.1 Funnel de Reclamos ✅
**Tipo:** Funnel chart de izquierda a derecha  
**Etapas:**
```
Recibidos → Abiertos → En Revisión → En Proceso → Aprobados → Resueltos
                                                 ↘ Rechazados → Cerrados
```
**Métricas por etapa:** N° reclamos + monto total + tiempo promedio en esa etapa  
**Insight:** ¿Dónde se "atascan" los reclamos? Si hay muchos en "En Revisión" es un cuello de botella operacional.

---

### 3.2 SLA — Cumplimiento de Plazos ✅
**Tipo:** Gauge + Barras por tipo de reclamo  
**Lógica:** `FechaResolucion <= FechaLimiteRespuesta` → cumplido  
```
GLOBAL: 73% dentro de plazo   [Gauge]

Por tipo:
  Siniestro:  65% ████████░░
  Queja:      92% █████████░
  Sugerencia: 98% ██████████
```
**Filtros:** Por mes, por agente asignado, por aseguradora  
**Alerta:** Reclamos que en 24h superarán su FechaLimiteRespuesta

---

### 3.3 Loss Ratio — Ratio de Pérdida ✅
**Tipo:** Barras agrupadas por aseguradora / tipo  
**Fórmula:** `MontoAprobado / PrimalTotal_del_portafolio_de_esa_aseguradora × 100`  
**Comparación:** Vs benchmark industria (~60-70% para vida, ~40-50% para auto en CR)  
**Insight:** Si INS tiene loss ratio del 85%, esa cartera no es rentable para el broker → renegociar comisiones o revisar perfil de cliente.

---

### 3.4 Tiempo Promedio de Resolución ✅
**Tipo:** Box plot o Barras con error (min/max/promedio)  
**Agrupación:**
- Por tipo de reclamo (Siniestro vs Queja)
- Por prioridad (Baja → Crítica)
- Por agente asignado
- Por aseguradora involucrada

**Cálculo:** `FechaResolucion - FechaReclamo` en días hábiles  
**KPI derivado:** Si `Crítico` tarda en promedio 8 días y la ley exige 5, hay riesgo legal.

---

### 3.5 Mapa de Calor de Reclamos por Mes × Tipo ✅
**Tipo:** Heatmap  
**Ejes:** X = mes (12 meses), Y = tipo de reclamo  
**Valor:** N° reclamos / monto reclamado  
**Insight de estacionalidad:** ¿En qué meses hay más siniestros AUTO? (Semana Santa, lluvias en CR). Permite pre-asignar recursos.

---

### 3.6 Análisis Monto Reclamado vs Aprobado ✅
**Tipo:** Scatter plot  
**Eje X:** Monto reclamado  
**Eje Y:** Monto aprobado  
**Color:** Por tipo de reclamo  
**Tamaño del punto:** Duración de resolución en días  
**Insight:** La línea de referencia y = x muestra qué tan generosa es la aseguradora. Puntos muy por debajo = sub-pagos que requieren seguimiento.

---

### 3.7 Rendimiento por Agente ✅
**Tipo:** Tabla con sparklines  
**Columnas:** Agente, Reclamos cerrados, Tiempo promedio, % dentro de SLA, Monto aprobado/reclamado  
**Filtros:** Por mes, por tipo de reclamo  
**Uso:** Evaluación de desempeño + detección de agentes sobrecargados

---

## MÓDULO 4 — Analítica de Cotizaciones (Sales Funnel) ✅ IMPLEMENTADO

> De prospectos a clientes. Conversión, pérdidas, ticket promedio.

**Estado:** Completamente implementado — backend + frontend compilados sin errores.

### Resumen de implementación

| # | Visualización | Tipo | Estado |
|---|---------------|------|--------|
| 4.1 | Funnel de Conversión | Custom funnel visual + `ngx-charts-bar-horizontal` por tipo | ✅ |
| 4.2 | Velocidad de Conversión | `ngx-charts-bar-vertical` histograma de días (5 buckets) | ✅ |
| 4.3 | Pipeline de Valor | `ngx-charts-area-chart-stacked` por mes + KPI summary | ✅ |
| 4.4 | Cotizaciones Perdidas | `ngx-charts-pie-chart` (donut) rechazadas por aseguradora | ✅ |
| 4.5 | Ticket Promedio | `ngx-charts-bar-vertical-2d` TipoSeguro × Modalidad + `mat-table` | ✅ |

**Endpoint actualizado:** `GET /api/analytics/cotizaciones/funnel` — ahora incluye `velocidadBuckets`, `porAseguradora`, `velocidadPromedioDias` calculado con `FechaActualizacion`.

**DTOs agregados:** `VelocidadBucketDto`

**Chunk Angular:** `sales-funnel-component` 81.28 kB (lazy-loaded)

**Correcçión colateral:** Renombradas todas las propiedades `Antigüedad*` → `Antiguedad*` en DTOs, controller, service e interfaces para evitar error NG5002 del parser de templates Angular.

---

### 4.1 Funnel de Conversión ✅
**Tipo:** Funnel vertical con porcentajes de conversión entre etapas

```
Cotizaciones creadas:    100%  ████████████████████
  ↓ 73% continúan
Pendiente → Aprobada:    73%   ██████████████░░░░░░
  ↓ 68% se convierten
Aprobada → Convertida:   49%   ██████████░░░░░░░░░░
  ↓ 51% se rechazan
Rechazadas:              27%   █████░░░░░░░░░░░░░░░
```

**Filtros:** Por tipo de seguro, por aseguradora, por mes  
**Insight:** Si AUTO tiene 80% conversión y VIDA tiene 20%, hay diferencias de propuesta de valor o proceso de venta.

---

### 4.2 Velocidad de Conversión (Sales Velocity) ✅
**Tipo:** Histograma de días  
**Lógica:** `FechaCreacion_Poliza - FechaCotizacion` en días  
**Segmentado por:** Tipo de seguro, modalidad  
**KPI:** Tiempo promedio de cierre = X días  
**Acción:** Cotizaciones que llevan más de `µ + 2σ` días en PENDIENTE → alerta de seguimiento

---

### 4.3 Pipeline de Valor ✅
**Tipo:** Barras + línea de tendencia  
**X:** Mes de creación de la cotización  
**Y_izq:** Suma de `PrimaCotizada` por estado  
**Y_der:** Valor potencial anual (PrimaCotizada × factor_frecuencia)  
**Insight:** ¿Cuánto vale el pipeline de cotizaciones pendientes en prima anual proyectada?

---

### 4.4 Análisis de Cotizaciones Perdidas ✅
**Tipo:** Treemap o Donut de las RECHAZADAS  
**Agrupación:** Por tipo de seguro, por aseguradora propuesta, por monto de prima  
**Insight:** Si el 60% de rechazos son en PREMIUM AUTO con SAGICOR, hay un problema de precio o propuesta en ese segmento.

---

### 4.5 Ticket Promedio (Prima Cotizada) por Segmento ✅
**Tipo:** Barras agrupadas  
**Dimensiones cruzadas:** Tipo de seguro × Modalidad  
**Método de visualización:**

```
                BASICO    PLUS    PREMIUM    TOTAL
AUTO          ₡45k      ₡78k    ₡145k      ₡280k
VIDA          ₡25k      ₡48k    ₡95k       ₡200k
HOGAR         ₡18k      ₡35k    ₡72k       ₡155k
EMPRESARIAL   ₡120k     ₡250k   ₡450k      ₡900k
```

**Uso:** Optimización de pricing y estrategia de upsell.

---

## MÓDULO 5 — Analítica de Comunicaciones (Email) ✅ COMPLETO

> Efectividad del canal de comunicación con los clientes.

### Estado de Implementación

| Sección | Descripción | Estado |
|---------|-------------|--------|
| 5.1 | Tasa de Entrega por Tipo | ✅ Bar chart + tabla de estadísticas |
| 5.2 | Volumen de Emails por Día | ✅ Bar vertical stacked (exitosos/fallidos) |
| 5.3 | Heatmap Horario Óptimo | ✅ Custom CSS 7×24 con colores de calor |
| 5.4 | Correlación Email → Cobro (ROI) | ✅ Barras + comparativa con/sin email |
| 5.5 | Cobertura de Notificaciones | ✅ Donut + desglose numérico |

**DTOs backend nuevos:** `EmailCorrelacionCanalDto`, `CorrelacionBucketDto`  
**Endpoint:** `GET /api/analytics/emails?days=N` — devuelve todos los datos incluída correlación  
**Correlación:** Cruza `EmailLog.ToEmail` (CobroVencido) con `Cobro.CorreoElectronico` + `Cobro.FechaCobro` para calcular buckets de velocidad de pago y uplift del canal

### 5.1 Tasa de Entrega por Tipo de Email
**Tipo:** Barras agrupadas  
**Series:** Generic / CobroVencido / ReclamoRecibido / Bienvenida  
**Métricas:** Enviados, Exitosos, Fallidos, % Éxito  
**Insight:** Si "CobroVencido" tiene alta tasa de fallo, los clientes no están recibiendo los recordatorios → problema de recuperación de cartera.

---

### 5.2 Volumen de Emails por Tiempo
**Tipo:** Línea de área  
**X:** Día/semana  
**Y:** N° emails enviados  
**Color:** Por tipo  
**Anotaciones:** Días con envío masivo automático (`POST /email/automatic/cobros-vencidos`)  
**Insight:** Patrón de uso del canal + detectar picos anómalos.

---

### 5.3 Análisis de Horario Óptimo de Envío
**Tipo:** Heatmap  
**Ejes:** X = hora (0–23), Y = día de semana  
**Valor:** % de éxito de entrega  
**Insight:** ¿A qué hora hay menos rebotes? Los ISPs son más agresivos con el spam en ciertos horarios.

---

### 5.4 Correlación Email → Cobro (Conversión de Canal)
**Lógica:** Para cada cobro vencido que recibió email: ¿cuántos días después se pagó?

```
Recibió email → pagó en < 3 días:  45%  ████████████
Recibió email → pagó en 4-7 días:  28%  ████████
Recibió email → pagó en > 7 días:  15%  ████
Recibió email → nunca pagó:        12%  ███
No recibió email → pagó:            X%  ← baseline
```

**Tipo:** Bar chart de comparación  
**Valor:** Cuantifica el ROI del canal de email en términos de recuperación de cartera.

**Requiere:** Cruzar `EmailLog.ToEmail` + `EmailLog.SentAt` + `Cobro.CorreoElectronico` + `Cobro.FechaCobro` — posible con JOIN en backend.

---

### 5.5 Cobertura de Notificaciones
**Tipo:** Donut  
**Lógica:** De todos los cobros vencidos del mes, ¿qué % tiene `CorreoElectronico` y recibió notificación?  
```
Con email + notificado:         65%
Con email + no notificado:      15%
Sin email registrado:           20%
```
**Acción:** Los "con email + no notificado" → botón para envío masivo.

---

## MÓDULO 6 — Analítica Operacional (Usuarios y Sistema) ✅ IMPLEMENTADO

> Productividad del equipo, uso del sistema, carga de trabajo.
> **Ruta:** `/analytics/operational` | **Componente:** `operational-analytics`
> **Build:** ✅ Backend 0 errores · ✅ Angular 0 errores

### Entregables implementados
- Heatmap semanal por agente (ultimas 12 semanas) usando `ReclamoHistorial`.
- Carga operativa por agente con reclamos activos/alta/criticos, SLA y acciones 12W.
- Riesgo de concentracion critica cuando un agente acumula >= 40% de reclamos activos.
- Actividad de usuarios con ultimo login, dias sin acceso y acciones de ayer.
- Chat IA con sesiones activas/cerradas por semana, satisfaccion y top palabras.
- Rendimiento por endpoint con percentiles P50/P90/P99 via middleware en memoria.
- Alertas operacionales para concentracion, inactividad y satisfaccion de chat.

### 6.1 Actividad por Agente (Heatmap de Productividad)
**Tipo:** Heatmap  
**Ejes:** X = semana, Y = agente  
**Valor:** N° acciones (cobros registrados + reclamos actualizados + cotizaciones creadas)  
**Estado:** ✅ Implementado (basado en acciones del historial de reclamos por agente asignado).  
**Insight:** Detecta agentes inactivos, sobrecargados, y vacaciones no gestionadas.

---

### 6.2 Distribución de Carga de Reclamos
**Tipo:** Barras horizontales  
**Lógica:** Reclamos ACTIVOS asignados por agente  
**Segmentado por prioridad:** Alta/Crítica en rojo  
**Alerta:** Si un agente concentra carga critica, se marca riesgo operacional.  
**Uso:** Balanceo de carga y decisiones de asignación.

---

### 6.3 Sesiones del Chat IA
**Tipo:** Línea + tabla  
**Métricas:**
- Sesiones activas / cerradas por semana
- Mensajes promedio por sesión
- Tiempo promedio de procesamiento (`ProcessingTimeMs`)
- Satisfacción: % de mensajes con `ReactionScore = 1` vs `-1`
- Mensajes más frecuentes → análisis de tópicos (NLP básico: top 20 palabras)

**Valor:** Mide si el chat IA está resolviendo dudas o frustrando usuarios.

---

### 6.4 Último Acceso y Actividad de Usuarios
**Tipo:** Tabla sorteable  
**Columnas:** Usuario, Rol, Último login, Días sin acceso, N° acciones ayer  
**Alertas:** Usuarios sin acceso > 30 días → candidatos a desactivar  
**Uso:** Gestión de seguridad + licencias + onboarding.

---

### 6.5 Tiempos de Respuesta del Sistema
**Tipo:** Línea de percentiles  
**Implementado con logging de performance en backend (middleware):**  
- P50, P90, P99 de tiempo de respuesta por endpoint
- Picos correlacionados con volumen de datos

---

## MÓDULO 7 — Analítica Predictiva (Machine Learning Ligero) ✅ COMPLETO

> Algoritmos simples implementables sin ML framework. Basados en heurísticas estadísticas.

### Estado de Implementación

| Sección | Descripción | Algoritmo | Estado |
|---------|-------------|-----------|--------|
| 7.1 | Score de Riesgo de Morosidad | Scoring por puntos (5 factores, 0–100) | ✅ Donut distribución + tabla top 10 |
| 7.2 | Predicción de Reclamos por Temporada | Promedio móvil YoY (3 años) | ✅ Bar histórico 12m + tabla forecast 3m |
| 7.3 | Detección de Anomalías en Cobros | Z-score ≥68σ (agrupado por póliza) | ✅ Tabla con badges de tipo y z-score |
| 7.4 | Renovación Proactiva (Lead Scoring) | Scoring por puntos (5 criterios) | ✅ Donut prioridad + tabla top 20 leads |
| 7.5 | Forecast de Prima Mensual | Suma programada × tasa histórica 6m + IC 90% | ✅ Bar agrupado 3m hist+3m proyectados + KPIs |

**Endpoint:** `GET /api/analytics/predictivo` — devuelve los 5 secciones en un solo response  
**Ruta Angular:** `/analytics/predictivo` | **Componente:** `predictive-analytics` (lazy-loaded)  
**Algoritmos implementados (sin ML framework):**
- Scoring lineal por puntos con factores ponderados
- Promedio móvil sobre 3 años de historial mensual
- Z-score por póliza + fallback a desviación global  
- Intervalo de confianza 90%: ±1.645σ sobre varianza mensual 6m

### 7.1 Score de Riesgo de Morosidad por Póliza
**Algoritmo:** Regresión logística simple o scoring por puntos  
**Variables input:**
- Historial de pagos (% veces que estuvo vencido)
- Antigüedad de la póliza como cliente
- Tipo de seguro (VIDA tiene menor tasa histórica de morosidad)
- Frecuencia de cobro (MENSUAL es más propenso que ANUAL)
- Aseguradora

**Output:** Score 0-100 + categoría: Verde/Amarillo/Rojo  
**Implementación:** Calcular en backend al generar cobros automáticos. Mostrar en columna de cobros-dashboard.

---

### 7.2 Predicción de Reclamos por Temporada
**Algoritmo:** Promedio móvil + detección de estacionalidad  
**Lógica:** Basado en histórico de `FechaReclamo` por tipo, predecir volumen del próximo mes  
**Output:** "Se esperan ~23 reclamos AUTO en abril (mismo período año anterior: 19)"  
**Valor:** Pre-asignación de recursos humanos.

---

### 7.3 Detección de Anomalías en Cobros
**Algoritmo:** Z-score simple  
**Trigger:** Si un cobro tiene `MontoTotal` que es `>µ + 3σ` del promedio histórico del cliente → alerta de posible error de digitación o fraude  
**Output:** Notificación en panel de alertas con botón "Revisar cobro"

---

### 7.4 Renovación Proactiva (Lead Scoring)
**Algoritmo:** Scoring por puntos  
**Criterios:**
- Póliza vence en < 60 días (+30 pts)
- Cliente nunca ha tenido cobro vencido (+20 pts)
- Tiene más de 1 póliza activa (+15 pts)
- Respondió al último email (correlación EmailLog) (+10 pts)
- Tipo VIDA o EMPRESARIAL (+10 pts) ← mayor ticket

**Output:** Lista "Top 20 pólizas para renovación activa esta semana" ordenada por score  
**Acción directa:** Botón para enviar email de renovación personalizado.

---

### 7.5 Forecast de Prima Mensual Proyectada
**Algoritmo:** Suma de cobros programados + tasa histórica de cobro  
```
Mes próximo esperado:  ₡4,820,000
  - Cobros programados:   ₡5,200,000
  - × Tasa cobro 6m:      92.7%
  = Proyección realista:  ₡4,820,000

IC 90%: [₡4,400,000 — ₡5,100,000]
```
**Implementación:** Cálculo serverside usando datos de `GET /api/cobros/proximos-vencer`.

---

## MÓDULO 8 — Reportes Exportables ✅ IMPLEMENTADO

> Documentos generables bajo demand para presentar a dirección o aseguradoras.
> **Ruta:** `/analytics/reportes` | **Componente:** `reportes-exportables`

### Resumen de implementación

| # | Reporte | Formato | Estado |
|---|---------|---------|--------|
| 8.1 | Cartera por Aseguradora | Excel + PDF | ✅ |
| 8.2 | Morosidad | Excel + envío por email con adjunto | ✅ |
| 8.3 | Reclamos (SLA Compliance) | PDF ejecutivo | ✅ |
| 8.4 | Estado del Portafolio | PDF 1 página + envío mensual a Admin | ✅ |

**Automatización mensual implementada:**
- Job en backend (hosted service) con ejecución periódica e idempotencia por mes
- Día 1 de cada mes: genera y distribuye automáticamente el reporte 8.4 a usuarios Admin
- Día 1 de cada mes: envía reporte 8.2 a destinatarios de gerencia de cobros configurables en `Reports:MorosidadRecipients`

**Endpoints M8 implementados (`/api/analytics/reportes/...`):**
- `GET /cartera-aseguradora/excel?aseguradora=`
- `GET /cartera-aseguradora/pdf?aseguradora=`
- `GET /morosidad/excel`
- `POST /morosidad/email`
- `GET /reclamos-sla/pdf`
- `GET /estado-portafolio/pdf`
- `POST /estado-portafolio/email-admins`

### 8.1 Reporte de Cartera por Aseguradora
**Formato:** PDF + Excel  
**Contenido:** Todas las pólizas de una aseguradora con estado de cobros actual, reclamos pendientes, tasa de cobro  
**Frecuencia:** Mensual automático

### 8.2 Reporte de Morosidad
**Formato:** Excel ordenado por monto descendente  
**Contenido:** Todos los cobros vencidos con aging, datos de contacto del cliente, historial de cobros  
**Acción:** Adjuntar automáticamente en email para gerencia de cobros

### 8.3 Reporte de Reclamos (SLA Compliance)
**Formato:** PDF ejecutivo  
**Contenido:** N° reclamos por estado, % dentro de SLA, tiempo promedio resolución, monto total aprobado vs reclamado  

### 8.4 Estado del Portafolio (Dashboard PDF)
**Formato:** PDF de 1 página con todos los KPIs del módulo 0  
**Frecuencia:** Generado automáticamente el primer día de cada mes  
**Distribución:** Enviado por email a usuarios con rol Admin

---

## Stack Tecnológico Recomendado

### Frontend
| Librería | Uso | Instalación |
|----------|-----|-------------|
| `ngx-charts` | Todos los gráficos de barras, líneas, donuts, treemaps, gauge | `npm install @swimlane/ngx-charts` |
| `d3` (peer dep) | Escala de colores, formateo de ejes | `npm install d3` |
| `ng2-pdf-viewer` | Preview de reportes PDF | `npm install ng2-pdf-viewer` |
| `exceljs` | Export a Excel desde frontend | `npm install exceljs` |

### Backend (nuevos endpoints requeridos)
| Endpoint | Propósito | Módulo |
|----------|-----------|--------|
| `GET /api/dashboard/stats` | KPIs consolidados módulo 0 | Dashboard |
| `GET /api/cobros/aging` | Antigüedad de deuda agrupada | Cobros |
| `GET /api/cobros/monthly-trend?months=18` | Serie temporal mensual | Cobros |
| `GET /api/cobros/by-payment-method` | Distribución por método pago | Cobros |
| `GET /api/cobros/cashflow-forecast?weeks=12` | Proyección semanal | Cobros |
| `GET /api/polizas/expiring-timeline?months=12` | Vencimientos por mes | Pólizas |
| `GET /api/polizas/portfolio-distribution` | Composición del portafolio | Pólizas |
| `GET /api/reclamos/funnel-stats` | Datos para funnel de reclamos | Reclamos |
| `GET /api/reclamos/sla-report` | Cumplimiento de plazos | Reclamos |
| `GET /api/reclamos/resolution-time` | Tiempos promedio por segmento | Reclamos |
| `GET /api/cotizaciones/conversion-funnel` | Embudo de ventas | Cotizaciones |
| `GET /api/cotizaciones/pipeline-value` | Valor de pipeline por mes | Cotizaciones |
| `GET /api/email/conversion-correlation` | Email → cobro pagado | Emails |
| `GET /api/analytics/reportes/cartera-aseguradora/excel` | Cartera por aseguradora (Excel) | Reportes |
| `GET /api/analytics/reportes/cartera-aseguradora/pdf` | Cartera por aseguradora (PDF) | Reportes |
| `GET /api/analytics/reportes/morosidad/excel` | Morosidad exportable (Excel) | Reportes |
| `POST /api/analytics/reportes/morosidad/email` | Envío de morosidad por email | Reportes |
| `GET /api/analytics/reportes/reclamos-sla/pdf` | Reporte ejecutivo SLA (PDF) | Reportes |
| `GET /api/analytics/reportes/estado-portafolio/pdf` | Estado de portafolio (PDF) | Reportes |
| `POST /api/analytics/reportes/estado-portafolio/email-admins` | Distribución a Admins | Reportes |

---

## Arquitectura del Módulo de Analítica

### Nueva ruta: `/analytics`
```
analytics/
  ├── analytics.module.ts
  ├── analytics-routing.module.ts
  ├── components/
  │   ├── executive-dashboard/     (módulo 0)
  │   ├── cobros-analytics/        (módulo 1)
  │   ├── portfolio-analytics/     (módulo 2)
  │   ├── reclamos-analytics/      (módulo 3)
  │   ├── sales-funnel/            (módulo 4)
  │   ├── email-analytics/         (módulo 5)
  │   ├── operational-analytics/   (módulo 6)
  │   └── reportes-exportables/    (módulo 8)
  ├── services/
  │   └── analytics.service.ts     (llama a todos los endpoints agregados)
  └── shared/
      ├── chart-card/              (componente reutilizable: card + gráfico + filtros)
      └── date-range-picker/       (selector de período)
```

### Servicio Compartido
```typescript
// analytics.service.ts
@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  getDashboardStats(): Observable<DashboardStats> { ... }
  getCobrosAgingReport(): Observable<AgingBucket[]> { ... }
  getMonthlyTrend(months: number): Observable<MonthlyDataPoint[]> { ... }
  getPortfolioDistribution(): Observable<PortfolioDistribution> { ... }
  getReclamosFunnel(): Observable<FunnelStage[]> { ... }
  getCotizacionesConversionFunnel(): Observable<ConversionFunnel> { ... }
  // ...
}
```

### Componente Reutilizable `chart-card`
```html
<app-chart-card
  [title]="'Cobros por Mes'"
  [loading]="loading"
  [dateRangeEnabled]="true"
  (dateRangeChange)="onRangeChange($event)">
  
  <ngx-charts-area-chart [results]="data" ...></ngx-charts-area-chart>

</app-chart-card>
```

---

## Mapa de Colores para Gráficos

> Consistente con el tema neon del sistema (`--color-primary: #00E5FF`)

| Dimensión | Color | Token CSS |
|-----------|-------|-----------|
| Cobrado / Activo | `#00E5FF` | `--color-primary` |
| Pendiente | `#F59E0B` | `--status-pending` |
| Vencido / Riesgo | `#EF4444` | `--status-overdue` |
| Resuelto / Bueno | `#10B981` | `--status-active` |
| Cancelado | `#6B7280` | `--status-cancelled` |
| En Proceso | `#8B5CF6` | `--status-resolved` |
| Información | `#3B82F6` | `--color-info` |
| Aseguradoras (paleta) | Gradiente de cyan a violet | custom 6-color ramp |

---

## Priorización de Implementación

| Fase | Módulos | Estimación | Valor de Negocio |
|------|---------|------------|------------------|
| **Sprint 1** | 0 (Dashboard Ejecutivo) + 1.2 (Aging) + 1.4 (Forecast) | 2 semanas | 🔴 Crítico — dinero |
| **Sprint 2** | 2 (Portfolio) + 4 (Sales Funnel) | 2 semanas | 🟠 Alto — rentabilidad |
| **Sprint 3** | 3 (Reclamos SLA) + 5 (Email) | 2 semanas | 🟠 Alto — operación |
| **Sprint 4** | 6 (Operational) + 7 (Predictivo) | 3 semanas | 🟡 Medio — eficiencia |
| **Sprint 5** | 8 (Reportes PDF/Excel) | 2 semanas | 🟡 Medio — presentación |

---

## Quick Wins (implementables en < 1 día sin nuevos endpoints)

Estas analíticas se pueden hacer con los datos que **ya retornan los endpoints existentes**:

1. **Donut de cobros por estado** — usa `GET /api/cobros/stats` (CobroStatsDto ya tiene los conteos)
2. **Comparación monto esperado vs cobrado** — usa los mismos stats
3. **Barras de reclamos por prioridad** — usa `GET /api/reclamos/stats` 
4. **Donut de cotizaciones por estado** — `GET /api/cotizaciones` + agrupar en frontend
5. **Tabla de pólizas por vencer (30 días)** — `GET /api/cobros/proximos-vencer?dias=30`
6. **Tasa de email éxito** — `GET /api/email/stats`

> Estos 6 quick wins pueden implementarse como un "mini-dashboard" agregando una sección de gráficos a cada dashboard existente, antes de construir el módulo `/analytics` completo.

---

> **Siguiente paso sugerido:** Instalar `ngx-charts`, implementar el componente `chart-card` reutilizable, y agregar los 6 quick wins a los dashboards existentes. Esto da valor inmediato y valida la integración visual antes de construir endpoints nuevos.
