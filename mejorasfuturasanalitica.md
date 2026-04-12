
# SINSEG — Mejoras Futuras de Analítica
> Generado: Abril 2026 | Módulos M9–M14 — Alto valor, bajo esfuerzo

---

## Estado de Implementación

| Módulo | Descripción | Estado | Prioridad |
|--------|-------------|--------|-----------|
| **M9**  | Cliente 360° (LTV + Historia) | ✅ **COMPLETO** | 🔴 Crítico |
| **M10** | Scorecard por Aseguradora | ⬜ Pendiente | 🟠 Alto |
| **M11** | Cross-sell / Brechas de Cobertura | ⬜ Pendiente | 🟠 Alto |
| **M12** | Pareto de Rentabilidad (VIP 80/20) | ⬜ Pendiente | 🟠 Alto |
| **M13** | Broker Salud Index (BSI 0-100) | ⬜ Pendiente | 🟡 Medio |
| **M14** | Agenda Inteligente del Agente | ✅ **COMPLETO** | 🔴 Crítico |

---

## Principios de Implementación

Todos los módulos siguen el **patrón establecido en M0–M8**:

1. Agregar DTOs en `backend/src/Application/DTOs/AnalyticsDtos.cs`
2. Agregar endpoint en `backend/src/WebApi/Controllers/AnalyticsController.cs` (antes de `private static NormalizePrimaMensual`)
3. Build backend: `dotnet build` → 0 errores
4. Agregar interfaces TypeScript y método en `frontend-new/src/app/analytics/services/analytics.service.ts`
5. Crear componente en `frontend-new/src/app/analytics/components/<nombre>/` (3 archivos: `.ts`, `.html`, `.scss`)
6. Registrar ruta en `frontend-new/src/app/analytics/analytics.module.ts`
7. Agregar nav item en `frontend-new/src/app/app.component.html`
8. Build Angular: `npx ng build --configuration development` → 0 errores
9. Actualizar `analitica.md` status

**Convenciones críticas:**
- Interfaces TypeScript usan `ChartDataPoint` y `MultiSeriesChart` (NO sufijo `Dto`)
- Tipos explícitos en callbacks `.map((p: ChartDataPoint) => ...)`
- Standalone components con `imports: [CommonModule, RouterModule, MatXxxModule, NgxChartsModule]`
- Endpoints HTTP: `GET /api/analytics/<nombre>`

---

## MÓDULO 9 — Cliente 360° (LTV + Historia Completa) ✅ COMPLETO

> Vista unificada de un cliente: todas sus pólizas, cobros, reclamos, valor de vida y score.

**Ruta Angular:** `/analytics/cliente360`  
**Componente:** `cliente360` (standalone lazy-loaded)  
**Endpoint:** `GET /api/analytics/cliente360?cedula={cedula}` · `GET /api/analytics/cliente360?nombre={nombre}`  
**Build:** ✅ Backend 0 errores CS · ✅ Angular 0 errores (`Build at: 2026-04-11T19:21:01.968Z`)

### Estado de Implementación

| Sección | Descripción | Estado |
|---------|-------------|--------|
| 9.1 | Backend DTOs (`Cliente360Dto`, `Cliente360PolizaDto`, `Cliente360CobroDto`, `Cliente360ReclamoDto`, `Cliente360SearchResultDto`) | ✅ |
| 9.2 | Controller `GetCliente360()` — búsqueda por nombre/cédula + vista completa | ✅ |
| 9.3 | TypeScript interfaces en `analytics.service.ts` + `getCliente360()` + `searchClientes()` | ✅ |
| 9.4 | Componente Angular standalone (TS + HTML + SCSS) — tabs: Resumen, Pólizas, Cobros, Reclamos | ✅ |
| 9.5 | Ruta `/analytics/cliente360` registrada en `analytics.module.ts` | ✅ |
| 9.6 | Nav item "Cliente 360°" con icono `manage_accounts` en sidebar | ✅ |

### Funcionalidades Implementadas

| Feature | Descripción |
|---------|-------------|
| Búsqueda | SearchBar con `debounce` por nombre o cédula → sugerencias en tiempo real |
| KPI Strip | LTV, Prima Activa, Meses como Cliente, Score Lealtad, Score Riesgo, Total Cobrado, Total Reclamado |
| Score Lealtad | 0-100: meses como cliente + cobros sin vencer + multi-póliza + sin reclamos |
| Score Riesgo | 0-100: tasa de cobros vencidos + cobros >30 días vencidos |
| LTV Timeline | ngx-charts-area-chart con prima cobrada acumulada mensual |
| Tab Pólizas | mat-table: tipo, aseguradora, prima normalizada, frecuencia, vencimiento, estado badge |
| Tab Cobros | mat-table: últimos 20 cobros con estado chips y días vencido/pendiente |
| Tab Reclamos | mat-table: número, tipo, estado, prioridad badge, monto reclamado vs aprobado, días resolución |



### 9.1 DTOs Backend (`AnalyticsDtos.cs`)

```csharp
// ── MÓDULO 9 — Cliente 360°
public class Cliente360Dto
{
    // Identidad
    public string NombreCompleto   { get; set; } = "";
    public string Cedula           { get; set; } = "";
    public string Email            { get; set; } = "";
    public string Telefono         { get; set; } = "";

    // Métricas financieras
    public decimal LTV             { get; set; }  // PrimaTotal - ReclamosAprobados
    public decimal PrimaMensualActiva { get; set; }
    public decimal TotalReclamado  { get; set; }
    public decimal TotalCobrado    { get; set; }
    public int     MesesComoCliente { get; set; }

    // Salud del cliente
    public int     ScoreLealtad    { get; set; }  // 0-100
    public int     ScoreRiesgo     { get; set; }  // 0-100
    public string  CategoriaRiesgo { get; set; } = ""; // Verde/Amarillo/Rojo

    // Listas
    public List<Cliente360PolizaDto>   Polizas    { get; set; } = new();
    public List<Cliente360CobroDto>    Cobros     { get; set; } = new();
    public List<Cliente360ReclamoDto>  Reclamos   { get; set; } = new();
    public List<ChartDataPointDto>     LtvTimeline { get; set; } = new(); // prima acum. mensual
}

public class Cliente360PolizaDto
{
    public string  NumeroPoliza    { get; set; } = "";
    public string  TipoSeguro      { get; set; } = "";
    public string  Aseguradora     { get; set; } = "";
    public decimal Prima           { get; set; }
    public string  Frecuencia      { get; set; } = "";
    public DateTime FechaVigencia  { get; set; }
    public bool    EsActiva        { get; set; }
    public int     DiasParaVencer  { get; set; }
}

public class Cliente360CobroDto
{
    public string  NumeroRecibo    { get; set; } = "";
    public string  Estado          { get; set; } = "";
    public decimal MontoTotal      { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public int     DiasVencido     { get; set; } // negativo = no vencido
}

public class Cliente360ReclamoDto
{
    public string  NumeroReclamo   { get; set; } = "";
    public string  TipoReclamo     { get; set; } = "";
    public string  Estado          { get; set; } = "";
    public string  Prioridad       { get; set; } = "";
    public decimal MontoClamado    { get; set; }
    public decimal MontoAprobado   { get; set; }
    public int     DiasResolucion  { get; set; }
}

public class Cliente360SearchResultDto
{
    public string Cedula           { get; set; } = "";
    public string NombreCompleto   { get; set; } = "";
    public string Email            { get; set; } = "";
    public int    NumeroPolizas    { get; set; }
    public decimal PrimaMensual   { get; set; }
}
```

