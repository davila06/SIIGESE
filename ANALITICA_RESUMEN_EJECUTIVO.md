# SINSEG — Analítica de Datos: Resumen Ejecutivo
> Abril 2026 | Sistema de brokeraje de seguros · Costa Rica

---

## Por qué importa la analítica en un broker de seguros

Un broker no fabrica seguros — **vende tiempo, confianza y conocimiento**. La analítica transforma los datos de operación diaria (cobros, pólizas, reclamos, emails) en ventaja competitiva concreta: saber antes que el cliente que su póliza va a caducar, saber antes que el contador que el flujo de caja del próximo mes será bajo, y saber antes que la gerencia que un agente está al borde del agotamiento. A continuación, los nueve módulos implementados y lo que significan para el negocio.

---

## M0 · Dashboard Ejecutivo — La "señal vital" del negocio

**Qué es:** Vista única con 23 KPIs en tiempo real, cinco gráficos interactivos y un panel de alertas proactivas con código de colores (rojo/naranja/amarillo/verde).

**Valor convencional:** Cualquier gerente puede ver cómo va el mes sin llamar a nadie.

**Valor fuera de lo obvio:** Las alertas proactivas son el verdadero diferenciador. El sistema *no espera que alguien consulte* — avisa automáticamente: cobros vencidos sin gestión > 30 días, pólizas por caducar en 15 días, reclamos críticos sin asignar. Esto convierte el software en un *asistente activo*, no en un repositorio pasivo. Cada alerta no atendida es dinero medible: una póliza no renovada a tiempo es prima perdida para siempre.

---

## M1 · Analítica de Cobros — Donde vive el flujo de caja

**Qué es:** Siete visualizaciones sobre el ciclo de cobro: curva mensual esperado vs cobrado, aging report (antigüedad de deuda), métodos de pago, proyección de cash flow a 3 meses, heatmap de horarios de pago, top 10 deudores y rendimiento por agente.

**Valor convencional:** Saber cuánto se debe y quién lo debe.

**Valor fuera de lo obvio:** El *heatmap de horarios de pago* (7 días × 24 horas) revela cuándo paga realmente el cliente costarricense — y permite sincronizar los recordatorios de cobro con esos momentos exactos, aumentando la tasa de conversión del email sin enviar un solo correo adicional. El *Aging Report* con cinco colores por antigüedad es también una herramienta contable: el segmento +90 días debe pasar a provisión de incobrables, y ese dato hoy no existe de forma visual para nadie en la empresa.

---

## M2 · Analítica de Pólizas — Radiografía del portafolio

**Qué es:** Seis vistas sobre la composición del portafolio: distribución por aseguradora/tipo/modalidad/frecuencia, radar comparativo de aseguradoras, timeline de vencimientos a 12 meses, análisis de retención/churn, histograma de prima y mapa de concentración de riesgo.

**Valor convencional:** Saber qué pólizas hay y cuántas.

**Valor fuera de lo obvio:** El *radar comparativo de aseguradoras* cruza cinco dimensiones simultáneas (prima promedio, volumen, tasa de reclamos, tasa de cobro, antigüedad). Esto le da al broker un argumento visual y objetivo para renegociar comisiones — o para dejar de recomendar activamente a una aseguradora con bajo rendimiento. El *mapa de concentración de riesgo* detecta sobre-exposición: si el 60 % de la cartera está en una sola aseguradora, un cambio regulatorio en ella puede hundir el negocio completo.

---

## M3 · Analítica de Reclamos — Cumplimiento, costos y reputación

**Qué es:** Siete visualizaciones sobre el ciclo de reclamos: funnel por etapa, cumplimiento de SLA, loss ratio por aseguradora, tiempos de resolución, heatmap estacional, análisis monto reclamado vs aprobado y rendimiento por agente.

**Valor convencional:** Saber cuántos reclamos hay abiertos.

**Valor fuera de lo obvio:** El *Loss Ratio por aseguradora* (monto aprobado / prima total de esa cartera) expone algo que pocas empresas traquean: si INS tiene un loss ratio de 85 % mientras el benchmark de mercado es 50 %, esa cartera no es rentable — y el broker está asumiendo costos operativos de reclamos sin ganancia real. El *SLA compliance* tiene dimensión legal en Costa Rica: los plazos de respuesta son regulados, y un reclamo crítico resuelto fuera de plazo puede derivar en multas de SUGESE. El módulo convierte eso en un semáforo visible antes de que ocurra.

