# IADS - Pricing y Control de Versiones (Single-Tenant)

## Objetivo
Definir dos versiones del producto y una estrategia tecnica para controlar acceso por plan en una sola empresa (no multitenant).

- Lite/Normal: acceso a todo menos Analitica y Chatbot.
- Premium/Completa: acceso total.

---

## Pricing propuesto

## Plan Lite
- Precio sugerido: USD 49/mes (o CRC equivalente).
- Incluye: Polizas, Cobros, Reclamos, Cotizaciones, Emails/Notificaciones, Configuracion y Usuarios (segun rol).
- No incluye: Analitica, Chatbot IA.

## Plan Premium
- Precio sugerido: USD 99/mes (o CRC equivalente).
- Incluye todo Lite + Analitica + Chatbot IA.

## Politica comercial sugerida
- Anual: 2 meses gratis.
- Upgrade inmediato prorrateado.
- Downgrade al siguiente ciclo.

---

## Decision de arquitectura (tu enfoque)
No usar multitenancy.
Usar una tabla Company unica para:
- metadata de empresa (branding y personalizacion futura)
- plan actual (Lite/Premium)
- estado comercial basico (activo/suspendido)

Esto encaja bien con la estructura actual.

---

## Estado actual del sistema (relevante)

## Frontend
- Ruteo en frontend-new/src/app/app-routing.module.ts.
- Analitica y chat hoy solo exigen autenticacion (authGuard), no plan.
- Menu en frontend-new/src/app/app.component.ts y frontend-new/src/app/app.component.html.
- Chatbot flotante en frontend-new/src/app/app.component.html.

## Backend
- AnalyticsController y ChatController tienen [Authorize], pero no policy por plan.
- Program.cs ya tiene JWT y politicas por rol, se puede extender a politicas por feature.
- No existe entidad de plan/suscripcion actualmente.

---

## Modelo recomendado (single-tenant)

## Tabla Company
Una sola fila activa para toda la app.

Campos minimos sugeridos:
- Id (int, PK)
- Name (nvarchar(200))
- LegalName (nvarchar(200), null)
- TaxId (nvarchar(50), null)
- LogoUrl (nvarchar(500), null)
- PrimaryColor (nvarchar(20), null)
- SecondaryColor (nvarchar(20), null)
- PlanCode (nvarchar(20))  // LITE | PREMIUM
- IsPlanActive (bit)       // true/false
- PlanUpdatedAt (datetime2)
- CreatedAt / UpdatedAt / IsDeleted

Recomendacion:
- Constraint para PlanCode in ('LITE','PREMIUM').
- Seed inicial: una company por defecto en plan PREMIUM para no romper ambientes actuales.

---

## Entitlements (features)
No depender solo del string del plan en frontend. Resolver features en backend:

- analytics.access
- chatbot.access

Mapa:
- LITE: analytics=false, chatbot=false
- PREMIUM: analytics=true, chatbot=true

---

## Implementacion con la estructura actual

## 1) Backend - entidad y DbContext
- Crear Domain/Entities/Company.cs.
- Agregar DbSet<Company> en Infrastructure/Data/ApplicationDbContext.cs.
- Configurar Company en OnModelCreating.
- Crear migracion.

## 2) Backend - resolver plan en login/refresh
En Application/Services/AuthService.cs:
- leer Company (fila unica)
- agregar claims al JWT:
  - plan = LITE|PREMIUM
  - feature_analytics = true/false
  - feature_chatbot = true/false

## 3) Backend - policies por feature
En backend/src/WebApi/Program.cs agregar:
- policy AnalyticsAccess => claim feature_analytics=true
- policy ChatbotAccess => claim feature_chatbot=true

Aplicar:
- WebApi/Controllers/AnalyticsController.cs -> [Authorize(Policy = "AnalyticsAccess")]
- WebApi/Controllers/ChatController.cs -> [Authorize(Policy = "ChatbotAccess")]
- ChatHub si corresponde -> misma policy.

Resultado: usuario Lite no puede acceder aunque manipule URL/UI.

## 4) Frontend - guard por feature
Crear feature guard en frontend:
- feature.guard.ts

Rutas en app-routing.module.ts:
- analytics -> [authGuard, featureGuard] con data.feature='analytics'
- chat -> [authGuard, featureGuard] con data.feature='chatbot'

Fallback:
- redirigir a /polizas
- mostrar mensaje de upgrade.

## 5) Frontend - menu y chatbot segun plan
En app.component.ts/html:
- ocultar bloque Analitica cuando feature_analytics=false
- ocultar boton/chat dialog cuando feature_chatbot=false
- filtrar quick access para no incluir rutas bloqueadas.

## 6) Endpoint de capacidades (recomendado)
Agregar:
- GET /api/auth/capabilities
Retorna:
- company
- plan
- features

Ventaja:
- refresco de permisos sin relogin.
- soporte para cambio de plan en caliente.

---

## Matriz de acceso final

| Modulo | Lite | Premium |
|---|---|---|
| Polizas | Si | Si |
| Cobros | Si | Si |
| Reclamos | Si | Si |
| Cotizaciones | Si | Si |
| Emails/Notificaciones | Si | Si |
| Configuracion (rol) | Si | Si |
| Usuarios (rol) | Si | Si |
| Analitica | No | Si |
| Chatbot IA | No | Si |