---

### 9.2 Endpoint Backend (`AnalyticsController.cs`)

```csharp
[HttpGet("cliente360")]
public async Task<IActionResult> GetCliente360([FromQuery] string? cedula, [FromQuery] string? nombre)
{
    // --- Búsqueda ---
    if (string.IsNullOrWhiteSpace(cedula) && string.IsNullOrWhiteSpace(nombre))
    {
        return BadRequest("Debe indicar cedula o nombre.");
    }

    // Si no hay cedula exacta → retornar lista de sugerencias
    if (!string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(cedula))
    {
        var sugerencias = await _db.Polizas
            .Where(p => p.NombreAsegurado.Contains(nombre) || p.NumeroCedula.Contains(nombre))
            .GroupBy(p => new { p.NumeroCedula, p.NombreAsegurado, p.Correo })
            .Select(g => new Cliente360SearchResultDto
            {
                Cedula           = g.Key.NumeroCedula ?? "",
                NombreCompleto   = g.Key.NombreAsegurado,
                Email            = g.Key.Correo ?? "",
                NumeroPolizas    = g.Count(),
                PrimaMensual     = g.Sum(p => NormalizePrimaMensual(p.Prima, p.Frecuencia))
            })
            .Take(10)
            .ToListAsync();
        return Ok(sugerencias);
    }

    // --- Vista completa por cédula ---
    var polizas = await _db.Polizas
        .Where(p => p.NumeroCedula == cedula)
        .ToListAsync();

    if (!polizas.Any()) return NotFound();

    var numPolizas   = polizas.Select(p => p.NumeroPoliza).ToList();
    var cobros       = await _db.Cobros.Where(c => numPolizas.Contains(c.NumeroPoliza)).ToListAsync();
    var reclamos     = await _db.Reclamos.Where(r => numPolizas.Contains(r.NumeroPoliza ?? "")).ToListAsync();

    var primerPoliza      = polizas.OrderBy(p => p.CreatedAt).First();
    var mesesComoCliente  = (int)Math.Max(1, (DateTime.Now - primerPoliza.CreatedAt).TotalDays / 30);
    var primaMensualActiva = polizas.Where(p => p.EsActivo)
                                    .Sum(p => NormalizePrimaMensual(p.Prima, p.Frecuencia));
    var totalCobrado      = cobros.Where(c => c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado)
                                  .Sum(c => c.MontoTotal);
    var totalReclamado    = reclamos.Sum(r => r.MontoReclamado);
    var ltv               = totalCobrado - totalReclamado;

    // Score Lealtad: factores positivos
    int scoreLealtad = 0;
    if (mesesComoCliente >= 24) scoreLealtad += 30;
    else if (mesesComoCliente >= 12) scoreLealtad += 15;
    var cobrosVencidos = cobros.Count(c => c.Estado == EstadoCobro.Vencido);
    if (cobrosVencidos == 0) scoreLealtad += 30;
    else if (cobrosVencidos <= 2) scoreLealtad += 10;
    if (polizas.Count(p => p.EsActivo) > 1) scoreLealtad += 20;
    if (reclamos.Count == 0) scoreLealtad += 20;
    scoreLealtad = Math.Min(100, scoreLealtad);

    // Score Riesgo (morosidad)
    int scoreRiesgo = 0;
    var totalCobrosActivos = cobros.Count(c => c.Estado != EstadoCobro.Cancelado);
    if (totalCobrosActivos > 0)
    {
        var tasaVencido = (double)cobrosVencidos / totalCobrosActivos;
        if (tasaVencido >= 0.5) scoreRiesgo += 50;
        else if (tasaVencido >= 0.25) scoreRiesgo += 25;
    }
    var cobrosVencidosMas30 = cobros.Count(c => c.Estado == EstadoCobro.Vencido &&
                                                 (DateTime.Now - c.FechaVencimiento).TotalDays > 30);
    scoreRiesgo += cobrosVencidosMas30 * 10;
    scoreRiesgo = Math.Min(100, scoreRiesgo);

    var categoriaRiesgo = scoreRiesgo >= 60 ? "Rojo" : scoreRiesgo >= 30 ? "Amarillo" : "Verde";

    // LTV Timeline (Prima acumulada por mes)
    var timeline = cobros
        .Where(c => c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado)
        .GroupBy(c => new DateTime(c.FechaVencimiento.Year, c.FechaVencimiento.Month, 1))
        .OrderBy(g => g.Key)
        .Select(g => new ChartDataPointDto { Name = g.Key.ToString("MMM yy"), Value = g.Sum(c => c.MontoTotal) })
        .ToList();

    var dto = new Cliente360Dto
    {
        NombreCompleto      = primerPoliza.NombreAsegurado,
        Cedula              = cedula,
        Email               = primerPoliza.Correo ?? "",
        Telefono            = primerPoliza.NumeroTelefono ?? "",
        LTV                 = ltv,
        PrimaMensualActiva  = primaMensualActiva,
        TotalReclamado      = totalReclamado,
        TotalCobrado        = totalCobrado,
        MesesComoCliente    = mesesComoCliente,
        ScoreLealtad        = scoreLealtad,
        ScoreRiesgo         = scoreRiesgo,
        CategoriaRiesgo     = categoriaRiesgo,
        LtvTimeline         = timeline,
        Polizas = polizas.Select(p => new Cliente360PolizaDto
        {
            NumeroPoliza   = p.NumeroPoliza,
            TipoSeguro     = NormalizeTipoSeguro(p.TipoSeguro),
            Aseguradora    = p.Aseguradora,
            Prima          = NormalizePrimaMensual(p.Prima, p.Frecuencia),
            Frecuencia     = p.Frecuencia.ToString(),
            FechaVigencia  = p.FechaVigencia,
            EsActiva       = p.EsActivo,
            DiasParaVencer = (int)(p.FechaVigencia - DateTime.Now).TotalDays
        }).OrderByDescending(p => p.EsActiva).ToList(),
        Cobros = cobros.OrderByDescending(c => c.FechaVencimiento).Take(20).Select(c => new Cliente360CobroDto
        {
            NumeroRecibo     = c.NumeroRecibo,
            Estado           = c.Estado.ToString(),
            MontoTotal       = c.MontoTotal,
            FechaVencimiento = c.FechaVencimiento,
            DiasVencido      = (int)(DateTime.Now - c.FechaVencimiento).TotalDays
        }).ToList(),
        Reclamos = reclamos.OrderByDescending(r => r.FechaReclamo).Select(r => new Cliente360ReclamoDto
        {
            NumeroReclamo  = r.NumeroReclamo ?? "",
            TipoReclamo    = r.TipoReclamo.ToString(),
            Estado         = r.Estado.ToString(),
            Prioridad      = r.PrioridadReclamo.ToString(),
            MontoClamado   = r.MontoReclamado,
            MontoAprobado  = r.MontoAprobado ?? 0,
            DiasResolucion = r.FechaResolucion.HasValue
                ? (int)(r.FechaResolucion.Value - r.FechaReclamo).TotalDays : -1
        }).ToList()
    };

    return Ok(dto);
}
```