---

## M4 · Analítica de Cotizaciones — El embudo de ventas

**Qué es:** Cinco visualizaciones sobre conversión: funnel desde cotización hasta póliza activa, velocidad de cierre, valor del pipeline, análisis de cotizaciones perdidas y ticket promedio por tipo de seguro × modalidad.

**Valor convencional:** Ver cuántas cotizaciones se convirtieron en ventas.

**Valor fuera de lo obvio:** La *velocidad de conversión* (histograma de días hasta cierre) más la desviación estándar permite definir un umbral automático de alerta: cualquier cotización que supere µ + 2σ días sin movimiento recibe una notificación de seguimiento. Esto implementa un CRM de ventas sin necesitar Salesforce. El *pipeline de valor* proyecta la prima anual de todo lo pendiente, dándole a la dirección un número concreto para hablar de forecast de ingresos con las aseguradoras.

---

## M5 · Analítica de Comunicaciones — El canal medido

**Qué es:** Cinco análisis sobre el canal de email: tasa de entrega por tipo, volumen temporal, heatmap de horario óptimo, correlación email→cobro (ROI), y cobertura de notificaciones.

**Valor convencional:** Saber si los emails llegaron.

**Valor fuera de lo obvio:** La *correlación email → cobro* es probablemente el cálculo más valioso del módulo. Mide cuántos cobros vencidos que recibieron un recordatorio por email se pagaron en < 3 días, 4-7 días, > 7 días, o nunca — versus cobros sin email. Ese porcentaje de uplift cuantifica el ROI del canal en dinero real recuperado, y justifica (o no justifica) la inversión en el servicio de email. Si el 45 % paga en < 3 días después del email y solo el 12 % paga sin él, el sistema de email tiene un retorno demostrable. Ese argumento es vendible internamente para cualquier decisión de inversión tecnológica futura.

---

## M6 · Analítica Operacional — El equipo como sistema

**Qué es:** Cinco vistas sobre el rendimiento interno: heatmap de productividad por agente (12 semanas), distribución de carga de reclamos, sesiones del chat IA, actividad de usuarios y tiempos de respuesta del sistema.

**Valor convencional:** Ver si los agentes están trabajando.

**Valor fuera de lo obvio:** La *detección de concentración crítica* (un agente con ≥ 40 % de reclamos activos) no es solo un dato de RRHH — es un riesgo operacional real. Si ese agente enferma o renuncia, el SLA de medio portafolio colapsa. El *análisis del chat IA* revela las preguntas más frecuentes de los agentes: si el 30 % de sesiones pregunta lo mismo, esa fricción debería resolverse con documentación o automatización, no con respuestas repetitivas del chat.

---

## M7 · Analítica Predictiva — Ver el futuro con los datos del pasado

**Qué es:** Cinco modelos estadísticos ligeros (sin framework de ML): score de riesgo de morosidad, predicción estacional de reclamos, detección de anomalías en cobros, lead scoring de renovación y forecast de prima mensual con intervalo de confianza.

**Valor convencional:** Tener proyecciones.

**Valor fuera de lo obvio:** El *score de morosidad por póliza* (0-100, calculado por cinco factores ponderados) puede integrarse directamente en la decisión de ofrecer descuento por pago anticipado: un cliente con score ≥ 80 no necesita incentivo para pagar; uno con score < 30 sí. El *lead scoring de renovación* prioriza las 20 pólizas más valiosas a renovar *esta semana*, concentrando el esfuerzo del agente donde hay mayor probabilidad de retención y mayor ticket. El *forecast con intervalo de confianza al 90 %* da a la dirección un rango realista (no un número puntual falso) para compromisos frente a las aseguradoras.

---

## M8 · Reportes Exportables — La inteligencia que sale del sistema

**Qué es:** Cuatro reportes en PDF/Excel generados bajo demanda o automáticamente el día 1 de cada mes: cartera por aseguradora, morosidad, SLA de reclamos y estado del portafolio.