## Matriz final por modulo, version y rol

Convenciones:
- Si = acceso permitido.
- No = acceso denegado.
- Parcial = acceso segun permisos internos del modulo.

| Modulo | Lite Admin | Lite DataLoader | Lite User | Premium Admin | Premium DataLoader | Premium User |
|---|---|---|---|---|---|---|
| Polizas | Si | Si | Si | Si | Si | Si |
| Subir Polizas Excel | Si | Si | No | Si | Si | No |
| Cobros | Si | Si | Si | Si | Si | Si |
| Reclamos | Si | Si | Si | Si | Si | Si |
| Cotizaciones | Si | Si | Si | Si | Si | Si |
| Emails/Notificaciones | Si | Si | Si | Si | Si | Si |
| Configuracion | Si | No | No | Si | No | No |
| Usuarios | Si | No | No | Si | No | No |
| Analitica (feature global) | No | No | No | Si | Si | Si |
| Chatbot IA | No | No | No | Si | Si | Si |

Notas tecnicas:
- Configuracion y Usuarios mantienen control por rol (admin guard).
- Subir Polizas Excel mantiene control por rol (Admin o DataLoader).
- Analitica y Chatbot deben bloquearse por plan (Lite = No) en frontend y backend.

## Matriz profunda por submodulo de Analitica (version + rol)

Analisis aplicado sobre los submodulos reales del modulo de Analitica.

Convenciones:
- Si = acceso completo del submodulo.
- No = acceso denegado.
- Parcial = acceso permitido con restricciones de accion (por ejemplo: sin enviar emails, sin exportaciones sensibles o sin datos de usuarios).

| Submodulo Analitica | Lite Admin | Lite DataLoader | Lite User | Premium Admin | Premium DataLoader | Premium User |
|---|---|---|---|---|---|---|
| 0. Dashboard Ejecutivo | No | No | No | Si | Si | Si |
| 1. Cobros Analytics | No | No | No | Si | Si | Si |
| 2. Portfolio Analytics | No | No | No | Si | Si | Si |
| 3. Reclamos Analytics | No | No | No | Si | Si | Si |
| 4. Sales Funnel | No | No | No | Si | Si | Si |
| 5. Email Analytics | No | No | No | Si | Si | Si |
| 6. Dashboard Operacional | No | No | No | Si | Parcial | No |
| 7. Predictive Analytics | No | No | No | Si | Si | Parcial |
| 8. Agenda Inteligente | No | No | No | Si | Si | Si |
| 9. Cliente360 | No | No | No | Si | Si | No |
| 10. Reportes Exportables | No | No | No | Si | Parcial | No |

Notas de criterio por submodulo:
- Dashboard Operacional (6): contiene carga por agente, actividad de usuarios y rendimiento tecnico; por sensibilidad operativa se recomienda dejar completo en Admin, lectura acotada en DataLoader y cerrado para User.
- Predictive Analytics (7): permite decisiones de priorizacion/riesgo; User debe quedar en lectura limitada (sin indicadores avanzados o sin vistas de riesgo detalladas).
- Cliente360 (9): concentra datos de cliente y contexto 360, por privacidad se recomienda Admin/DataLoader y bloquear User.
- Reportes Exportables (10): es el mas sensible porque ejecuta descargas y envio por email; User debe estar bloqueado y DataLoader solo con descargas controladas (sin envio a admins/gerencia).

## Reglas tecnicas recomendadas por accion (Premium)

Para implementar la matriz anterior sin ambiguedad:

- `analytics.access`:
  habilita entrada al modulo de Analitica por plan (solo Premium).
- `analytics.operational.read`:
  Admin completo, DataLoader lectura restringida, User denegado.
- `analytics.predictive.read`:
  Admin/DataLoader completo, User lectura limitada.
- `analytics.cliente360.read`:
  Admin/DataLoader permitido, User denegado.
- `analytics.reports.download`:
  Admin y DataLoader permitido.
- `analytics.reports.email`:
  solo Admin permitido.

Esto evita que un usuario Premium con rol bajo obtenga acceso a acciones sensibles solo por tener plan Premium.

---

## Plan de ejecucion recomendado

## Sprint 1 (enforcement real)
1. Crear Company + migracion.
2. Claims de plan/features en login/refresh.
3. Policies y proteccion de Analytics/Chat.
4. Feature guard y ocultamiento UI en frontend.

## Sprint 2 (comercial y UX)
1. Endpoint capabilities.
2. Pantalla/Modal upgrade a Premium.
3. Configuracion branding basica desde Company.

---

## Riesgos y mitigacion
- Riesgo: ocultar solo UI.
  - Mitigacion: policies backend obligatorias.

- Riesgo: claim desactualizado tras cambio de plan.
  - Mitigacion: endpoint capabilities y refresh controlado.

- Riesgo: ambientes viejos sin registro Company.
  - Mitigacion: seed inicial y fallback a PREMIUM temporal.

---

## Decision recomendada
Tu enfoque es correcto para este producto: Company unica + plan flag.
Permite lanzar rapido Lite/Premium sin complejidad multitenant, y deja lista la base para branding y personalizacion futura.