---

### 9.3 Interfaces TypeScript (`analytics.service.ts`)

```typescript
export interface Cliente360SearchResultDto {
  cedula: string;
  nombreCompleto: string;
  email: string;
  numeroPolizas: number;
  primaMensual: number;
}

export interface Cliente360PolizaDto {
  numeroPoliza: string;
  tipoSeguro: string;
  aseguradora: string;
  prima: number;
  frecuencia: string;
  fechaVigencia: string;
  esActiva: boolean;
  diasParaVencer: number;
}

export interface Cliente360CobroDto {
  numeroRecibo: string;
  estado: string;
  montoTotal: number;
  fechaVencimiento: string;
  diasVencido: number;
}

export interface Cliente360ReclamoDto {
  numeroReclamo: string;
  tipoReclamo: string;
  estado: string;
  prioridad: string;
  montoClamado: number;
  montoAprobado: number;
  diasResolucion: number;
}

export interface Cliente360Dto {
  nombreCompleto: string;
  cedula: string;
  email: string;
  telefono: string;
  ltv: number;
  primaMensualActiva: number;
  totalReclamado: number;
  totalCobrado: number;
  mesesComoCliente: number;
  scoreLealtad: number;
  scoreRiesgo: number;
  categoriaRiesgo: string;
  ltvTimeline: ChartDataPoint[];
  polizas: Cliente360PolizaDto[];
  cobros: Cliente360CobroDto[];
  reclamos: Cliente360ReclamoDto[];
}

// Método en AnalyticsService:
getCliente360(cedula: string): Observable<Cliente360Dto> {
  return this.http.get<Cliente360Dto>(`${this.base}/cliente360?cedula=${cedula}`);
}
searchClientes(query: string): Observable<Cliente360SearchResultDto[]> {
  return this.http.get<Cliente360SearchResultDto[]>(`${this.base}/cliente360?nombre=${query}`);
}
```

---

### 9.4 Componente Angular — Layout

```
cliente360.component.html
├── SearchBar (buscar por nombre/cédula con autocomplete)
├── [Si hay resultado]
│   ├── Header: Nombre + Cédula + Email + Teléfono
│   ├── KPI Strip: LTV · Prima Activa · Meses como Cliente · Score Lealtad · Score Riesgo
│   ├── Tabs (MatTabGroup):
│   │   ├── Tab "Resumen"
│   │   │   ├── ngx-charts-area-chart (LTV Timeline acumulado)
│   │   │   └── Score bars (Lealtad verde, Riesgo color dinámico)
│   │   ├── Tab "Pólizas" → mat-table con badge activa/vencida + dias para vencer
│   │   ├── Tab "Cobros"  → mat-table últimos 20 + chips de estado
│   │   └── Tab "Reclamos" → mat-table con badge prioridad + monto aprobado/reclamado
```

---

## MÓDULO 10 — Scorecard por Aseguradora

> Dashboard comparativo de rendimiento entre las 6 aseguradoras.

**Ruta Angular:** `/analytics/aseguradoras`  
**Componente:** `aseguradoras-scorecard`  
**Endpoint:** `GET /api/analytics/aseguradoras`  
**Esfuerzo estimado:** 0.5 días

---

### 10.1 DTOs Backend

```csharp
// ── MÓDULO 10 — Scorecard Aseguradoras
public class AseguradoraScorecardDto
{
    public List<AseguradoraRowDto> Tabla    { get; set; } = new();
    public List<ChartDataPointDto> PorPrima { get; set; } = new(); // bar chart
    public List<ChartDataPointDto> PorTasaCobro { get; set; } = new();
    public List<ChartDataPointDto> PorLossRatio { get; set; } = new();
    public List<ChartDataPointDto> PorSLA       { get; set; } = new();
}

public class AseguradoraRowDto
{
    public string  Nombre          { get; set; } = "";
    public int     NumPolizas      { get; set; }
    public decimal PrimaMensual    { get; set; }
    public double  TasaCobro       { get; set; }  // %
    public double  LossRatio       { get; set; }  // %
    public double  TasaSLA         { get; set; }  // % reclamos dentro de plazo
    public double  TasaReclamos    { get; set; }  // reclamos / polizas activas
    public int     ReclamosActivos { get; set; }
    public string  Grade           { get; set; } = ""; // A/B/C/D según scoring
    public int     Score           { get; set; }  // 0-100
}
```

### 10.2 Lógica de scoring en controller