**Valor convencional:** Tener documentos para presentar en reuniones.

**Valor fuera de lo obvio:** La automatización del reporte mensual de portafolio (generado y enviado por email a los Admin el día 1 de cada mes) elimina una tarea administrativa recurrente y crea un *artefacto de gobernanza*: hay registro histórico de cómo estaba el negocio cada mes. Con 12 meses de reportes acumulados, la empresa tiene información estructurada para auditorías, para negociar con aseguradoras y para argumentar su crecimiento frente a posibles inversores o frente a bancos. Es la diferencia entre un broker que "siente" que creció y uno que puede *demostrarlo con números*.

---

## M9 · Cliente 360° — La ficha completa del asegurado

**Qué es:** Vista unificada de un cliente buscable por cédula o nombre. Consolida en una sola pantalla: todas sus pólizas activas/inactivas, últimos 20 cobros con estado y días vencidos, reclamos históricos, LTV (valor de vida del cliente), prima mensual activa, score de lealtad (0–100) y score de riesgo (0–100) con categoría semafórica Verde/Amarillo/Rojo. Incluye una mini-gráfica del LTV acumulado mes a mes.

**Valor convencional:** Ver todo lo de un cliente en un lugar sin abrir cuatro pantallas distintas.

**Valor fuera de lo obvio:** Los dos scores calculados automáticamente son los elementos más potentes. El *score de lealtad* (basado en antigüedad, historial de pago puntual, número de pólizas, ausencia de reclamos) permite al agente decidir en segundos si vale la pena ofrecer un descuento de retención o si el cliente ya es fiel por convicción. El *score de riesgo* identifica clientes con alta tasa de vencidos o cobros sin gestión > 30 días — candidatos a cobro preventivo antes de que escalen. El LTV calculado (`total cobrado − total reclamado`) responde en tiempo real la pregunta que ningún broker de tamaño mediano puede contestar hoy sin una hoja de cálculo: *¿este cliente vale la pena?*

---

## M14 · Agenda Inteligente — El día del agente, priorizado

**Qué es:** Vista personal del agente que genera automáticamente su lista de tareas del día, ordenada por urgencia en cinco secciones con código de colores: (1) cobros vencidos de los clientes con mayor prima acumulada, (2) pólizas que vencen hoy o mañana, (3) reclamos críticos/altos activos, (4) "leads calientes" con score ≥ 50 por vencer en 7 días, (5) cotizaciones pendientes sin movimiento por más de 5 días. Cada ítem incluye un enlace directo al registro correspondiente.

**Valor convencional:** Tener una lista de pendientes organizada.

**Valor fuera de lo obvio:** La *ordenación por valor de cartera* en la sección de cobros vencidos es la decisión más inteligente del módulo: si el agente solo puede hacer cinco llamadas de cobranza hoy, el sistema le asegura que llamará primero a los cinco clientes con mayor prima acumulada, no al que llegó primero a la lista. Esto maximiza el dinero recuperado por hora trabajada. Los "leads calientes" aplican el mismo scoring de M7.4 (póliza pronto a vencer + sin cobros vencidos + múltiples pólizas + email activo) pero cruzado con la urgencia del *día*, convirtiendo el análisis predictivo en una acción concreta de hoy. La Agenda Inteligente es, en la práctica, un CRM de prioridades sin necesitar ningún CRM externo.

---

## Síntesis estratégica

| Dimensión | Lo que da la analítica |
|-----------|------------------------|
| **Liquidez** | M1 + M7.5: forecast de cash flow con IC |
| **Retención** | M2.4 + M7.4: churn detectado y leads priorizados |
| **Riesgo legal** | M3.2: SLA de reclamos monitoreado en tiempo real |
| **Rentabilidad** | M3.3 + M5.4: loss ratio y ROI del canal email medidos |
| **Productividad** | M6.1 + M6.2 + M14: carga equitativa, agentes priorizados por valor |
| **Crecimiento** | M4.3 + M8: pipeline de ventas y evidencia histórica |
| **Conocimiento del cliente** | M9: LTV, scores de lealtad/riesgo, historial unificado |

> La analítica no cambia lo que hace el broker — cambia la velocidad y precisión con la que toma decisiones.
