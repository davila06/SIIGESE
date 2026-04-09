# Roadmap — MCP Server + Bot SIINSEG

## Visión

Convertir SIINSEG en un sistema con **IA integrada**: un bot que entiende lenguaje natural y puede consultar, crear y gestionar pólizas, cobros, cotizaciones y reclamos directamente desde el chat, usando MCP como puente entre el modelo de IA y el backend existente.

```
Usuario (chat) → Bot (IA) → MCP Server → Backend SIINSEG API → SQL Server
```

---

## Stack Tecnológico

| Capa | Tecnología |
|---|---|
| MCP Server | **TypeScript** con `@modelcontextprotocol/sdk` (Node.js) |
| Bot / IA | **Claude API** (Anthropic) o Azure OpenAI GPT-4o |
| Frontend chat | Angular — módulo de chat existente (SignalR Hub) |
| Backend API | .NET 8 existente — sin cambios en endpoints |
| Autenticación | JWT del sistema actual (el bot actúa como usuario de tipo `AGENT_BOT`) |

---

## Fases del Roadmap

---

### FASE 1 — MCP Server scaffolding
**Duración estimada: 1–2 días**

Crear el servidor MCP como proyecto independiente dentro del monorepo.

```
enterprise-web-app/
  mcp-server/
    src/
      tools/
      index.ts
    package.json
    tsconfig.json
```

**Tareas:**
- [ ] `npm init` + instalar `@modelcontextprotocol/sdk`
- [ ] Configurar `tsconfig.json` + `package.json` con `"type": "module"`
- [ ] Crear `index.ts` con `Server` y `StdioServerTransport` del SDK
- [ ] Definir variable de entorno `SIINSEG_API_URL` y `SIINSEG_BOT_TOKEN`
- [ ] Probar que el servidor inicia y responde al protocolo MCP básico

**Archivo base:**
```typescript
import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";

const server = new Server(
  { name: "siinseg-mcp", version: "1.0.0" },
  { capabilities: { tools: {} } }
);

const transport = new StdioServerTransport();
await server.connect(transport);
```

---

### FASE 2 — Herramientas MCP (Tools)
**Duración estimada: 3–4 días**

Exponer los endpoints del backend existente como herramientas MCP. Cada tool corresponde a uno o más endpoints de la API actual.

#### 2.1 — Tools de Pólizas
| Tool MCP | Endpoint API | Descripción |
|---|---|---|
| `listar_polizas` | `GET /api/polizas` | Lista pólizas con filtros opcionales |
| `obtener_poliza` | `GET /api/polizas/{id}` | Detalle de una póliza específica |
| `buscar_polizas_cliente` | `GET /api/polizas?cliente=` | Buscar pólizas de un cliente por nombre |
| `polizas_por_vencer` | `GET /api/polizas?vencen=30días` | Pólizas próximas a vencer |

#### 2.2 — Tools de Cobros
| Tool MCP | Endpoint API | Descripción |
|---|---|---|
| `listar_cobros_pendientes` | `GET /api/cobros?estado=Pendiente` | Cobros sin pagar |
| `stats_cobros` | `GET /api/cobros/stats` | Totales, porcentaje cobrado, montos |
| `registrar_pago` | `PUT /api/cobros/{id}/registrar` | Marcar cobro como pagado |
| `generar_cobros` | `POST /api/cobros/generar` | Trigger de generación automática |

#### 2.3 — Tools de Cotizaciones
| Tool MCP | Endpoint API | Descripción |
|---|---|---|
| `listar_cotizaciones` | `GET /api/cotizaciones` | Lista con estado opcional |
| `crear_cotizacion` | `POST /api/cotizaciones` | Crear nueva cotización |
| `aprobar_cotizacion` | `PUT /api/cotizaciones/{id}/aprobar` | Cambiar estado a APROBADA |
| `convertir_cotizacion` | `POST /api/cotizaciones/{id}/convertir` | Crear póliza desde cotización |

#### 2.4 — Tools de Reclamos y Clientes
| Tool MCP | Endpoint API | Descripción |
|---|---|---|
| `listar_reclamos` | `GET /api/reclamos` | Reclamos activos |
| `listar_clientes` | `GET /api/usuarios?rol=CLIENTE` | Lista de clientes |
| `buscar_cliente` | `GET /api/usuarios?nombre=` | Buscar cliente por nombre |

#### 2.5 — Tool de resumen ejecutivo
| Tool MCP | Descripción |
|---|---|
| `dashboard_resumen` | Combina stats de cobros + pólizas activas + cotizaciones pendientes en una sola respuesta |

**Estructura de cada tool:**
```typescript
server.setRequestHandler(ListToolsRequestSchema, async () => ({
  tools: [
    {
      name: "listar_cobros_pendientes",
      description: "Lista todos los cobros pendientes de pago. Puede filtrar por cliente o póliza.",
      inputSchema: {
        type: "object",
        properties: {
          clienteNombre: { type: "string", description: "Filtrar por nombre del cliente" },
          limite: { type: "number", description: "Máximo de resultados (default: 20)" }
        }
      }
    }
    // ... más tools
  ]
}));
```

---

### FASE 3 — Bot con IA
**Duración estimada: 2–3 días**

Crear el bot que usa el MCP server como fuente de herramientas.