```csharp
[HttpGet("aseguradoras")]
public async Task<IActionResult> GetAseguradoras()
{
    var polizas  = await _db.Polizas.ToListAsync();
    var cobros   = await _db.Cobros.ToListAsync();
    var reclamos = await _db.Reclamos.ToListAsync();
    
    var aseguradoras = polizas.Select(p => p.Aseguradora).Distinct();
    var rows = new List<AseguradoraRowDto>();
    
    foreach (var ase in aseguradoras)
    {
        var pols = polizas.Where(p => p.Aseguradora == ase).ToList();
        var cobs = cobros.Where(c => pols.Select(p => p.NumeroPoliza).Contains(c.NumeroPoliza)).ToList();
        var recl = reclamos.Where(r => pols.Select(p => p.NumeroPoliza).Contains(r.NumeroPoliza ?? "")).ToList();

        decimal primaMensual = pols.Where(p => p.EsActivo).Sum(p => NormalizePrimaMensual(p.Prima, p.Frecuencia));
        var cobTotal = cobs.Sum(c => c.MontoTotal);
        var cobPagado = cobs.Where(c => c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado).Sum(c => c.MontoTotal);
        double tasaCobro = cobTotal > 0 ? (double)(cobPagado / cobTotal) * 100 : 0;
        double lossRatio = primaMensual > 0 ? (double)(recl.Sum(r => r.MontoAprobado ?? 0) / (primaMensual * 12)) * 100 : 0;
        var reclConFechaLimite = recl.Where(r => r.FechaLimiteRespuesta.HasValue && r.FechaResolucion.HasValue).ToList();
        double sla = reclConFechaLimite.Count > 0
            ? reclConFechaLimite.Count(r => r.FechaResolucion <= r.FechaLimiteRespuesta) * 100.0 / reclConFechaLimite.Count : 100;
        var polsActivas = pols.Count(p => p.EsActivo);
        var reclActivos = recl.Count(r => r.Estado != EstadoReclamo.Resuelto && r.Estado != EstadoReclamo.Cerrado);

        // Score: tasaCobro×0.40 + (100-lossRatio)×0.30 + sla×0.20 + (reclActivos==0?10:0)
        int score = (int)(tasaCobro * 0.40 + Math.Max(0, 100 - lossRatio) * 0.30 + sla * 0.20);
        score = Math.Min(100, Math.Max(0, score));
        string grade = score >= 80 ? "A" : score >= 65 ? "B" : score >= 50 ? "C" : "D";

        rows.Add(new AseguradoraRowDto
        {
            Nombre = ase, NumPolizas = pols.Count, PrimaMensual = primaMensual,
            TasaCobro = Math.Round(tasaCobro, 1), LossRatio = Math.Round(lossRatio, 1),
            TasaSLA = Math.Round(sla, 1), TasaReclamos = polsActivas > 0 ? Math.Round((double)reclActivos / polsActivas * 100, 1) : 0,
            ReclamosActivos = reclActivos, Grade = grade, Score = score
        });
    }

    rows = rows.OrderByDescending(r => r.Score).ToList();
    return Ok(new AseguradoraScorecardDto
    {
        Tabla        = rows,
        PorPrima     = rows.Select(r => new ChartDataPointDto { Name = r.Nombre, Value = (double)r.PrimaMensual }).ToList(),
        PorTasaCobro = rows.Select(r => new ChartDataPointDto { Name = r.Nombre, Value = r.TasaCobro }).ToList(),
        PorLossRatio = rows.Select(r => new ChartDataPointDto { Name = r.Nombre, Value = r.LossRatio }).ToList(),
        PorSLA       = rows.Select(r => new ChartDataPointDto { Name = r.Nombre, Value = r.TasaSLA }).ToList()
    });
}
```

### 10.3 Layout Componente Angular

```
aseguradoras-scorecard.component.html
├── KPI strip: Mejor aseguradora (grade A) · Peor loss ratio · Mejor SLA
├── mat-table: Aseguradora | Pólizas | Prima | Tasa Cobro | Loss Ratio | SLA | Grade chip
├── Grid 2×2 bar charts:
│   ├── Prima mensual por aseguradora (bar-horizontal)
│   ├── Tasa de cobro (bar-horizontal, colores semáforo)
│   ├── Loss Ratio (bar-horizontal — menor es mejor)
│   └── Cumplimiento SLA (bar-horizontal)
```

---

## MÓDULO 11 — Cross-sell / Detección de Brechas de Cobertura

> Lista priorizada de clientes con oportunidades de venta cruzada.

**Ruta Angular:** `/analytics/crosssell`  
**Componente:** `crosssell-analytics`  
**Endpoint:** `GET /api/analytics/crosssell`  
**Esfuerzo estimado:** 0.5 días

---

### 11.1 DTOs Backend

```csharp
// ── MÓDULO 11 — Cross-sell
public class CrosssellAnalyticsDto
{
    public int    TotalOportunidades    { get; set; }
    public decimal ValorPotencialMensual { get; set; }
    public List<CrosssellClienteDto>  Clientes { get; set; } = new();
    public List<ChartDataPointDto>    PorTipo  { get; set; } = new(); // oportunidades por tipo
}

public class CrosssellClienteDto
{
    public string   Cedula           { get; set; } = "";
    public string   NombreCompleto   { get; set; } = "";
    public string   Email            { get; set; } = "";
    public string   Telefono         { get; set; } = "";
    public List<string> TiposActuales  { get; set; } = new(); // ["AUTO", "HOGAR"]
    public List<string> TiposFaltantes { get; set; } = new(); // ["VIDA", "EMPRESARIAL"]
    public decimal  PrimaMensualActual { get; set; }
    public decimal  PotencialAdicional { get; set; } // estimado por tipo faltante
    public int      NumeroPolicasActivas { get; set; }
    public int      ScoreOportunidad  { get; set; } // 0-100
    public bool     EsClienteLeal     { get; set; } // sin cobros vencidos
}
```

### 11.2 Lógica Controller

```csharp
[HttpGet("crosssell")]
public async Task<IActionResult> GetCrosssell()
{
    // Prima promedio por tipo para estimación
    var tiposPromedio = await _db.Polizas
        .Where(p => p.EsActivo)
        .GroupBy(p => NormalizeTipoSeguro(p.TipoSeguro))
        .Select(g => new { Tipo = g.Key, Promedio = g.Average(p => NormalizePrimaMensual(p.Prima, p.Frecuencia)) })
        .ToDictionaryAsync(x => x.Tipo, x => x.Promedio);

    var todosTipos = new[] { "AUTO", "VIDA", "HOGAR", "EMPRESARIAL" };
    var cobros     = await _db.Cobros.ToListAsync();

    var clientes = await _db.Polizas
        .Where(p => p.EsActivo && !string.IsNullOrEmpty(p.NumeroCedula))
        .GroupBy(p => new { p.NumeroCedula, p.NombreAsegurado, p.Correo, p.NumeroTelefono })
        .ToListAsync();

    var resultado = new List<CrosssellClienteDto>();

    foreach (var grupo in clientes)
    {
        var tiposActuales = grupo.Select(p => NormalizeTipoSeguro(p.TipoSeguro)).Distinct().ToList();
        var tiposFaltantes = todosTipos.Where(t => !tiposActuales.Contains(t)).ToList();
        if (!tiposFaltantes.Any()) continue;

        var numPolizas = grupo.Select(p => p.NumeroPoliza).ToList();
        var cobrosCliente = cobros.Where(c => numPolizas.Contains(c.NumeroPoliza)).ToList();
        var esLeal = !cobrosCliente.Any(c => c.Estado == EstadoCobro.Vencido);

        decimal potencial = tiposFaltantes.Sum(t => tiposPromedio.TryGetValue(t, out var prom) ? prom : 50000m);
        decimal primaActual = grupo.Sum(p => NormalizePrimaMensual(p.Prima, p.Frecuencia));

        int score = tiposFaltantes.Count * 20 + (esLeal ? 20 : 0) + (grupo.Count() > 1 ? 10 : 0);
        score = Math.Min(100, score);

        resultado.Add(new CrosssellClienteDto
        {
            Cedula = grupo.Key.NumeroCedula!, NombreCompleto = grupo.Key.NombreAsegurado,
            Email = grupo.Key.Correo ?? "", Telefono = grupo.Key.NumeroTelefono ?? "",
            TiposActuales = tiposActuales, TiposFaltantes = tiposFaltantes,
            PrimaMensualActual = primaActual, PotencialAdicional = potencial,
            NumeroPolicasActivas = grupo.Count(), ScoreOportunidad = score, EsClienteLeal = esLeal
        });
    }

    resultado = resultado.OrderByDescending(r => r.ScoreOportunidad).Take(100).ToList();

    var porTipo = tiposFaltantes => todosTipos.Select(t => new ChartDataPointDto
        { Name = t, Value = resultado.Count(r => r.TiposFaltantes.Contains(t)) }).ToList();

    return Ok(new CrosssellAnalyticsDto
    {
        TotalOportunidades     = resultado.Count,
        ValorPotencialMensual  = resultado.Sum(r => r.PotencialAdicional),
        Clientes               = resultado,
        PorTipo                = todosTipos.Select(t => new ChartDataPointDto
            { Name = t, Value = resultado.Count(r => r.TiposFaltantes.Contains(t)) }).ToList()
    });
}
```

### 11.3 Layout Componente Angular

```
crosssell-analytics.component.html
├── KPI strip: Total oportunidades · Valor potencial mensual (₡) · Clientes leales con brecha
├── ngx-charts-pie-chart (oportunidades por tipo faltante, donut)
├── Filtros: [ ] Solo leales  |  Tipo faltante: [AUTO][VIDA][HOGAR][EMPRESARIAL]
├── mat-table:
│   Nombre | Tiene | Le falta (chips de color) | Prima Actual | Potencial | Score | Acción
│   (botón "Enviar oferta" → navega a /cotizaciones/nueva?cedula=xxx)
```

---

## MÓDULO 12 — Pareto de Rentabilidad (VIP 80/20)

> Identifica qué clientes generan el 80% del ingreso y cuáles concentran el 80% del riesgo.

**Ruta Angular:** `/analytics/pareto`  
**Componente:** `pareto-analytics`  
**Endpoint:** `GET /api/analytics/pareto`  
**Esfuerzo estimado:** 0.5 días

---

### 12.1 DTOs Backend

```csharp
// ── MÓDULO 12 — Pareto
public class ParetoAnalyticsDto
{
    public int    TotalClientes        { get; set; }
    public int    ClientesVip20Pct     { get; set; } // cuántos clientes = 80% prima
    public decimal PrimaVip            { get; set; }
    public decimal PrimaTotal          { get; set; }
    public List<ParetoClienteDto>   Clientes    { get; set; } = new();
    public List<ChartDataPointDto>  CurvaLorenz { get; set; } = new(); // % clientes vs % prima acum.
}

public class ParetoClienteDto
{
    public int     Rank               { get; set; }
    public string  Cedula             { get; set; } = "";
    public string  NombreCompleto     { get; set; } = "";
    public decimal PrimaMensual       { get; set; }
    public double  PctDelTotal        { get; set; }
    public double  PctAcumulado       { get; set; }
    public bool    EsVip              { get; set; } // en el top 20%
    public int     NumPolizas         { get; set; }
    public bool    TieneCobrosVencidos { get; set; }
    public string  CategoriaRiesgo    { get; set; } = ""; // Verde/Amarillo/Rojo
}
```

### 12.2 Lógica Controller