#### Opción A — Claude (Anthropic) — Recomendada
```typescript
import Anthropic from "@anthropic-ai/sdk";
import { Client } from "@modelcontextprotocol/sdk/client/index.js";

const claude = new Anthropic({ apiKey: process.env.ANTHROPIC_KEY });
const mcp = new Client({ name: "siinseg-bot", version: "1.0.0" });
// conectar mcp al servidor siinseg-mcp
// pasar tools de mcp a claude en cada mensaje
```

**System prompt base:**
```
Eres el asistente de SIINSEG, un sistema de gestión de seguros.
Ayudas a agentes y administradores a consultar pólizas, cobros, cotizaciones y reclamos.
- Responde siempre en español.
- Usa las herramientas disponibles para obtener datos reales del sistema.
- Nunca inventes datos — si no tienes la información, dilo y ofrece buscarla.
- Para acciones destructivas (cancelar, eliminar) pide confirmación primero.
- La fecha actual es {FECHA_HOY}. Úsala para calcular vencimientos.
```

#### Opción B — Azure OpenAI GPT-4o
- Misma arquitectura pero usando `openai` SDK
- Ventaja: stays dentro del ecosistema Azure ya configurado
- Usar `tool_choice: "auto"` con las tools del MCP convertidas a formato OpenAI

#### API del Bot (nuevo endpoint)
```
POST /api/bot/chat
Body: { "message": "¿Cuántos cobros están pendientes este mes?", "sessionId": "uuid" }
Response: { "reply": "...", "toolsUsed": ["listar_cobros_pendientes"] }
```

- Nuevo controller `BotController.cs` en el backend existente
- O servicio Node.js separado en puerto propio (más rápido de implementar)

---

### FASE 4 — Integración en el Frontend
**Duración estimada: 2 días**

Agregar el bot como widget flotante en el Angular app existente.

#### Componente: `BotChatComponent`
- Botón flotante (FAB) en la esquina inferior derecha
- Panel de chat que se despliega al hacer clic
- Input de texto + historial de mensajes
- Indicador de "pensando..." mientras IA procesa
- Chips de sugerencias rápidas:
  - "Cobros pendientes hoy"
  - "Pólizas por vencer"
  - "Resumen del mes"
  - "Crear cotización AUTO"

#### UX del chat
```
👤 Usuario: ¿Cuánto tengo pendiente de cobrar?

🤖 Bot: Tienes **23 cobros pendientes** por un total de ₡1,245,000.
       Los más urgentes son:
       - Luis Mora — Vence 22/03/2026 — ₡85,000
       - Ana Torres — Vence 25/03/2026 — ₡120,000
       ¿Querés que registre alguno como pagado?
```

---

### FASE 5 — Seguridad y Control de Acceso
**Duración estimada: 1 día**

- El bot solo puede acceder a datos del usuario autenticado (mismo JWT)
- Rol `ADMIN` → acceso completo a todas las tools
- Rol `AGENTE` → solo sus clientes asignados
- Acciones de escritura (registrar pago, crear cotización) requieren confirmación explícita
- Log de todas las acciones realizadas por el bot en tabla `BotAuditLog`

---

### FASE 6 — Testing y Deploy
**Duración estimada: 1–2 días**

- Testing manual del MCP server con MCP Inspector (`npx @modelcontextprotocol/inspector`)
- Testing de conversaciones de extremo a extremo
- Deploy del mcp-server como proceso adicional en el mismo container o como Azure Container App separado
- Variables de entorno en Azure: `ANTHROPIC_KEY` / `AZURE_OPENAI_KEY`, `SIINSEG_API_URL`, `SIINSEG_BOT_TOKEN`

---

## Resumen de Fases

| Fase | Descripción | Días |
|---|---|---|
| 1 | MCP Server scaffolding | 1–2 |
| 2 | Implementar todas las tools MCP | 3–4 |
| 3 | Bot con IA (Claude o GPT-4o) | 2–3 |
| 4 | Widget de chat en Angular | 2 |
| 5 | Seguridad y roles | 1 |
| 6 | Tests y deploy | 1–2 |
| **Total** | | **10–14 días** |

---

## Estructura Final de Archivos

```
enterprise-web-app/
├── backend/                          # .NET existente — sin cambios mayores
│   └── src/WebApi/Controllers/
│       └── BotController.cs          # NUEVO: endpoint POST /api/bot/chat
├── frontend-new/                     # Angular existente
│   └── src/app/components/
│       └── bot-chat/                 # NUEVO: componente FAB + chat
│           ├── bot-chat.component.ts
│           ├── bot-chat.component.html
│           └── bot-chat.component.scss
└── mcp-server/                       # NUEVO: servidor MCP independiente
    ├── src/
    │   ├── index.ts                  # Entry point
    │   ├── client.ts                 # HTTP client → SIINSEG API
    │   └── tools/
    │       ├── polizas.tools.ts
    │       ├── cobros.tools.ts
    │       ├── cotizaciones.tools.ts
    │       └── reclamos.tools.ts
    ├── package.json
    └── tsconfig.json
```

---

## Decisión Pendiente

**¿Qué proveedor de IA usar?**

| | Claude (Anthropic) | Azure OpenAI GPT-4o |
|---|---|---|
| Tool use (MCP nativo) | ✅ Soporte nativo | ✅ Compatible |
| Costo por token | Medio | Medio |
| Latencia | Baja | Baja |
| Ya en Azure | ❌ | ✅ |
| Calidad de razonamiento | Muy alta | Alta |
| **Recomendado para** | Si prioridad es calidad | Si prioridad es integración Azure |

**¿Por dónde empezar?**
→ Fase 1 + Fase 2 son independientes del proveedor de IA. Se puede avanzar sin decidir aún.