```csharp
[HttpGet("pareto")]
public async Task<IActionResult> GetPareto()
{
    var cobros = await _db.Cobros.ToListAsync();
    var polizas = await _db.Polizas.Where(p => p.EsActivo).ToListAsync();

    var porCliente = polizas
        .Where(p => !string.IsNullOrEmpty(p.NumeroCedula))
        .GroupBy(p => new { p.NumeroCedula, p.NombreAsegurado })
        .Select(g =>
        {
            var nums = g.Select(p => p.NumeroPoliza).ToList();
            var cobrosCliente = cobros.Where(c => nums.Contains(c.NumeroPoliza)).ToList();
            var prima = g.Sum(p => NormalizePrimaMensual(p.Prima, p.Frecuencia));
            var tieneVencidos = cobrosCliente.Any(c => c.Estado == EstadoCobro.Vencido);
            return new { g.Key.NumeroCedula, g.Key.NombreAsegurado, Prima = prima,
                         NumPolizas = g.Count(), TieneVencidos = tieneVencidos };
        })
        .OrderByDescending(x => x.Prima)
        .ToList();

    var total = porCliente.Sum(x => x.Prima);
    var vip20Count = (int)Math.Ceiling(porCliente.Count * 0.20);
    decimal acumulado = 0;
    var clientes = new List<ParetoClienteDto>();
    var lorenzPuntos = new List<ChartDataPointDto>();

    for (int i = 0; i < porCliente.Count; i++)
    {
        acumulado += porCliente[i].Prima;
        double pctAcum = total > 0 ? (double)(acumulado / total) * 100 : 0;
        bool esVip = i < vip20Count;
        double tasaVencidos = cobros.Count(c => c.Estado == EstadoCobro.Vencido &&
            polizas.Where(p => p.NumeroCedula == porCliente[i].NumeroCedula).Select(p => p.NumeroPoliza).Contains(c.NumeroPoliza)) * 100.0
            / Math.Max(1, cobros.Count(c => polizas.Where(p => p.NumeroCedula == porCliente[i].NumeroCedula).Select(p => p.NumeroPoliza).Contains(c.NumeroPoliza)));
        string cat = tasaVencidos >= 50 ? "Rojo" : tasaVencidos >= 20 ? "Amarillo" : "Verde";

        clientes.Add(new ParetoClienteDto
        {
            Rank = i + 1, Cedula = porCliente[i].NumeroCedula!, NombreCompleto = porCliente[i].NombreAsegurado,
            PrimaMensual = porCliente[i].Prima, PctDelTotal = total > 0 ? (double)(porCliente[i].Prima / total) * 100 : 0,
            PctAcumulado = pctAcum, EsVip = esVip, NumPolizas = porCliente[i].NumPolizas,
            TieneCobrosVencidos = porCliente[i].TieneVencidos, CategoriaRiesgo = cat
        });

        // Lorenz: cada 5% de clientes
        if ((i + 1) % Math.Max(1, porCliente.Count / 20) == 0)
            lorenzPuntos.Add(new ChartDataPointDto
            {
                Name = $"{Math.Round((i + 1) * 100.0 / porCliente.Count, 0)}%",
                Value = pctAcum
            });
    }

    return Ok(new ParetoAnalyticsDto
    {
        TotalClientes = porCliente.Count, ClientesVip20Pct = vip20Count,
        PrimaVip = clientes.Where(c => c.EsVip).Sum(c => c.PrimaMensual),
        PrimaTotal = total, Clientes = clientes, CurvaLorenz = lorenzPuntos
    });
}
```

### 12.3 Layout Componente Angular

```
pareto-analytics.component.html
├── KPI strip: "Los 20% top clientes generan el X% de la prima" · Clientes VIP N · Prima VIP ₡
├── ngx-charts-area-chart (Curva de Lorenz — eje X: % clientes, eje Y: % prima acum.)
│   con línea de referencia 45° perfecta igualdad
├── mat-table con paginación (ordenable):
│   Rank | Cliente | Prima/Mes | % del total | % Acum. | VIP badge | Cobros vencidos chip
├── Alerta: "X clientes VIP tienen cobros vencidos — acción prioritaria recomendada"
```

---

## MÓDULO 13 — Broker Salud Index (BSI 0-100)

> Un número que resume la salud operativa y financiera del broker en tiempo real.

**Ruta Angular:** parte del Dashboard Ejecutivo (sección adicional) o `/analytics/bsi`  
**Componente:** puede ser un widget en `executive-dashboard` o componente independiente  
**Endpoint:** `GET /api/analytics/bsi`  
**Esfuerzo estimado:** 0.25 días

---

### 13.1 DTOs Backend

```csharp
// ── MÓDULO 13 — BSI
public class BsiDto
{
    public int     ScoreActual       { get; set; }   // 0-100
    public int     ScoreMesAnterior  { get; set; }
    public int     Delta             { get; set; }   // diferencia
    public string  Categoria         { get; set; } = ""; // Excelente/Bueno/Regular/Crítico
    public List<BsiComponenteDto> Componentes { get; set; } = new();
    public List<ChartDataPointDto> HistoricoMeses { get; set; } = new(); // últimos 6 meses
}

public class BsiComponenteDto
{
    public string  Nombre     { get; set; } = "";
    public double  Valor      { get; set; }
    public double  Peso       { get; set; }  // 0.35, 0.30, 0.20, 0.15
    public double  Puntuacion { get; set; }  // Valor × Peso
    public string  Estado     { get; set; } = ""; // Verde/Amarillo/Rojo
}
```

### 13.2 Fórmula BSI

```
BSI = (TasaCobro × 0.35) + (TasaRetencionPolizas × 0.30) + (CumplimientoSLA × 0.20) + (TasaConversionCotizaciones × 0.15)

Categorías:
  80–100: 🟢 Excelente
  65–79:  🟡 Bueno
  50–64:  🟠 Regular
  0–49:   🔴 Crítico
```

### 13.3 Layout Widget

```
bsi.component.html
├── ngx-charts-gauge (valor 0-100, colores semáforo)
│   + Badge: "EXCELENTE" / "REGULAR" / etc.
│   + Delta vs mes anterior: ▲ +3 pts / ▼ -5 pts
├── Breakdown de componentes (progress bars con etiquetas):
│   Tasa Cobro (35%):          ████████░░ 87.3 → 30.6 pts
│   Retención Pólizas (30%):   ███████░░░ 82.1 → 24.6 pts
│   Cumplimiento SLA (20%):    █████░░░░░ 71.0 → 14.2 pts
│   Conversión Cotizaciones (15%): ████░░░░ 68.5 → 10.3 pts
├── ngx-charts-line-chart (BSI los últimos 6 meses)
```

---

## MÓDULO 14 — Agenda Inteligente del Agente ✅ COMPLETO

> Una sola pantalla que le dice al agente qué hacer hoy, no mañana, no la semana pasada. HOY.

**Ruta Angular:** `/analytics/agenda`  
**Componente:** `agenda-inteligente` (standalone lazy-loaded)  
**Endpoint:** `GET /api/analytics/agenda`  
**Build:** ✅ Backend 0 errores CS · ✅ Angular 0 errores (lazy chunk)

### Estado de Implementación

| Sección | Descripción | Estado |
|---------|-------------|--------|
| 14.1 | Backend DTOs (`AgendaDto`, `AgendaSeccionDto`, `AgendaItemDto`) | ✅ |
| 14.2 | Controller `GetAgenda()` — 5 secciones de prioridad | ✅ |
| 14.3 | TypeScript interfaces en `analytics.service.ts` + `getAgenda()` | ✅ |
| 14.4 | Componente Angular standalone (TS + HTML + SCSS) | ✅ |
| 14.5 | Ruta `/analytics/agenda` registrada en `analytics.module.ts` | ✅ |
| 14.6 | Nav item "Agenda Inteligente" con icono `today` en sidebar | ✅ |

### Secciones de la Agenda

| Prioridad | Sección | Lógica |
|-----------|---------|--------|
| 🔴 Crítico | Cobros Vencidos | Top 5 clientes con mayor cartera y cobros vencidos, agrupados por cédula |
| 🟠 Alerta | Vencimientos Hoy / Mañana | Pólizas activas con `FechaVigencia` en ≤ 48 horas |
| 🟠 Alerta | Reclamos Críticos Activos | Reclamos con prioridad Alta o Crítica no resueltos |
| 🟣 Aviso | Leads Calientes | Pólizas por vencer en ≤ 7 días con score ≥ 50 (5 factores) |
| 🔵 Info | Cotizaciones sin Seguimiento | Estado "PENDIENTE" con > 5 días sin cambio, top 5 por prima |

---

### 14.1 DTOs Backend

```csharp
// ── MÓDULO 14 — Agenda Inteligente
public class AgendaDto
{
    public DateTime FechaGeneracion   { get; set; }
    public int      TotalAcciones     { get; set; }
    public List<AgendaSeccionDto> Secciones { get; set; } = new();
}

public class AgendaSeccionDto
{
    public string   Titulo      { get; set; } = "";  // "URGENTE HOY", "ESTA SEMANA", etc.
    public string   Nivel       { get; set; } = "";  // critico / alerta / aviso / info
    public string   Icono       { get; set; } = "";  // material icon name
    public int      Total       { get; set; }
    public List<AgendaItemDto> Items { get; set; } = new();
}

public class AgendaItemDto
{
    public string   Tipo        { get; set; } = ""; // cobro/poliza/cotizacion/reclamo/lead
    public string   Titulo      { get; set; } = ""; // "Cobro vencido - Juan Pérez"
    public string   Descripcion { get; set; } = ""; // "NumRecibo #1234 · ₡145,000 · 45 días vencido"
    public string   Nivel       { get; set; } = ""; // critico/alerta/aviso/info
    public string?  Cedula      { get; set; }        // para navegar a cliente360
    public string?  NumeroRef   { get; set; }        // número de cobro/póliza/etc.
    public string?  NavLink     { get; set; }        // ruta angular sugerida
    public decimal? Monto       { get; set; }
    public int?     DiasVencido { get; set; }
    public int?     Score       { get; set; }        // para leads
}
```

### 14.2 Lógica Controller

```csharp
[HttpGet("agenda")]
public async Task<IActionResult> GetAgenda()
{
    var hoy      = DateTime.Now.Date;
    var en7dias  = hoy.AddDays(7);
    var en30dias = hoy.AddDays(30);

    var polizas  = await _db.Polizas.Where(p => p.EsActivo).ToListAsync();
    var cobros   = await _db.Cobros.ToListAsync();
    var reclamos = await _db.Reclamos.ToListAsync();
    var cotizaciones = await _db.Cotizaciones.ToListAsync();
    var emails   = await _db.EmailLogs.Where(e => e.SentAt >= hoy.AddDays(-30)).ToListAsync();

    var secciones = new List<AgendaSeccionDto>();

    // SECCIÓN 1: URGENTE HOY (crítico)
    var vencidosVip = cobros
        .Where(c => c.Estado == EstadoCobro.Vencido)
        .Join(polizas, c => c.NumeroPoliza, p => p.NumeroPoliza, (c, p) => new { c, p })
        .GroupBy(x => x.p.NumeroCedula)
        .Select(g => new { g, Prima = g.Sum(x => NormalizePrimaMensual(x.p.Prima, x.p.Frecuencia)) })
        .OrderByDescending(x => x.Prima)
        .Take(5)
        .ToList();

    var urgentes = vencidosVip.Select(x => new AgendaItemDto
    {
        Tipo = "cobro", Nivel = "critico",
        Titulo = $"Cobro vencido — {x.g.First().p.NombreAsegurado}",
        Descripcion = $"{x.g.Count()} cobro(s) vencidos · ₡{x.g.Sum(y => y.c.MontoTotal):N0} · cliente VIP",
        Cedula = x.g.Key, Monto = x.g.Sum(y => y.c.MontoTotal),
        NavLink = $"/analytics/cliente360?cedula={x.g.Key}"
    }).ToList();

    if (urgentes.Any())
        secciones.Add(new AgendaSeccionDto { Titulo = "URGENTE HOY", Nivel = "critico",
            Icono = "crisis_alert", Total = urgentes.Count, Items = urgentes });

    // SECCIÓN 2: PÓLIZAS POR VENCER HOY / MAÑANA
    var porVencer = polizas
        .Where(p => p.FechaVigencia.Date >= hoy && p.FechaVigencia.Date <= hoy.AddDays(2))
        .Select(p => new AgendaItemDto
        {
            Tipo = "poliza", Nivel = "alerta",
            Titulo = $"Póliza vence hoy — {p.NombreAsegurado}",
            Descripcion = $"{p.NumeroPoliza} · {NormalizeTipoSeguro(p.TipoSeguro)} · {p.Aseguradora}",
            NumeroRef = p.NumeroPoliza, Monto = NormalizePrimaMensual(p.Prima, p.Frecuencia),
            DiasVencido = (int)(p.FechaVigencia.Date - hoy).TotalDays
        }).ToList();

    if (porVencer.Any())
        secciones.Add(new AgendaSeccionDto { Titulo = "VENCIMIENTOS HOY / MAÑANA", Nivel = "alerta",
            Icono = "event_busy", Total = porVencer.Count, Items = porVencer });

    // SECCIÓN 3: LEADS CALIENTES (score >= 70, vencen en 7 días)
    var leads = polizas
        .Where(p => p.FechaVigencia.Date >= hoy && p.FechaVigencia.Date <= en7dias)
        .Select(p =>
        {
            var numPols = polizas.Count(pp => pp.NumeroCedula == p.NumeroCedula);
            var cobrosCliente = cobros.Where(c => c.NumeroPoliza == p.NumeroPoliza).ToList();
            int score = 30 + (cobrosCliente.All(c => c.Estado != EstadoCobro.Vencido) ? 20 : 0)
                           + (numPols > 1 ? 15 : 0)
                           + (emails.Any(e => e.ToEmail == p.Correo && e.IsSuccess) ? 10 : 0)
                           + (NormalizeTipoSeguro(p.TipoSeguro) is "VIDA" or "EMPRESARIAL" ? 10 : 0);
            return new { p, score };
        })
        .Where(x => x.score >= 60)
        .OrderByDescending(x => x.score)
        .Take(5)
        .Select(x => new AgendaItemDto
        {
            Tipo = "lead", Nivel = "aviso",
            Titulo = $"Lead caliente — {x.p.NombreAsegurado}",
            Descripcion = $"Score {x.score}/100 · Vence en {(int)(x.p.FechaVigencia.Date - hoy).TotalDays} días · {NormalizeTipoSeguro(x.p.TipoSeguro)}",
            Score = x.score, NumeroRef = x.p.NumeroPoliza, Cedula = x.p.NumeroCedula
        }).ToList();

    if (leads.Any())
        secciones.Add(new AgendaSeccionDto { Titulo = "LEADS CALIENTES — Contactar esta semana", Nivel = "aviso",
            Icono = "local_fire_department", Total = leads.Count, Items = leads });

    // SECCIÓN 4: COTIZACIONES PENDIENTES + 5 DÍAS
    var cotsPendientes = cotizaciones
        .Where(c => c.EstadoCotizacion == EstadoCotizacion.Pendiente
                 && (DateTime.Now - c.FechaCreacion).TotalDays > 5)
        .OrderByDescending(c => c.PrimaCotizada)
        .Take(5)
        .Select(c => new AgendaItemDto
        {
            Tipo = "cotizacion", Nivel = "info",
            Titulo = $"Cotización sin respuesta — {c.NombreAsegurado}",
            Descripcion = $"#{c.NumeroCotizacion} · {(int)(DateTime.Now - c.FechaCreacion).TotalDays} días pendiente · ₡{c.PrimaCotizada:N0}",
            NumeroRef = c.NumeroCotizacion, Monto = c.PrimaCotizada
        }).ToList();

    if (cotsPendientes.Any())
        secciones.Add(new AgendaSeccionDto { Titulo = "COTIZACIONES SIN SEGUIMIENTO", Nivel = "info",
            Icono = "pending_actions", Total = cotsPendientes.Count, Items = cotsPendientes });

    return Ok(new AgendaDto
    {
        FechaGeneracion = DateTime.Now,
        TotalAcciones   = secciones.Sum(s => s.Total),
        Secciones       = secciones
    });
}
```

### 14.3 Layout Componente Angular

```
agenda-inteligente.component.html
├── Header: "Agenda — Viernes 11 Abril 2026" + "X acciones hoy"  [Actualizar]
├── Para cada sección:
│   ├── Banner de color segun nivel: [🔴 URGENTE HOY — 3 items]
│   └── Lista de cards:
│       ├── Icono de tipo + badge de nivel
│       ├── Título en negrita
│       ├── Descripción secundaria
│       ├── Chips: monto / días / score
│       └── Botón acción (navegar a cliente360, cobros, cotizaciones)
├── Empty state si TotalAcciones = 0: "✅ Todo en orden — no hay acciones pendientes hoy"
```

---

## Plan de Implementación Sugerido

### Sprint A (2 días) — Mayor ROI inmediato
| # | Módulo | Días |
|---|--------|------|
| 1 | M14 Agenda Inteligente | 0.75 |
| 2 | M11 Cross-sell | 0.50 |
| 3 | M13 BSI Score (widget en dashboard M0) | 0.25 |
| 4 | M10 Scorecard Aseguradoras | 0.50 |

### Sprint B (1.5 días) — Visión de cliente
| # | Módulo | Días |
|---|--------|------|
| 5 | M9 Cliente 360° | 1.50 |

### Sprint C (0.5 días) — Análisis estratégico
| # | Módulo | Días |
|---|--------|------|
| 6 | M12 Pareto 80/20 | 0.50 |

**Total estimado: ~4 días de desarrollo** para los 6 módulos.

---

## Patrones Reutilizables del Proyecto

### Score bars (ya en M7 SCSS)
```scss
.score-bar-wrap { background: rgba(255,255,255,.08); border-radius: 4px; height: 6px; }
.score-bar { height: 6px; border-radius: 4px; transition: width .4s ease; }
.score-bar--verde   { background: #10B981; }
.score-bar--amarillo { background: #F59E0B; }
.score-bar--rojo    { background: #EF4444; }
```

### Chips de estado (reutilizar de M7)
```scss
.chip { padding: 2px 8px; border-radius: 12px; font-size: .72rem; font-weight: 600; }
.chip.success { background: rgba(16,185,129,.15); color: #10B981; }
.chip.warning { background: rgba(245,158,11,.15); color: #F59E0B; }
.chip.danger  { background: rgba(239,68,68,.15);  color: #EF4444; }
.chip.info    { background: rgba(59,130,246,.15);  color: #3B82F6; }
```

### Grade badges (para M10)
```scss
.grade-a { background: rgba(16,185,129,.15); color: #10B981; }
.grade-b { background: rgba(59,130,246,.15);  color: #3B82F6; }
.grade-c { background: rgba(245,158,11,.15);  color: #F59E0B; }
.grade-d { background: rgba(239,68,68,.15);   color: #EF4444; }
```

---

## Checklist de Implementación por Módulo

Para cada módulo nuevo, ejecutar en orden:

- [ ] Agregar DTOs en `AnalyticsDtos.cs` dentro del namespace existente
- [ ] Agregar método `[HttpGet("nombre")]` en `AnalyticsController.cs` antes de `private static NormalizePrimaMensual`
- [ ] `cd backend ; dotnet build` → confirmar 0 errores
- [ ] Agregar interfaces y método en `analytics.service.ts`
- [ ] Crear `<nombre>.component.ts` standalone con `OnInit`, cargar datos en `ngOnInit()`
- [ ] Crear `<nombre>.component.html` con secciones y gráficos
- [ ] Crear `<nombre>.component.scss` con estilos
- [ ] Agregar ruta en `analytics.module.ts` con `loadComponent`
- [ ] Agregar nav item en `app.component.html` con `mat-icon` + `routerLink`
- [ ] `cd frontend-new ; npx ng build --configuration development` → confirmar 0 errores
- [ ] Actualizar tabla de estado en `analitica.md`
