using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using WebApi.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AnalyticsController> _logger;
        private readonly IAnalyticsReportsService _reportsService;
        private readonly IAnalyticsDashboardDomainService _dashboardService;
        private readonly IAnalyticsPortfolioDomainService _portfolioService;
        private readonly IAnalyticsReclamosDomainService _reclamosService;
        private readonly IEndpointPerformanceStore _endpointPerformanceStore;

        public AnalyticsController(
            ApplicationDbContext db,
            ILogger<AnalyticsController> logger,
            IAnalyticsReportsService reportsService,
            IAnalyticsDashboardDomainService dashboardService,
            IAnalyticsPortfolioDomainService portfolioService,
            IAnalyticsReclamosDomainService reclamosService,
            IEndpointPerformanceStore endpointPerformanceStore)
        {
            _db = db;
            _logger = logger;
            _reportsService = reportsService;
            _dashboardService = dashboardService;
            _portfolioService = portfolioService;
            _reclamosService = reclamosService;
            _endpointPerformanceStore = endpointPerformanceStore;
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÓDULO 0 — Dashboard Ejecutivo
        // GET /api/analytics/dashboard
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("dashboard")]
        public async Task<ActionResult<ExecutiveDashboardDto>> GetDashboard()
        {
            try
            {
                return Ok(await _dashboardService.GetDashboardAsync(HttpContext.RequestAborted));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo dashboard ejecutivo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÓDULO 1 — Cobros Analytics
        // GET /api/analytics/cobros/trend?months=18
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("cobros/trend")]
        public async Task<ActionResult<CobrosTrendDto>> GetCobrosTrend([FromQuery] int months = 18)
        {
            try
            {
                var desde = DateTime.UtcNow.AddMonths(-months);
                var cobros = await _db.Cobros
                    .Where(c => !c.IsDeleted && c.FechaVencimiento >= desde)
                    .Select(c => new { c.FechaVencimiento, c.MontoTotal, c.MontoCobrado, c.Estado, c.Moneda })
                    .ToListAsync();

                var grouped = cobros
                    .GroupBy(c => new { c.FechaVencimiento.Year, c.FechaVencimiento.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new CobroMensualDto
                    {
                        Mes        = $"{g.Key.Year}-{g.Key.Month:D2}",
                        MesLabel   = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        MontoEsperado  = g.Sum(c => c.MontoTotal),
                        MontoCobrado   = g.Sum(c => c.MontoCobrado),
                        MontoVencido   = g.Where(c => c.Estado == EstadoCobro.Vencido).Sum(c => c.MontoTotal),
                        CantidadCobros = g.Count()
                    }).ToList();

                return Ok(new CobrosTrendDto
                {
                    Mensual                = grouped,
                    PromedioMensualEsperado = grouped.Count > 0 ? grouped.Average(m => m.MontoEsperado) : 0,
                    PromedioMensualCobrado  = grouped.Count > 0 ? grouped.Average(m => m.MontoCobrado) : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo trend de cobros");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET /api/analytics/cobros/aging
        [HttpGet("cobros/aging")]
        public async Task<ActionResult<AgingReportDto>> GetCobrosAging()
        {
            try
            {
                var now = DateTime.UtcNow;
                var vencidos = await _db.Cobros
                    .Where(c => !c.IsDeleted && c.Estado == EstadoCobro.Vencido)
                    .Select(c => new { c.FechaVencimiento, c.MontoTotal, c.Moneda })
                    .ToListAsync();

                var totalVencido = vencidos.Sum(c => c.MontoTotal);

                var rangos = new[]
                {
                    (Label: "1-15 días",   Min:  1, Max:  15, Color: "status-pending"),
                    (Label: "16-30 días",  Min: 16, Max:  30, Color: "color-warning"),
                    (Label: "31-60 días",  Min: 31, Max:  60, Color: "status-overdue"),
                    (Label: "61-90 días",  Min: 61, Max:  90, Color: "color-error"),
                    (Label: "+90 días",    Min: 91, Max: int.MaxValue, Color: "color-error")
                };

                var buckets = rangos.Select(r =>
                {
                    var items = vencidos.Where(c =>
                    {
                        var dias = (int)(now - c.FechaVencimiento).TotalDays;
                        return dias >= r.Min && (r.Max == int.MaxValue || dias <= r.Max);
                    }).ToList();

                    var monto = items.Sum(c => c.MontoTotal);
                    return new AgingBucketDto
                    {
                        Rango    = r.Label,
                        DiasMin  = r.Min,
                        DiasMax  = r.Max == int.MaxValue ? -1 : r.Max,
                        Cantidad = items.Count,
                        Monto    = monto,
                        PorcentajeMonto = totalVencido > 0 ? Math.Round(monto / totalVencido * 100, 1) : 0,
                        Color    = r.Color
                    };
                }).ToList();

                return Ok(new AgingReportDto
                {
                    Buckets             = buckets,
                    TotalVencido        = totalVencido,
                    TotalCobrosVencidos = vencidos.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo aging de cobros");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET /api/analytics/cobros/metodos-pago
        [HttpGet("cobros/metodos-pago")]
        public async Task<ActionResult<IEnumerable<PagoMetodoDistribucionDto>>> GetCobrosMetodosPago()
        {
            try
            {
                var pagados = await _db.Cobros
                    .Where(c => !c.IsDeleted && (c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado))
                    .Select(c => new { c.MetodoPago, c.MontoCobrado })
                    .ToListAsync();

                var total = pagados.Count;
                var result = pagados
                    .GroupBy(c => c.MetodoPago.ToString())
                    .Select(g => new PagoMetodoDistribucionDto
                    {
                        MetodoPago  = g.Key,
                        Cantidad    = g.Count(),
                        Monto       = g.Sum(c => c.MontoCobrado),
                        Porcentaje  = total > 0 ? (decimal)Math.Round((double)g.Count() / total * 100, 1) : 0m
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo distribución métodos de pago");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET /api/analytics/cobros/cashflow-forecast?weeks=12
        [HttpGet("cobros/cashflow-forecast")]
        public async Task<ActionResult<CashflowForecastDto>> GetCashflowForecast([FromQuery] int weeks = 12)
        {
            try
            {
                var now = DateTime.UtcNow;
                var desde = now.Date;
                var hasta = desde.AddDays(weeks * 7);

                // Tasa histórica basada en últimos 6 meses
                var hace6Meses = now.AddMonths(-6);
                var historico = await _db.Cobros
                    .Where(c => !c.IsDeleted && c.FechaVencimiento >= hace6Meses && c.FechaVencimiento < now)
                    .Select(c => new { c.MontoTotal, c.MontoCobrado })
                    .ToListAsync();

                var totalHistEsperado = historico.Sum(c => c.MontoTotal);
                var totalHistCobrado  = historico.Sum(c => c.MontoCobrado);
                var tasaHistorica     = totalHistEsperado > 0 ? totalHistCobrado / totalHistEsperado : 0.9m;

                var futuros = await _db.Cobros
                    .Where(c => !c.IsDeleted && c.FechaVencimiento >= desde && c.FechaVencimiento < hasta)
                    .Select(c => new { c.FechaVencimiento, c.MontoTotal, c.MontoCobrado, c.Estado, c.Moneda })
                    .ToListAsync();

                var semanas = Enumerable.Range(0, weeks).Select(i =>
                {
                    var inicio = desde.AddDays(i * 7);
                    var fin    = inicio.AddDays(7);
                    var items  = futuros.Where(c => c.FechaVencimiento >= inicio && c.FechaVencimiento < fin).ToList();
                    var esFuturo = inicio > now;

                    return new CashflowSemanaDto
                    {
                        Semana      = $"Sem {i + 1}",
                        FechaInicio = inicio,
                        FechaFin    = fin,
                        MontoEsperado = items.Sum(c => c.MontoTotal),
                        MontoCobrado  = esFuturo
                            ? Math.Round(items.Sum(c => c.MontoTotal) * tasaHistorica, 0)
                            : items.Sum(c => c.MontoCobrado),
                        EsFuturo = esFuturo
                    };
                }).ToList();

                return Ok(new CashflowForecastDto
                {
                    Semanas          = semanas,
                    TotalProjectado  = semanas.Sum(s => s.MontoEsperado),
                    TotalCobrado     = semanas.Sum(s => s.MontoCobrado),
                    TasaHistorica    = Math.Round(tasaHistorica * 100, 1)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo forecast de cashflow");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET /api/analytics/cobros/top-deudores?top=10
        [HttpGet("cobros/top-deudores")]
        public async Task<ActionResult<IEnumerable<TopDeudorDto>>> GetTopDeudores([FromQuery] int top = 10)
        {
            try
            {
                var now = DateTime.UtcNow;
                var vencidos = await _db.Cobros
                    .Where(c => !c.IsDeleted && c.Estado == EstadoCobro.Vencido)
                    .Select(c => new { c.Id, c.ClienteNombreCompleto, c.CorreoElectronico, c.NumeroPoliza, c.MontoTotal, c.FechaVencimiento, c.Moneda })
                    .ToListAsync();

                var resultado = vencidos
                    .GroupBy(c => c.ClienteNombreCompleto)
                    .Select(g => new TopDeudorDto
                    {
                        ClienteNombre       = g.Key,
                        CorreoElectronico   = g.First().CorreoElectronico ?? "",
                        NumeroPolizas       = g.Select(c => c.NumeroPoliza).Distinct().Count(),
                        MontoVencidoTotal   = g.Sum(c => c.MontoTotal),
                        AntiguedadMaxDias   = g.Max(c => (int)(now - c.FechaVencimiento).TotalDays),
                        Moneda              = g.First().Moneda ?? "CRC",
                        CobroId             = g.OrderByDescending(c => c.MontoTotal).First().Id
                    })
                    .OrderByDescending(d => d.MontoVencidoTotal)
                    .Take(top)
                    .ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo top deudores");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET /api/analytics/cobros/heatmap
        // Distribución de pagos recibidos por día de semana × hora del día (1.5)
        [HttpGet("cobros/heatmap")]
        public async Task<ActionResult<IEnumerable<HeatmapCellDto>>> GetCobrosHeatmap()
        {
            try
            {
                var pagados = await _db.Cobros
                    .Where(c => !c.IsDeleted &&
                                (c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado))
                    .Select(c => new { c.FechaCobro })
                    .ToListAsync();

                var cells = pagados
                    .GroupBy(c => new
                    {
                        // 0 = Lunes … 6 = Domingo
                        DiaSemana = c.FechaCobro.DayOfWeek == DayOfWeek.Sunday
                            ? 6
                            : (int)c.FechaCobro.DayOfWeek - 1,
                        Hora = c.FechaCobro.Hour
                    })
                    .Select(g => new HeatmapCellDto
                    {
                        DiaSemana = g.Key.DiaSemana,
                        Hora      = g.Key.Hora,
                        Valor     = g.Count()
                    })
                    .OrderBy(c => c.DiaSemana).ThenBy(c => c.Hora)
                    .ToList();

                return Ok(cells);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo heatmap de cobros");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET /api/analytics/cobros/por-agente
        // Rendimiento de cobro por agente (1.7)
        [HttpGet("cobros/por-agente")]
        public async Task<ActionResult<IEnumerable<AgenteCobrosDto>>> GetCobrosPorAgente()
        {
            try
            {
                var cobros = await _db.Cobros
                    .Where(c => !c.IsDeleted &&
                                (c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado) &&
                                !string.IsNullOrEmpty(c.UsuarioCobroNombre))
                    .Select(c => new
                    {
                        c.UsuarioCobroNombre,
                        c.MontoCobrado,
                        c.FechaVencimiento,
                        c.FechaCobro
                    })
                    .ToListAsync();

                var result = cobros
                    .GroupBy(c => c.UsuarioCobroNombre)
                    .Select(g =>
                    {
                        // Días desde vencimiento hasta cobro efectivo (0 si se cobró antes)
                        var demoras = g
                            .Select(c => Math.Max(0.0, (c.FechaCobro - c.FechaVencimiento).TotalDays))
                            .ToList();

                        return new AgenteCobrosDto
                        {
                            NombreAgente          = g.Key,
                            CobrosProcesados      = g.Count(),
                            MontoCobrado          = Math.Round(g.Sum(c => c.MontoCobrado), 0),
                            TiempoPromedioResDias = demoras.Count > 0
                                ? Math.Round(demoras.Average(), 1)
                                : 0
                        };
                    })
                    .OrderByDescending(a => a.MontoCobrado)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cobros por agente");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÓDULO 2 — Portfolio de Pólizas
        // GET /api/analytics/portfolio
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("portfolio")]
        public async Task<ActionResult<PortfolioDistribucionDto>> GetPortfolio()
        {
            try
            {
                var now = DateTime.UtcNow;

                var polizasActivas = await _db.Polizas
                    .Where(p => p.EsActivo && !p.IsDeleted)
                    .Select(p => new
                    {
                        p.Id,
                        p.NumeroPoliza,
                        p.NombreAsegurado,
                        p.NumeroCedula,
                        p.Aseguradora,
                        p.Modalidad,
                        p.Frecuencia,
                        p.Prima,
                        p.FechaVigencia,
                        p.CreatedAt
                    })
                    .ToListAsync();

                if (!polizasActivas.Any())
                {
                    return Ok(new PortfolioDistribucionDto());
                }

                var cotizaciones = await _db.Cotizaciones
                    .Where(c => !c.IsDeleted)
                    .Select(c => new
                    {
                        c.NumeroPoliza,
                        c.TipoSeguro,
                        c.Prima,
                        c.FechaCreacion
                    })
                    .ToListAsync();

                var tipoSeguroByPoliza = cotizaciones
                    .Where(c => !string.IsNullOrWhiteSpace(c.NumeroPoliza) && !string.IsNullOrWhiteSpace(c.TipoSeguro))
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(x => x.FechaCreacion).Select(x => x.TipoSeguro).FirstOrDefault() ?? "OTROS",
                        StringComparer.OrdinalIgnoreCase);

                var montoAseguradoProxyByPoliza = cotizaciones
                    .Where(c => !string.IsNullOrWhiteSpace(c.NumeroPoliza))
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => Math.Round(g.Average(x => x.Prima) * 12m, 2),
                        StringComparer.OrdinalIgnoreCase);

                var polizas = polizasActivas
                    .Select(p => new
                    {
                        p.Id,
                        p.NumeroPoliza,
                        p.NombreAsegurado,
                        p.NumeroCedula,
                        Aseguradora = string.IsNullOrWhiteSpace(p.Aseguradora) ? "OTROS" : p.Aseguradora!,
                        Modalidad = string.IsNullOrWhiteSpace(p.Modalidad) ? "SIN_MODALIDAD" : p.Modalidad!,
                        Frecuencia = string.IsNullOrWhiteSpace(p.Frecuencia) ? "MENSUAL" : p.Frecuencia!,
                        p.FechaVigencia,
                        p.CreatedAt,
                        PrimaMensual = NormalizePrimaMensual(p.Frecuencia, p.Prima),
                        TipoSeguro = NormalizeTipoSeguro(
                            !string.IsNullOrWhiteSpace(p.NumeroPoliza) && tipoSeguroByPoliza.TryGetValue(p.NumeroPoliza, out var tipo)
                                ? tipo
                                : null,
                            p.Modalidad)
                    })
                    .ToList();

                var cobrosPorPoliza = await _db.Cobros
                    .Where(c => !c.IsDeleted && !string.IsNullOrWhiteSpace(c.NumeroPoliza))
                    .GroupBy(c => c.NumeroPoliza!)
                    .Select(g => new
                    {
                        NumeroPoliza = g.Key,
                        TotalEsperado = g.Sum(x => x.MontoTotal),
                        TotalCobrado = g.Sum(x => x.MontoCobrado)
                    })
                    .ToListAsync();

                var cobrosPorPolizaDict = cobrosPorPoliza
                    .ToDictionary(x => x.NumeroPoliza, x => new { x.TotalEsperado, x.TotalCobrado }, StringComparer.OrdinalIgnoreCase);

                var reclamosPorPoliza = await _db.Reclamos
                    .Where(r => !r.IsDeleted && !string.IsNullOrWhiteSpace(r.NumeroPoliza))
                    .GroupBy(r => r.NumeroPoliza!)
                    .Select(g => new { NumeroPoliza = g.Key, Total = g.Count() })
                    .ToListAsync();

                var reclamosPorPolizaDict = reclamosPorPoliza
                    .ToDictionary(x => x.NumeroPoliza, x => x.Total, StringComparer.OrdinalIgnoreCase);

                List<PortfolioBreakdownDto> BuildBreakdown<T>(
                    IEnumerable<T> source,
                    Func<T, string> keySelector,
                    Func<T, decimal> primaSelector)
                {
                    var items = source.ToList();
                    var totalPolizas = items.Count;
                    var totalPrima = items.Sum(primaSelector);

                    return items
                        .GroupBy(keySelector)
                        .Select(g =>
                        {
                            var prima = g.Sum(primaSelector);
                            return new PortfolioBreakdownDto
                            {
                                Name = g.Key,
                                TotalPolizas = g.Count(),
                                PrimaMensual = Math.Round(prima, 2),
                                PorcentajePolizas = totalPolizas > 0
                                    ? Math.Round((double)g.Count() / totalPolizas * 100, 1)
                                    : 0,
                                PorcentajePrima = totalPrima > 0
                                    ? Math.Round((double)(prima / totalPrima * 100m), 1)
                                    : 0
                            };
                        })
                        .OrderByDescending(x => x.PrimaMensual)
                        .ToList();
                }

                var composicionAseguradora = BuildBreakdown(polizas, p => p.Aseguradora, p => p.PrimaMensual);
                var composicionTipoSeguro = BuildBreakdown(polizas, p => p.TipoSeguro, p => p.PrimaMensual);
                var composicionModalidad = BuildBreakdown(polizas, p => p.Modalidad, p => p.PrimaMensual);
                var composicionFrecuencia = BuildBreakdown(polizas, p => p.Frecuencia, p => p.PrimaMensual);

                var radar = polizas
                    .GroupBy(p => p.Aseguradora)
                    .Select(g =>
                    {
                        var numerosPoliza = g
                            .Where(x => !string.IsNullOrWhiteSpace(x.NumeroPoliza))
                            .Select(x => x.NumeroPoliza!)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        decimal esperado = 0;
                        decimal cobrado = 0;
                        var totalReclamos = 0;

                        foreach (var numero in numerosPoliza)
                        {
                            if (cobrosPorPolizaDict.TryGetValue(numero, out var cobrosPoliza))
                            {
                                esperado += cobrosPoliza.TotalEsperado;
                                cobrado += cobrosPoliza.TotalCobrado;
                            }

                            if (reclamosPorPolizaDict.TryGetValue(numero, out var reclamosPoliza))
                            {
                                totalReclamos += reclamosPoliza;
                            }
                        }

                        var totalPolizas = g.Count();
                        return new RadarDataDto
                        {
                            Aseguradora = g.Key,
                            PrimaPromedio = totalPolizas > 0 ? Math.Round(g.Average(x => x.PrimaMensual), 2) : 0,
                            NumPolizas = totalPolizas,
                            TasaReclamos = totalPolizas > 0 ? Math.Round((decimal)totalReclamos / totalPolizas * 100m, 1) : 0,
                            TasaCobro = esperado > 0 ? Math.Round(cobrado / esperado * 100m, 1) : 0,
                            AntiguedadPromedioDias = totalPolizas > 0 ? Math.Round(g.Average(x => (now - x.CreatedAt).TotalDays), 1) : 0
                        };
                    })
                    .OrderByDescending(x => x.NumPolizas)
                    .ToList();

                var polizasHistoricas = await _db.Polizas
                    .Where(p => !p.IsDeleted)
                    .Select(p => new
                    {
                        p.Id,
                        p.NumeroPoliza,
                        p.NombreAsegurado,
                        p.NumeroCedula,
                        p.Aseguradora,
                        p.Modalidad,
                        p.Frecuencia,
                        p.Prima,
                        p.FechaVigencia
                    })
                    .ToListAsync();

                var polizasHistoricasEnriquecidas = polizasHistoricas
                    .Select(p => new
                    {
                        p.Id,
                        p.NumeroPoliza,
                        p.NombreAsegurado,
                        p.NumeroCedula,
                        Aseguradora = string.IsNullOrWhiteSpace(p.Aseguradora) ? "OTROS" : p.Aseguradora!,
                        TipoSeguro = NormalizeTipoSeguro(
                            !string.IsNullOrWhiteSpace(p.NumeroPoliza) && tipoSeguroByPoliza.TryGetValue(p.NumeroPoliza, out var tipo)
                                ? tipo
                                : null,
                            p.Modalidad),
                        p.FechaVigencia
                    })
                    .ToList();

                var inicioRetencion = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
                var vencidasPeriodo = polizasHistoricasEnriquecidas
                    .Where(p => p.FechaVigencia >= inicioRetencion && p.FechaVigencia <= now)
                    .ToList();

                var renovacionEval = vencidasPeriodo
                    .Select(v =>
                    {
                        var hasCedula = !string.IsNullOrWhiteSpace(v.NumeroCedula);
                        var hasNombre = !string.IsNullOrWhiteSpace(v.NombreAsegurado);

                        var renovada = polizasHistoricasEnriquecidas.Any(c =>
                            c.Id != v.Id
                            && c.FechaVigencia > v.FechaVigencia
                            && c.FechaVigencia <= v.FechaVigencia.AddMonths(6)
                            && (
                                (hasCedula && c.NumeroCedula == v.NumeroCedula)
                                || (!hasCedula && hasNombre && c.NombreAsegurado == v.NombreAsegurado)
                            ));

                        return new
                        {
                            Mes = new DateTime(v.FechaVigencia.Year, v.FechaVigencia.Month, 1),
                            v.Aseguradora,
                            v.TipoSeguro,
                            Renovada = renovada
                        };
                    })
                    .ToList();

                var totalVencidas = renovacionEval.Count;
                var totalRenovadas = renovacionEval.Count(x => x.Renovada);
                var totalNoRenovadas = totalVencidas - totalRenovadas;

                var tasaRetencionMensual = Enumerable.Range(0, 12)
                    .Select(i => inicioRetencion.AddMonths(i))
                    .Select(mes =>
                    {
                        var mesItems = renovacionEval
                            .Where(x => x.Mes.Year == mes.Year && x.Mes.Month == mes.Month)
                            .ToList();
                        var pct = mesItems.Count > 0
                            ? Math.Round((decimal)mesItems.Count(x => x.Renovada) / mesItems.Count * 100m, 1)
                            : 0;

                        return new ChartDataPointDto
                        {
                            Name = mes.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                            Value = pct
                        };
                    })
                    .ToList();

                var retencionPorTipoSeguro = renovacionEval
                    .GroupBy(x => x.TipoSeguro)
                    .Select(g => new ChartSeriesDto
                    {
                        Name = g.Key,
                        Value = g.Any() ? Math.Round((decimal)g.Count(x => x.Renovada) / g.Count() * 100m, 1) : 0
                    })
                    .OrderByDescending(x => x.Value)
                    .ToList();

                var churnPorAseguradora = renovacionEval
                    .Where(x => !x.Renovada)
                    .GroupBy(x => x.Aseguradora)
                    .Select(g => new ChartSeriesDto { Name = g.Key, Value = g.Count() })
                    .OrderByDescending(x => x.Value)
                    .ToList();

                var retencion = new RetencionRenovacionDto
                {
                    TotalVencidas = totalVencidas,
                    TotalRenovadas = totalRenovadas,
                    TotalNoRenovadas = totalNoRenovadas,
                    TasaRetencionPromedio = totalVencidas > 0
                        ? Math.Round((double)totalRenovadas / totalVencidas * 100, 1)
                        : 0,
                    Funnel = new List<ChartSeriesDto>
                    {
                        new() { Name = "Vencidas", Value = totalVencidas },
                        new() { Name = "Renovadas", Value = totalRenovadas },
                        new() { Name = "No renovadas", Value = totalNoRenovadas }
                    },
                    TasaRetencionMensual = tasaRetencionMensual,
                    RetencionPorTipoSeguro = retencionPorTipoSeguro,
                    ChurnPorAseguradora = churnPorAseguradora
                };

                var bucketDefs = new (string Label, decimal Min, decimal? Max)[]
                {
                    ("₡0-50k", 0m, 50000m),
                    ("₡50k-100k", 50000m, 100000m),
                    ("₡100k-200k", 100000m, 200000m),
                    ("₡200k-500k", 200000m, 500000m),
                    ("₡500k+", 500000m, null)
                };

                var histograma = bucketDefs
                    .Select(b =>
                    {
                        var items = polizas
                            .Where(p => p.PrimaMensual >= b.Min && (!b.Max.HasValue || p.PrimaMensual < b.Max.Value))
                            .ToList();

                        return new HistogramaPrimaBucketDto
                        {
                            Rango = b.Label,
                            Min = b.Min,
                            Max = b.Max ?? decimal.MaxValue,
                            Cantidad = items.Count,
                            PrimaMensualTotal = Math.Round(items.Sum(p => p.PrimaMensual), 2)
                        };
                    })
                    .ToList();

                var mapaRiesgo = polizas
                    .GroupBy(p => new { p.Aseguradora, p.TipoSeguro })
                    .Select(g =>
                    {
                        var clients = g
                            .Select(x => !string.IsNullOrWhiteSpace(x.NumeroCedula) ? x.NumeroCedula! : x.NombreAsegurado ?? string.Empty)
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count();

                        var proxy = g.Select(x =>
                        {
                            decimal value;
                            if (!string.IsNullOrWhiteSpace(x.NumeroPoliza) && montoAseguradoProxyByPoliza.TryGetValue(x.NumeroPoliza, out var val))
                            {
                                value = val;
                            }
                            else
                            {
                                value = x.PrimaMensual * 12m;
                            }

                            return value;
                        }).ToList();

                        return new RiesgoConcentracionDto
                        {
                            Segmento = $"{g.Key.Aseguradora} · {g.Key.TipoSeguro}",
                            Aseguradora = g.Key.Aseguradora,
                            TipoSeguro = g.Key.TipoSeguro,
                            PrimaUnitariaPromedio = Math.Round(g.Average(x => x.PrimaMensual), 2),
                            MontoAseguradoProxy = proxy.Count > 0 ? Math.Round(proxy.Average(), 2) : 0,
                            NumeroClientes = clients
                        };
                    })
                    .OrderByDescending(x => x.NumeroClientes)
                    .ThenByDescending(x => x.MontoAseguradoProxy)
                    .ToList();

                return Ok(new PortfolioDistribucionDto
                {
                    PorAseguradora = composicionAseguradora.Select(x => new ChartSeriesDto { Name = x.Name, Value = x.PrimaMensual }).ToList(),
                    PorTipoSeguro = composicionTipoSeguro.Select(x => new ChartSeriesDto { Name = x.Name, Value = x.PrimaMensual }).ToList(),
                    PorModalidad = composicionModalidad.Select(x => new ChartSeriesDto { Name = x.Name, Value = x.TotalPolizas }).ToList(),
                    PorFrecuencia = composicionFrecuencia.Select(x => new ChartSeriesDto { Name = x.Name, Value = x.TotalPolizas }).ToList(),
                    ComposicionPorAseguradora = composicionAseguradora,
                    ComposicionPorTipoSeguro = composicionTipoSeguro,
                    ComposicionPorModalidad = composicionModalidad,
                    ComposicionPorFrecuencia = composicionFrecuencia,
                    RadarAseguradoras = radar,
                    Retencion = retencion,
                    HistogramaPrima = histograma,
                    MapaRiesgo = mapaRiesgo,
                    PrimaMensualTotal = Math.Round(polizas.Sum(x => x.PrimaMensual), 2),
                    TotalPolizasActivas = polizas.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo portfolio");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET /api/analytics/portfolio/vencimientos?months=12
        [HttpGet("portfolio/vencimientos")]
        public async Task<ActionResult<VencimientosTimelineDto>> GetVencimientos([FromQuery] int months = 12)
        {
            try
            {
                months = Math.Clamp(months, 1, 24);

                var now = DateTime.UtcNow;
                var inicioMes = new DateTime(now.Year, now.Month, 1);
                var finHorizonte = inicioMes.AddMonths(months);

                var cotizaciones = await _db.Cotizaciones
                    .Where(c => !c.IsDeleted)
                    .Select(c => new { c.NumeroPoliza, c.TipoSeguro, c.FechaCreacion })
                    .ToListAsync();

                var tipoSeguroByPoliza = cotizaciones
                    .Where(c => !string.IsNullOrWhiteSpace(c.NumeroPoliza) && !string.IsNullOrWhiteSpace(c.TipoSeguro))
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(x => x.FechaCreacion).Select(x => x.TipoSeguro).FirstOrDefault() ?? "OTROS",
                        StringComparer.OrdinalIgnoreCase);

                var polizas = await _db.Polizas
                    .Where(p => p.EsActivo
                        && !p.IsDeleted
                        && p.FechaVigencia >= now
                        && p.FechaVigencia < finHorizonte)
                    .Select(p => new
                    {
                        p.NumeroPoliza,
                        p.Modalidad,
                        p.FechaVigencia,
                        p.Prima,
                        p.Frecuencia
                    })
                    .ToListAsync();

                var porMes = Enumerable.Range(0, months)
                    .Select(i => inicioMes.AddMonths(i))
                    .Select(mes =>
                    {
                        var finMes = mes.AddMonths(1);
                        var itemsMes = polizas
                            .Where(p => p.FechaVigencia >= mes && p.FechaVigencia < finMes)
                            .Select(p => new
                            {
                                TipoSeguro = NormalizeTipoSeguro(
                                    !string.IsNullOrWhiteSpace(p.NumeroPoliza) && tipoSeguroByPoliza.TryGetValue(p.NumeroPoliza, out var tipo)
                                        ? tipo
                                        : null,
                                    p.Modalidad),
                                PrimaMensual = NormalizePrimaMensual(p.Frecuencia, p.Prima)
                            })
                            .ToList();

                        return new VencimientoMesDto
                        {
                            Mes = $"{mes.Year}-{mes.Month:D2}",
                            MesLabel = mes.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                            TotalPolizas = itemsMes.Count,
                            Auto = itemsMes.Count(x => x.TipoSeguro == "AUTO"),
                            Vida = itemsMes.Count(x => x.TipoSeguro == "VIDA"),
                            Hogar = itemsMes.Count(x => x.TipoSeguro == "HOGAR"),
                            Empresarial = itemsMes.Count(x => x.TipoSeguro == "EMPRESARIAL"),
                            PrimaEnRiesgo = Math.Round(itemsMes.Sum(x => x.PrimaMensual), 2)
                        };
                    })
                    .ToList();

                return Ok(new VencimientosTimelineDto
                {
                    Meses = porMes,
                    TotalPolizasProximasAnio = polizas.Count,
                    PrimaEnRiesgoAnio = Math.Round(porMes.Sum(x => x.PrimaEnRiesgo), 2)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo timeline de vencimientos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET /api/analytics/portfolio/vencimientos/detalle?month=2026-04
        [HttpGet("portfolio/vencimientos/detalle")]
        public async Task<ActionResult<VencimientoDetalleMesDto>> GetVencimientosDetalle([FromQuery] string month)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(month)
                    || !DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedMonth))
                {
                    return BadRequest("Parámetro month inválido. Formato esperado: yyyy-MM");
                }

                var inicioMes = new DateTime(parsedMonth.Year, parsedMonth.Month, 1);
                var finMes = inicioMes.AddMonths(1);

                var cotizaciones = await _db.Cotizaciones
                    .Where(c => !c.IsDeleted)
                    .Select(c => new { c.NumeroPoliza, c.TipoSeguro, c.FechaCreacion })
                    .ToListAsync();

                var tipoSeguroByPoliza = cotizaciones
                    .Where(c => !string.IsNullOrWhiteSpace(c.NumeroPoliza) && !string.IsNullOrWhiteSpace(c.TipoSeguro))
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(x => x.FechaCreacion).Select(x => x.TipoSeguro).FirstOrDefault() ?? "OTROS",
                        StringComparer.OrdinalIgnoreCase);

                var polizas = await _db.Polizas
                    .Where(p => p.EsActivo
                        && !p.IsDeleted
                        && p.FechaVigencia >= inicioMes
                        && p.FechaVigencia < finMes)
                    .Select(p => new
                    {
                        p.NumeroPoliza,
                        p.NombreAsegurado,
                        p.Aseguradora,
                        p.Modalidad,
                        p.FechaVigencia,
                        p.Prima,
                        p.Frecuencia
                    })
                    .OrderBy(p => p.FechaVigencia)
                    .ToListAsync();

                var detalle = polizas.Select(p => new VencimientoPolizaDetalleDto
                {
                    NumeroPoliza = p.NumeroPoliza ?? string.Empty,
                    ClienteNombre = p.NombreAsegurado ?? string.Empty,
                    Aseguradora = p.Aseguradora ?? "OTROS",
                    TipoSeguro = NormalizeTipoSeguro(
                        !string.IsNullOrWhiteSpace(p.NumeroPoliza) && tipoSeguroByPoliza.TryGetValue(p.NumeroPoliza, out var tipo)
                            ? tipo
                            : null,
                        p.Modalidad),
                    FechaVigencia = p.FechaVigencia,
                    PrimaMensualNormalizada = Math.Round(NormalizePrimaMensual(p.Frecuencia, p.Prima), 2)
                }).ToList();

                return Ok(new VencimientoDetalleMesDto
                {
                    Mes = month,
                    MesLabel = inicioMes.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                    Polizas = detalle
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalle de vencimientos del mes {Month}", month);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÓDULO 3 — Reclamos Analytics
        // GET /api/analytics/reclamos/funnel
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("reclamos/funnel")]
        public async Task<ActionResult<ReclamosFunnelDto>> GetReclamosFunnel()
        {
            try
            {
                var reclamos = await _db.Reclamos
                    .Where(r => !r.IsDeleted)
                    .Select(r => new { r.Estado, r.MontoReclamado, r.MontoAprobado, r.FechaReclamo, r.FechaResolucion })
                    .ToListAsync();

                var total = reclamos.Count;
                var ordenEstados = new[]
                {
                    ("Recibido",    new[] { "Pendiente" }),
                    ("Abierto",     new[] { "Abierto" }),
                    ("En Revisión", new[] { "EnRevision" }),
                    ("En Proceso",  new[] { "EnProceso" }),
                    ("Aprobado",    new[] { "Aprobado" }),
                    ("Resuelto",    new[] { "Resuelto" }),
                    ("Rechazado",   new[] { "Rechazado"  }),
                    ("Cerrado",     new[] { "Cerrado" })
                };

                var etapas = ordenEstados.Select(e =>
                {
                    var items = reclamos.Where(r => e.Item2.Contains(r.Estado.ToString())).ToList();
                    double tiempoPromedio = 0;
                    if (items.Any())
                    {
                        var conFecha = items.Where(r => r.FechaResolucion.HasValue).ToList();
                        if (conFecha.Any())
                            tiempoPromedio = conFecha.Average(r => (r.FechaResolucion!.Value - r.FechaReclamo).TotalHours);
                    }
                    return new FunnelEtapaDto
                    {
                        Nombre            = e.Item1,
                        Estado            = e.Item2[0],
                        Cantidad          = items.Count,
                        Monto             = items.Sum(r => r.MontoReclamado),
                        TiempoPromedioHoras = Math.Round(tiempoPromedio, 1),
                        PorcentajeDel100  = total > 0 ? Math.Round((double)items.Count / total * 100, 1) : 0
                    };
                }).ToList();

                var totalReclamado = reclamos.Sum(r => r.MontoReclamado);
                var totalAprobado  = reclamos.Sum(r => r.MontoAprobado ?? 0);

                return Ok(new ReclamosFunnelDto
                {
                    Etapas               = etapas,
                    MontoTotalReclamado  = totalReclamado,
                    MontoTotalAprobado   = totalAprobado,
                    LossRatioGlobal      = totalReclamado > 0 ? Math.Round(totalAprobado / totalReclamado * 100, 1) : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo funnel de reclamos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET /api/analytics/reclamos/sla
        [HttpGet("reclamos/sla")]
        public async Task<ActionResult<SlaReportDto>> GetReclamosSla()
        {
            try
            {
                var now = DateTime.UtcNow;
                var reclamos = await _db.Reclamos
                    .Where(r => !r.IsDeleted)
                    .Select(r => new
                    {
                        r.Id, r.NumeroReclamo, r.Estado, r.TipoReclamo, r.Prioridad,
                        r.FechaReclamo, r.FechaLimiteRespuesta, r.FechaResolucion,
                        r.UsuarioAsignadoId, r.ClienteNombreCompleto, r.MontoAprobado
                    })
                    .ToListAsync();

                var usuarios = await _db.Users
                    .Where(u => !u.IsDeleted)
                    .Select(u => new { u.Id, Nombre = u.FirstName + " " + u.LastName })
                    .ToDictionaryAsync(u => u.Id, u => u.Nombre);

                var conSla = reclamos.Where(r => r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue).ToList();
                var globalPct = conSla.Count > 0
                    ? Math.Round((double)conSla.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value) / conSla.Count * 100, 1)
                    : 100;

                // Por tipo
                var porTipo = reclamos
                    .GroupBy(r => r.TipoReclamo.ToString())
                    .Select(g =>
                    {
                        var conSlaG = g.Where(r => r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue).ToList();
                        var dentroG = conSlaG.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value);
                        var conResol = g.Where(r => r.FechaResolucion.HasValue).ToList();
                        return new SlaPorTipoDto
                        {
                            Nombre               = g.Key,
                            TotalReclamos        = g.Count(),
                            DentroSla            = dentroG,
                            FueraSla             = conSlaG.Count - dentroG,
                            PorcentajeDentroSla  = conSlaG.Count > 0 ? Math.Round((double)dentroG / conSlaG.Count * 100, 1) : 100,
                            TiempoPromedioHoras  = conResol.Count > 0 ? Math.Round(conResol.Average(r => (r.FechaResolucion!.Value - r.FechaReclamo).TotalHours), 1) : 0
                        };
                    }).ToList();

                // Por prioridad
                var porPrioridad = reclamos
                    .GroupBy(r => r.Prioridad.ToString())
                    .Select(g =>
                    {
                        var conSlaG = g.Where(r => r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue).ToList();
                        var dentroG = conSlaG.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value);
                        return new SlaPorTipoDto
                        {
                            Nombre              = g.Key,
                            TotalReclamos       = g.Count(),
                            DentroSla           = dentroG,
                            FueraSla            = conSlaG.Count - dentroG,
                            PorcentajeDentroSla = conSlaG.Count > 0 ? Math.Round((double)dentroG / conSlaG.Count * 100, 1) : 100
                        };
                    }).ToList();

                // Por agente
                var porAgente = reclamos
                    .Where(r => r.UsuarioAsignadoId.HasValue)
                    .GroupBy(r => r.UsuarioAsignadoId!.Value)
                    .Select(g =>
                    {
                        var nombre = usuarios.TryGetValue(g.Key, out var n) ? n : $"Agente {g.Key}";
                        var conSlaG = g.Where(r => r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue).ToList();
                        var dentroG = conSlaG.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value);
                        return new SlaPorAgenteDto
                        {
                            NombreAgente        = nombre,
                            TotalAsignados      = g.Count(),
                            DentroSla           = dentroG,
                            PorcentajeDentroSla = conSlaG.Count > 0 ? Math.Round((double)dentroG / conSlaG.Count * 100, 1) : 100,
                            TiempoPromedioHoras = g.Where(r => r.FechaResolucion.HasValue).Any()
                                ? Math.Round(g.Where(r => r.FechaResolucion.HasValue).Average(r => (r.FechaResolucion!.Value - r.FechaReclamo).TotalHours), 1)
                                : 0,
                            MontoAprobado = g.Sum(r => r.MontoAprobado ?? 0)
                        };
                    }).ToList();

                // Próximos a vencer SLA (activos)
                var proximosVencer = reclamos
                    .Where(r => r.FechaLimiteRespuesta.HasValue
                        && r.FechaLimiteRespuesta.Value > now
                        && r.Estado != EstadoReclamo.Resuelto
                        && r.Estado != EstadoReclamo.Cerrado
                        && (r.FechaLimiteRespuesta.Value - now).TotalHours <= 24)
                    .Select(r => new ReclamoAlertoSlaDto
                    {
                        Id              = r.Id,
                        NumeroReclamo   = r.NumeroReclamo,
                        ClienteNombre   = r.ClienteNombreCompleto,
                        FechaLimite     = r.FechaLimiteRespuesta!.Value,
                        HorasRestantes  = (int)(r.FechaLimiteRespuesta.Value - now).TotalHours,
                        Prioridad       = r.Prioridad.ToString()
                    })
                    .OrderBy(r => r.HorasRestantes)
                    .ToList();

                return Ok(new SlaReportDto
                {
                    PorcentajeGlobalDentroSLA = globalPct,
                    PorTipo        = porTipo,
                    PorPrioridad   = porPrioridad,
                    PorAgente      = porAgente,
                    ProximosVencer = proximosVencer
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo SLA de reclamos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET /api/analytics/reclamos/advanced?months=12&agenteId=1&aseguradora=INS
        [HttpGet("reclamos/advanced")]
        public async Task<ActionResult<ReclamosAnalyticsAdvancedDto>> GetReclamosAdvanced(
            [FromQuery] int months = 12,
            [FromQuery] int? agenteId = null,
            [FromQuery] string? aseguradora = null)
        {
            try
            {
                return Ok(await _reclamosService.GetReclamosAdvancedAsync(
                    months,
                    agenteId,
                    aseguradora,
                    HttpContext.RequestAborted));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo analitica avanzada de reclamos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÓDULO 4 — Cotizaciones / Sales Funnel
        // GET /api/analytics/cotizaciones/funnel
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("cotizaciones/funnel")]
        public async Task<ActionResult<CotizacionesFunnelDto>> GetCotizacionesFunnel()
        {
            try
            {
                var cotizaciones = await _db.Cotizaciones
                    .Where(c => !c.IsDeleted)
                    .Select(c => new {
                        c.Estado, c.TipoSeguro, c.Modalidad, c.Prima,
                        c.FechaCotizacion, c.FechaCreacion, c.FechaActualizacion,
                        c.Frecuencia, c.Aseguradora
                    })
                    .ToListAsync();

                var total       = cotizaciones.Count;
                var pendientes  = cotizaciones.Count(c => c.Estado == "PENDIENTE");
                var aprobadas   = cotizaciones.Count(c => c.Estado == "APROBADA");
                var convertidas = cotizaciones.Count(c => c.Estado == "CONVERTIDA");
                var rechazadas  = cotizaciones.Count(c => c.Estado == "RECHAZADA");

                // 4.2 Velocidad: days from FechaCreacion to FechaActualizacion (proxied conversion date)
                var convertidosConFecha = cotizaciones
                    .Where(c => c.Estado == "CONVERTIDA" && c.FechaActualizacion.HasValue)
                    .Select(c => (c.FechaActualizacion!.Value - c.FechaCreacion).TotalDays)
                    .Where(d => d >= 0).ToList();
                var velocidadPromedioDias = convertidosConFecha.Count > 0 ? Math.Round(convertidosConFecha.Average(), 1) : 0;

                var bucketRanges = new[] { (0, 7, "0-7 días"), (8, 15, "8-15 días"), (16, 30, "16-30 días"), (31, 60, "31-60 días"), (61, int.MaxValue, "+60 días") };
                var velocidadBuckets = bucketRanges.Select(b => new VelocidadBucketDto
                {
                    Rango    = b.Item3,
                    Cantidad = convertidosConFecha.Count(d => d >= b.Item1 && d <= b.Item2)
                }).ToList();

                // 4.4 Rechazadas por aseguradora
                var porAseguradora = cotizaciones
                    .Where(c => c.Estado == "RECHAZADA")
                    .GroupBy(c => string.IsNullOrWhiteSpace(c.Aseguradora) ? "Sin aseguradora" : c.Aseguradora)
                    .Select(g => new ChartSeriesDto { Name = g.Key, Value = g.Count() })
                    .OrderByDescending(x => x.Value).ToList();

                // 4.1 Conversiones por tipo de seguro
                var porTipo = cotizaciones
                    .GroupBy(c => string.IsNullOrEmpty(c.TipoSeguro) ? "Sin Tipo" : c.TipoSeguro)
                    .Select(g => new ChartSeriesDto
                    {
                        Name  = g.Key,
                        Value = g.Count(c => c.Estado == "CONVERTIDA")
                    })
                    .OrderByDescending(x => x.Value).ToList();

                // 4.5 Ticket promedio
                var ticketPromedio = cotizaciones
                    .Where(c => c.Prima > 0)
                    .GroupBy(c => new { c.TipoSeguro, c.Modalidad })
                    .Select(g => new TicketPromedioDto
                    {
                        TipoSeguro    = g.Key.TipoSeguro ?? "Sin Tipo",
                        Modalidad     = g.Key.Modalidad ?? "Sin Modalidad",
                        PrimaPromedio = Math.Round(g.Average(c => c.Prima), 0),
                        Cantidad      = g.Count()
                    })
                    .OrderByDescending(x => x.PrimaPromedio).ToList();

                // 4.3 Pipeline de valor mensual
                var pipelineValor = cotizaciones
                    .GroupBy(c => new { c.FechaCreacion.Year, c.FechaCreacion.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Take(12)
                    .Select(g =>
                    {
                        static decimal factorAnual(string? freq) => freq switch
                        {
                            "MENSUAL" => 12, "TRIMESTRAL" => 4, "SEMESTRAL" => 2, _ => 1
                        };
                        return new PipelineValorDto
                        {
                            Mes                  = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                            ValorPendiente       = g.Where(c => c.Estado == "PENDIENTE").Sum(c => c.Prima),
                            ValorAprobado        = g.Where(c => c.Estado == "APROBADA").Sum(c => c.Prima),
                            ValorConvertido      = g.Where(c => c.Estado == "CONVERTIDA").Sum(c => c.Prima),
                            ValorAnualProyectado = g.Sum(c => c.Prima * factorAnual(c.Frecuencia))
                        };
                    }).ToList();

                return Ok(new CotizacionesFunnelDto
                {
                    Total                = total,
                    Pendientes           = pendientes,
                    Aprobadas            = aprobadas,
                    Convertidas          = convertidas,
                    Rechazadas           = rechazadas,
                    TasaAprobacion       = total > 0 ? Math.Round((double)(aprobadas + convertidas) / total * 100, 1) : 0,
                    TasaConversion       = total > 0 ? Math.Round((double)convertidas / total * 100, 1) : 0,
                    VelocidadPromedioDias = velocidadPromedioDias,
                    PorTipoSeguro        = porTipo,
                    PorAseguradora       = porAseguradora,
                    VelocidadBuckets     = velocidadBuckets,
                    TicketPromedio       = ticketPromedio,
                    PipelineValor        = pipelineValor
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo funnel de cotizaciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÓDULO 5 — Email Analytics
        // GET /api/analytics/emails?days=30
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("emails")]
        public async Task<ActionResult<EmailAnalyticsDto>> GetEmailAnalytics([FromQuery] int days = 30)
        {
            try
            {
                var desde = DateTime.UtcNow.AddDays(-days);
                var logs = await _db.EmailLogs
                    .Where(e => !e.IsDeleted && e.SentAt >= desde)
                    .Select(e => new { e.EmailType, e.IsSuccess, e.SentAt, e.ToEmail })
                    .ToListAsync();

                var porTipo = logs
                    .GroupBy(e => e.EmailType ?? "Generic")
                    .Select(g => new EmailTipoStatsDto
                    {
                        Tipo      = g.Key,
                        Enviados  = g.Count(),
                        Exitosos  = g.Count(e => e.IsSuccess),
                        Fallidos  = g.Count(e => !e.IsSuccess),
                        TasaExito = g.Count() > 0 ? Math.Round((double)g.Count(e => e.IsSuccess) / g.Count() * 100, 1) : 0
                    }).ToList();

                var volumenDia = logs
                    .GroupBy(e => e.SentAt.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new EmailVolumenDiaDto
                    {
                        Fecha       = g.Key,
                        FechaLabel  = g.Key.ToString("dd/MM"),
                        TotalEnviados = g.Count(),
                        Exitosos    = g.Count(e => e.IsSuccess),
                        Fallidos    = g.Count(e => !e.IsSuccess)
                    }).ToList();

                var heatmap = logs.Select(e => new HeatmapCellDto
                {
                    DiaSemana = (int)e.SentAt.DayOfWeek == 0 ? 6 : (int)e.SentAt.DayOfWeek - 1, // 0=Lun
                    Hora      = e.SentAt.Hour,
                    Valor     = 1
                })
                .GroupBy(h => new { h.DiaSemana, h.Hora })
                .Select(g => new HeatmapCellDto { DiaSemana = g.Key.DiaSemana, Hora = g.Key.Hora, Valor = g.Sum(h => h.Valor) })
                .ToList();

                // Cobertura de notificaciones para cobros vencidos
                var cobrosVencidos = await _db.Cobros
                    .Where(c => !c.IsDeleted && c.Estado == EstadoCobro.Vencido)
                    .Select(c => new { c.CorreoElectronico })
                    .ToListAsync();

                var emailsNotificados = logs.Where(e => e.EmailType == "CobroVencido").Select(e => e.ToEmail.ToLower()).Distinct().ToHashSet();
                int conEmailNotificado    = cobrosVencidos.Count(c => !string.IsNullOrEmpty(c.CorreoElectronico) && emailsNotificados.Contains(c.CorreoElectronico.ToLower()));
                int conEmailNoNotificado  = cobrosVencidos.Count(c => !string.IsNullOrEmpty(c.CorreoElectronico) && !emailsNotificados.Contains(c.CorreoElectronico.ToLower()));
                int sinEmail              = cobrosVencidos.Count(c => string.IsNullOrEmpty(c.CorreoElectronico));

                // 5.4 Correlación Email → Cobro (ROI del canal)
                // Match cobros vencidos that later became Pagado/Cobrado against email sends
                var cobrosVencidosAll = await _db.Cobros
                    .Where(c => !c.IsDeleted && !string.IsNullOrEmpty(c.CorreoElectronico))
                    .Select(c => new { c.CorreoElectronico, c.Estado, c.FechaVencimiento, c.FechaCobro })
                    .ToListAsync();

                var emailsSentToClients = logs
                    .Where(e => e.IsSuccess && e.EmailType == "CobroVencido")
                    .GroupBy(e => e.ToEmail.ToLower())
                    .ToDictionary(g => g.Key, g => g.Min(e => e.SentAt));   // earliest send

                var cobrosConEmail    = cobrosVencidosAll.Where(c => emailsSentToClients.ContainsKey(c.CorreoElectronico!.ToLower())).ToList();
                var cobrosSinEmail    = cobrosVencidosAll.Where(c => !emailsSentToClients.ContainsKey(c.CorreoElectronico!.ToLower())).ToList();

                static bool esPagado(EstadoCobro e) => e == EstadoCobro.Cobrado || e == EstadoCobro.Pagado;

                // Buckets: pagó en <3d, 4-7d, >7d, nunca pagó
                var buckets = new List<CorrelacionBucketDto>();
                var totalConEmail = cobrosConEmail.Count;
                if (totalConEmail > 0)
                {
                    var pago3  = cobrosConEmail.Count(c =>
                    {
                        if (!esPagado(c.Estado)) return false;
                        var sent = emailsSentToClients[c.CorreoElectronico!.ToLower()];
                        return (c.FechaCobro - sent).TotalDays <= 3;
                    });
                    var pago4a7 = cobrosConEmail.Count(c =>
                    {
                        if (!esPagado(c.Estado)) return false;
                        var sent = emailsSentToClients[c.CorreoElectronico!.ToLower()];
                        var d = (c.FechaCobro - sent).TotalDays;
                        return d > 3 && d <= 7;
                    });
                    var pagoMas7 = cobrosConEmail.Count(c =>
                    {
                        if (!esPagado(c.Estado)) return false;
                        var sent = emailsSentToClients[c.CorreoElectronico!.ToLower()];
                        return (c.FechaCobro - sent).TotalDays > 7;
                    });
                    var nuncaPago = cobrosConEmail.Count(c => !esPagado(c.Estado));

                    buckets.Add(new CorrelacionBucketDto { Etiqueta = "Pagó ≤3 días",    Cantidad = pago3,     Porcentaje = Math.Round((double)pago3     / totalConEmail * 100, 1) });
                    buckets.Add(new CorrelacionBucketDto { Etiqueta = "Pagó 4-7 días",   Cantidad = pago4a7,   Porcentaje = Math.Round((double)pago4a7   / totalConEmail * 100, 1) });
                    buckets.Add(new CorrelacionBucketDto { Etiqueta = "Pagó >7 días",    Cantidad = pagoMas7,  Porcentaje = Math.Round((double)pagoMas7  / totalConEmail * 100, 1) });
                    buckets.Add(new CorrelacionBucketDto { Etiqueta = "Nunca pagó",      Cantidad = nuncaPago, Porcentaje = Math.Round((double)nuncaPago / totalConEmail * 100, 1) });
                }

                var correlacion = new EmailCorrelacionCanalDto
                {
                    TotalCobrosVencidosConEmail  = totalConEmail,
                    TotalCobrosVencidosSinEmail  = cobrosSinEmail.Count,
                    TasaPagoConEmail             = totalConEmail > 0 ? Math.Round((double)cobrosConEmail.Count(c => esPagado(c.Estado))     / totalConEmail * 100, 1) : 0,
                    TasaPagoSinEmail             = cobrosSinEmail.Count > 0 ? Math.Round((double)cobrosSinEmail.Count(c => esPagado(c.Estado)) / cobrosSinEmail.Count * 100, 1) : 0,
                    BucketsConEmail              = buckets
                };

                return Ok(new EmailAnalyticsDto
                {
                    PorTipo              = porTipo,
                    VolumenPorDia        = volumenDia,
                    HeatmapEnvios        = heatmap,
                    TasaExitoGlobal      = logs.Count > 0 ? Math.Round((double)logs.Count(e => e.IsSuccess) / logs.Count * 100, 1) : 100,
                    TotalEnviadosPeriodo = logs.Count,
                    TotalFallidosPeriodo = logs.Count(e => !e.IsSuccess),
                    CorrelacionCanal     = correlacion,
                    CoberturaCobros      = new EmailCoberturaCobrosDto
                    {
                        CobrosVencidosTotal           = cobrosVencidos.Count,
                        ConEmailNotificado            = conEmailNotificado,
                        ConEmailNoNotificado          = conEmailNoNotificado,
                        SinEmail                      = sinEmail,
                        PorcentajeCoberturaNotificada = cobrosVencidos.Count > 0
                            ? Math.Round((double)conEmailNotificado / cobrosVencidos.Count * 100, 1)
                            : 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo analytics de emails");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÓDULO 6 — Operacional
        // GET /api/analytics/operacional
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("operacional")]
        public async Task<ActionResult<OperacionalStatsDto>> GetOperacional()
        {
            try
            {
                var now = DateTime.UtcNow;

                static DateTime GetWeekStart(DateTime date)
                {
                    var d = date.Date;
                    var diff = ((int)d.DayOfWeek + 6) % 7;
                    return d.AddDays(-diff);
                }

                static string WeekLabel(DateTime mondayStart)
                {
                    var isoWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                        mondayStart,
                        CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday);
                    return $"{mondayStart:yyyy}-W{isoWeek:D2}";
                }

                var weekStarts = Enumerable.Range(0, 12)
                    .Select(offset => GetWeekStart(now).AddDays(-7 * (11 - offset)))
                    .ToList();
                var weekLabels = weekStarts.Select(WeekLabel).ToList();
                var weekSet = weekLabels.ToHashSet(StringComparer.OrdinalIgnoreCase);

                var reclamosActivos = await _db.Reclamos
                    .Where(r => !r.IsDeleted && r.Estado != EstadoReclamo.Resuelto && r.Estado != EstadoReclamo.Cerrado && r.UsuarioAsignadoId.HasValue)
                    .Select(r => new
                    {
                        r.Id,
                        AgenteId = r.UsuarioAsignadoId!.Value,
                        r.Prioridad,
                        r.FechaLimiteRespuesta,
                        r.FechaResolucion
                    })
                    .ToListAsync();

                var reclamoToAgente = reclamosActivos
                    .GroupBy(r => r.Id)
                    .Select(g => g.First())
                    .ToDictionary(r => r.Id, r => r.AgenteId);

                var usuarios = await _db.Users
                    .Where(u => !u.IsDeleted)
                    .Select(u => new
                    {
                        u.Id,
                        Nombre = (u.FirstName + " " + u.LastName).Trim(),
                        u.Email,
                        u.LastLoginAt,
                        u.IsActive,
                        Rol = u.UserRoles.Any() ? u.UserRoles.First().Role.Name : "Sin Rol"
                    })
                    .ToListAsync();

                var historialDesde = weekStarts.First();
                var historialReclamos = await _db.ReclamoHistoriales
                    .Where(h => !h.IsDeleted && h.CreatedAt >= historialDesde)
                    .Select(h => new { h.ReclamoId, h.Usuario, h.CreatedAt })
                    .ToListAsync();

                var accionesPorAgenteSemana = new Dictionary<(int AgenteId, string Semana), int>();
                var totalAccionesPorAgente = new Dictionary<int, int>();

                foreach (var h in historialReclamos)
                {
                    if (!reclamoToAgente.TryGetValue(h.ReclamoId, out var agenteId))
                    {
                        continue;
                    }

                    var semana = WeekLabel(GetWeekStart(h.CreatedAt));
                    if (!weekSet.Contains(semana))
                    {
                        continue;
                    }

                    var key = (agenteId, semana);
                    accionesPorAgenteSemana[key] = accionesPorAgenteSemana.TryGetValue(key, out var count) ? count + 1 : 1;
                    totalAccionesPorAgente[agenteId] = totalAccionesPorAgente.TryGetValue(agenteId, out var total) ? total + 1 : 1;
                }

                var usuariosPorId = usuarios.ToDictionary(u => u.Id, u => u);

                var cargaReclamos = reclamosActivos
                    .GroupBy(r => r.AgenteId)
                    .Select(g =>
                    {
                        var agente = usuariosPorId.GetValueOrDefault(g.Key);
                        var nombre = !string.IsNullOrWhiteSpace(agente?.Nombre) ? agente!.Nombre : $"Agente {g.Key}";
                        var evaluablesSla = g.Where(r => r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue).ToList();
                        var dentroSla = evaluablesSla.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value);
                        var criticos = g.Count(r => r.Prioridad == PrioridadReclamo.Critica);
                        var alta = g.Count(r => r.Prioridad == PrioridadReclamo.Alta);

                        return new AgenteCargaDto
                        {
                            NombreAgente = nombre,
                            ReclamosActivos = g.Count(),
                            ReclamosAltaPrioridad = alta,
                            ReclamosCriticos = criticos,
                            PorcentajeSla = evaluablesSla.Count > 0 ? Math.Round((double)dentroSla / evaluablesSla.Count * 100, 1) : 100,
                            TieneCriticosExclusivos = criticos > 0 && alta == 0,
                            TotalAccionesUltimas12Semanas = totalAccionesPorAgente.TryGetValue(g.Key, out var acciones) ? acciones : 0
                        };
                    })
                    .OrderByDescending(a => a.ReclamosCriticos)
                    .ThenByDescending(a => a.ReclamosActivos)
                    .ToList();

                var heatmap = cargaReclamos
                    .Select(a =>
                    {
                        var agenteId = usuarios.FirstOrDefault(u => u.Nombre == a.NombreAgente)?.Id;
                        var semanas = weekLabels.Select(w => new HeatmapCellSemanaDto
                        {
                            Semana = w,
                            Acciones = agenteId.HasValue && accionesPorAgenteSemana.TryGetValue((agenteId.Value, w), out var acciones) ? acciones : 0
                        }).ToList();

                        return new HeatmapAgenteRowDto
                        {
                            NombreAgente = a.NombreAgente,
                            Semanas = semanas
                        };
                    })
                    .ToList();

                var ayerInicio = now.Date.AddDays(-1);
                var ayerFin = now.Date;
                var actividadAyerRaw = await _db.ReclamoHistoriales
                    .Where(h => !h.IsDeleted && h.CreatedAt >= ayerInicio && h.CreatedAt < ayerFin)
                    .Select(h => h.Usuario)
                    .ToListAsync();

                var actividadAyer = actividadAyerRaw
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .GroupBy(x => x.Trim().ToLower())
                    .ToDictionary(g => g.Key, g => g.Count());

                var actividadUsuarios = usuarios
                    .Select(u =>
                    {
                        var nombre = !string.IsNullOrWhiteSpace(u.Nombre) ? u.Nombre : "Usuario";
                        var emailKey = (u.Email ?? string.Empty).Trim().ToLower();
                        var nombreKey = nombre.Trim().ToLower();
                        var accionesAyer = (actividadAyer.TryGetValue(emailKey, out var byEmail) ? byEmail : 0)
                                         + (actividadAyer.TryGetValue(nombreKey, out var byName) ? byName : 0);

                        return new UsuarioActividadDto
                        {
                            NombreUsuario = nombre,
                            Email = u.Email ?? string.Empty,
                            Rol = u.Rol,
                            UltimoLogin = u.LastLoginAt,
                            DiasSinAcceso = u.LastLoginAt.HasValue ? Math.Max(0, (int)(now - u.LastLoginAt.Value).TotalDays) : 999,
                            AccionesAyer = accionesAyer,
                            EsActivo = u.IsActive
                        };
                    })
                    .OrderByDescending(u => u.DiasSinAcceso)
                    .ToList();

                var sesionesDesde = now.AddDays(-56);
                var chatSesiones = await _db.ChatSessions
                    .Where(s => !s.IsDeleted)
                    .Select(s => new { s.MessageCount, s.Status, s.CreatedAt })
                    .ToListAsync();

                var chatMensajes = await _db.ChatMessages
                    .Where(m => !m.IsDeleted)
                    .Select(m => new { m.ReactionScore, m.ProcessingTimeMs, m.Content })
                    .ToListAsync();

                var sesionesPorSemana = chatSesiones
                    .Where(s => s.CreatedAt >= sesionesDesde)
                    .GroupBy(s => WeekLabel(GetWeekStart(s.CreatedAt)))
                    .Select(g => new ChatSesionSemanalDto
                    {
                        Semana = g.Key,
                        Activas = g.Count(s => s.Status == ChatSessionStatus.Active),
                        Cerradas = g.Count(s => s.Status == ChatSessionStatus.Closed || s.Status == ChatSessionStatus.Archived)
                    })
                    .OrderBy(x => x.Semana)
                    .ToList();

                var stopwords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "de", "la", "el", "y", "a", "en", "que", "por", "para", "con", "los", "las", "del", "una", "uno", "un"
                };

                var separators = new[]
                {
                    ' ', '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']', '{', '}', '-', '_', '/', '\\', '\r', '\n', '\t'
                };

                var topPalabras = chatMensajes
                    .SelectMany(m => (m.Content ?? string.Empty).ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries))
                    .Where(w => w.Length >= 4 && !stopwords.Contains(w) && w.All(char.IsLetter))
                    .GroupBy(w => w)
                    .Select(g => new TopPalabraDto { Palabra = g.Key, Frecuencia = g.Count() })
                    .OrderByDescending(x => x.Frecuencia)
                    .ThenBy(x => x.Palabra)
                    .Take(10)
                    .ToList();

                var mensajesConFeedback = chatMensajes.Where(m => m.ReactionScore.HasValue).ToList();

                var chatStats = new ChatIAStatsDto
                {
                    TotalSesiones = chatSesiones.Count,
                    SesionesActivas = chatSesiones.Count(s => s.Status == ChatSessionStatus.Active),
                    SesionesCerradas = chatSesiones.Count(s => s.Status == ChatSessionStatus.Closed || s.Status == ChatSessionStatus.Archived),
                    MensajesPorSesionPromedio = chatSesiones.Count > 0 ? Math.Round(chatSesiones.Average(s => (double)s.MessageCount), 1) : 0,
                    TiempoProcesPromedioMs = chatMensajes.Any(m => m.ProcessingTimeMs.HasValue)
                        ? Math.Round(chatMensajes.Where(m => m.ProcessingTimeMs.HasValue).Average(m => (double)m.ProcessingTimeMs!.Value), 1)
                        : 0,
                    MensajesPositivos = mensajesConFeedback.Count(m => m.ReactionScore == 1),
                    MensajesNegativos = mensajesConFeedback.Count(m => m.ReactionScore == -1),
                    TasaSatisfaccion = mensajesConFeedback.Count > 0
                        ? Math.Round((double)mensajesConFeedback.Count(m => m.ReactionScore == 1) / mensajesConFeedback.Count * 100, 1)
                        : 100,
                    SesionesPorSemana = sesionesPorSemana,
                    TopPalabras = topPalabras
                };

                var totalActivos = cargaReclamos.Sum(x => x.ReclamosActivos);
                var agenteTop = cargaReclamos
                    .OrderByDescending(x => x.ReclamosActivos)
                    .ThenByDescending(x => x.ReclamosCriticos)
                    .FirstOrDefault();

                var riesgoCritico = totalActivos > 0 && agenteTop is not null
                    ? (double)agenteTop.ReclamosActivos / totalActivos >= 0.40
                    : false;

                var alertas = new List<string>();
                if (riesgoCritico && agenteTop is not null)
                {
                    alertas.Add($"Concentracion de reclamos: {agenteTop.NombreAgente} maneja {agenteTop.ReclamosActivos} activos.");
                }

                var inactivosCriticos = actividadUsuarios
                    .Where(u => u.EsActivo && u.DiasSinAcceso >= 15)
                    .Select(u => u.NombreUsuario)
                    .Take(5)
                    .ToList();
                if (inactivosCriticos.Count > 0)
                {
                    alertas.Add($"Usuarios activos sin acceso >=15 dias: {string.Join(", ", inactivosCriticos)}.");
                }

                if (chatStats.TasaSatisfaccion < 70)
                {
                    alertas.Add($"Satisfaccion de chat IA baja ({chatStats.TasaSatisfaccion}%).");
                }

                return Ok(new OperacionalStatsDto
                {
                    ActividadHeatmapPorAgente = heatmap,
                    CargaReclamos = cargaReclamos,
                    RiesgoConcentracionCritica = riesgoCritico,
                    AgenteConcentracionCritica = riesgoCritico ? agenteTop?.NombreAgente : null,
                    UltimoAcceso = actividadUsuarios,
                    ChatIA = chatStats,
                    SistemaRendimiento = _endpointPerformanceStore.BuildReport(),
                    Alertas = alertas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo stats operacionales");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÓDULO 8 — Reportes Exportables
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("reportes/cartera-aseguradora/excel")]
        public async Task<IActionResult> GetCarteraAseguradoraExcel([FromQuery] string? aseguradora = null)
        {
            var report = await _reportsService.GenerateCarteraAseguradoraExcelAsync(aseguradora);
            return File(report.Content, report.ContentType, report.FileName);
        }

        [HttpGet("reportes/cartera-aseguradora/pdf")]
        public async Task<IActionResult> GetCarteraAseguradoraPdf([FromQuery] string? aseguradora = null)
        {
            var report = await _reportsService.GenerateCarteraAseguradoraPdfAsync(aseguradora);
            return File(report.Content, report.ContentType, report.FileName);
        }

        [HttpGet("reportes/morosidad/excel")]
        public async Task<IActionResult> GetMorosidadExcel()
        {
            var report = await _reportsService.GenerateMorosidadExcelAsync();
            return File(report.Content, report.ContentType, report.FileName);
        }

        [HttpPost("reportes/morosidad/email")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ReportDispatchResultDto>> SendMorosidadByEmail([FromBody] ReportEmailRequestDto? request)
        {
            var total = await _reportsService.SendMorosidadReportByEmailAsync(request?.Recipients);
            return Ok(new ReportDispatchResultDto
            {
                EmailsEnviados = total,
                Message = total > 0
                    ? $"Reporte de morosidad enviado a {total} destinatario(s)."
                    : "No se enviaron emails; verifica destinatarios configurados."
            });
        }

        [HttpGet("reportes/reclamos-sla/pdf")]
        public async Task<IActionResult> GetReclamosSlaPdf()
        {
            var report = await _reportsService.GenerateReclamosSlaPdfAsync();
            return File(report.Content, report.ContentType, report.FileName);
        }

        [HttpGet("reportes/estado-portafolio/pdf")]
        public async Task<IActionResult> GetEstadoPortafolioPdf()
        {
            var report = await _reportsService.GenerateEstadoPortafolioPdfAsync();
            return File(report.Content, report.ContentType, report.FileName);
        }

        [HttpPost("reportes/estado-portafolio/email-admins")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ReportDispatchResultDto>> SendEstadoPortafolioAdmin()
        {
            var total = await _reportsService.SendEstadoPortafolioReportToAdminsAsync();
            return Ok(new ReportDispatchResultDto
            {
                EmailsEnviados = total,
                Message = total > 0
                    ? $"Estado del portafolio enviado a {total} admin(s)."
                    : "No se encontraron admins con correo configurado."
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÓDULO 7 — Analítica Predictiva (Machine Learning Ligero)
        // GET /api/analytics/predictivo
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("predictivo")]
        public async Task<ActionResult<PredictivoAnalyticsDto>> GetPredictivo()
        {
            try
            {
                var now      = DateTime.UtcNow;
                var mesBase  = new DateTime(now.Year, now.Month, 1);

                // ── Carga base (todas las queries en paralelo) ────────────────
                var pPolizas = await _db.Polizas
                    .Where(p => p.EsActivo && !p.IsDeleted && p.NumeroPoliza != null)
                    .Select(p => new
                    {
                        p.Id, p.NumeroPoliza, p.NombreAsegurado, p.NumeroCedula,
                        p.Aseguradora, p.Frecuencia, p.Prima,
                        p.FechaVigencia, p.CreatedAt, p.Correo, p.NumeroTelefono
                    })
                    .ToListAsync();

                var pCobros = await _db.Cobros
                    .Where(c => !c.IsDeleted)
                    .Select(c => new
                    {
                        c.Id, c.NumeroRecibo, c.NumeroPoliza,
                        c.ClienteNombreCompleto, c.Estado,
                        c.MontoTotal, c.FechaVencimiento
                    })
                    .ToListAsync();

                var pCotizaciones = await _db.Cotizaciones
                    .Where(c => !c.IsDeleted && !string.IsNullOrEmpty(c.NumeroPoliza))
                    .Select(c => new { c.NumeroPoliza, c.TipoSeguro, c.FechaCreacion })
                    .ToListAsync();

                var pReclamos = await _db.Reclamos
                    .Where(r => !r.IsDeleted)
                    .Select(r => new { r.FechaReclamo })
                    .ToListAsync();

                var pEmailsRecientes = await _db.EmailLogs
                    .Where(e => e.IsSuccess && e.SentAt >= now.AddDays(-90))
                    .Select(e => e.ToEmail)
                    .ToListAsync();

                // ── Lookups compartidos ───────────────────────────────────────
                var tipoSeguroPorPoliza = pCotizaciones
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => NormalizeTipoSeguro(g.OrderByDescending(x => x.FechaCreacion).First().TipoSeguro),
                        StringComparer.OrdinalIgnoreCase);

                var totalCobrosPorPoliza = pCobros
                    .Where(c => !string.IsNullOrEmpty(c.NumeroPoliza))
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

                var vencidosPorPoliza = pCobros
                    .Where(c => !string.IsNullOrEmpty(c.NumeroPoliza) && c.Estado == EstadoCobro.Vencido)
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

                var polizasConVencido = vencidosPorPoliza.Keys
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var polizasPorCedula = pPolizas
                    .Where(p => !string.IsNullOrEmpty(p.NumeroCedula))
                    .GroupBy(p => p.NumeroCedula!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

                var emailsRecientesSet = pEmailsRecientes
                    .Select(e => e.ToLowerInvariant())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // ══ 7.1 — Score de Riesgo de Morosidad ═══════════════════════
                var polizasConScore = pPolizas.Select(p =>
                {
                    var numPoliza   = p.NumeroPoliza!;
                    var total       = totalCobrosPorPoliza.TryGetValue(numPoliza, out var t) ? t : 0;
                    var vencidos    = vencidosPorPoliza.TryGetValue(numPoliza, out var v)    ? v : 0;
                    var tasaV       = total > 0 ? (double)vencidos / total : 0.0;
                    var tipo        = tipoSeguroPorPoliza.TryGetValue(numPoliza, out var tp) ? tp : "OTROS";
                    var numCliente  = !string.IsNullOrEmpty(p.NumeroCedula) &&
                                     polizasPorCedula.TryGetValue(p.NumeroCedula!, out var nc) ? nc : 1;

                    int score = 0;
                    // Historial morosidad (0–40 pts)
                    score += tasaV >= 0.50 ? 40 : tasaV >= 0.25 ? 25 : tasaV > 0 ? 15 : 0;
                    // Frecuencia de cobro
                    var freq = (p.Frecuencia ?? string.Empty).ToUpperInvariant();
                    if      (freq.Contains("MENSUAL"))     score += 20;
                    else if (freq.Contains("TRIMESTRAL")) score += 10;
                    // Póliza nueva sin historial
                    if ((now - p.CreatedAt).TotalDays < 90 && total == 0) score += 5;
                    // Tipo de seguro
                    if      (tipo.Contains("VIDA"))                                    score -= 10;
                    else if (tipo.Contains("AUTO") || tipo.Contains("EMPRESARIAL"))    score += 5;
                    // Lealtad multi-póliza
                    if (numCliente >= 2) score -= 5;
                    score = Math.Clamp(score, 0, 100);

                    return new PolizaRiesgoDto
                    {
                        NumeroPoliza    = numPoliza,
                        NombreAsegurado = p.NombreAsegurado ?? string.Empty,
                        Aseguradora     = p.Aseguradora     ?? string.Empty,
                        TipoSeguro      = tipo,
                        Frecuencia      = p.Frecuencia      ?? string.Empty,
                        Score           = score,
                        Categoria       = score >= 66 ? "Rojo" : score >= 36 ? "Amarillo" : "Verde",
                        TasaVencido     = Math.Round(tasaV * 100, 1),
                        TotalCobros     = total,
                        CobrosVencidos  = vencidos
                    };
                })
                .OrderByDescending(p => p.Score)
                .Take(100)
                .ToList();

                var scoreMorosidad = new PredictivoScoreMorosidadDto
                {
                    Polizas = polizasConScore,
                    DistribucionRiesgo = new ChartDataPointDto[]
                    {
                        new() { Name = "Verde (bajo)",     Value = polizasConScore.Count(p => p.Categoria == "Verde")    },
                        new() { Name = "Amarillo (medio)", Value = polizasConScore.Count(p => p.Categoria == "Amarillo") },
                        new() { Name = "Rojo (alto)",      Value = polizasConScore.Count(p => p.Categoria == "Rojo")     }
                    }
                };

                // ══ 7.2 — Predicción de Reclamos por Temporada ═══════════════
                var reclamosByYM = pReclamos
                    .GroupBy(r => (r.FechaReclamo.Year, r.FechaReclamo.Month))
                    .ToDictionary(g => g.Key, g => g.Count());

                var prediccionMeses = Enumerable.Range(1, 3).Select(offset =>
                {
                    var mes      = mesBase.AddMonths(offset);
                    var muestras = Enumerable.Range(1, 3)
                        .Select(y => reclamosByYM.TryGetValue((mes.Year - y, mes.Month), out var c) ? (int?)c : null)
                        .Where(c => c.HasValue).Select(c => c!.Value).ToList();

                    var esperado     = muestras.Count > 0 ? (int)Math.Round(muestras.Average()) : 0;
                    var anioAnterior = reclamosByYM.TryGetValue((mes.Year - 1, mes.Month), out var prev) ? prev : 0;

                    return new PrediccionMesDto
                    {
                        MesLabel     = mes.ToString("MMM yyyy", CultureInfo.CurrentCulture),
                        Esperado     = esperado,
                        AnioAnterior = anioAnterior,
                        CambioPorc   = anioAnterior > 0
                            ? Math.Round((double)(esperado - anioAnterior) / anioAnterior * 100, 1)
                            : 0d
                    };
                }).ToList();

                var historicoSerie12m = Enumerable.Range(0, 12).Select(m =>
                {
                    var mes = mesBase.AddMonths(-11 + m);
                    return new ChartDataPointDto
                    {
                        Name  = mes.ToString("MMM yy", CultureInfo.CurrentCulture),
                        Value = reclamosByYM.TryGetValue((mes.Year, mes.Month), out var cnt) ? cnt : 0
                    };
                }).ToList();

                var reclamosTemporada = new PredictivoReclamosDto
                {
                    ProximosMeses  = prediccionMeses,
                    HistoricoSerie = new[] { new MultiSeriesChartDto { Name = "Reclamos", Series = historicoSerie12m } }
                };

                // ══ 7.3 — Detección de Anomalías en Cobros (Z-score) ═════════
                var cobrosActivos = pCobros
                    .Where(c => c.Estado == EstadoCobro.Pendiente || c.Estado == EstadoCobro.Vencido)
                    .ToList();

                var gVals  = cobrosActivos.Select(c => (double)c.MontoTotal).ToList();
                var gMean  = gVals.Count > 0 ? gVals.Average() : 0.0;
                var gStd   = gVals.Count > 1
                    ? Math.Sqrt(gVals.Select(v => Math.Pow(v - gMean, 2)).Average())
                    : 1.0;
                if (gStd < 1.0) gStd = 1.0;

                var statsPerPoliza7 = cobrosActivos
                    .Where(c => !string.IsNullOrEmpty(c.NumeroPoliza))
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var vals = g.Select(c => (double)c.MontoTotal).ToList();
                            var m    = vals.Average();
                            var s    = vals.Count > 1
                                ? Math.Sqrt(vals.Select(v => Math.Pow(v - m, 2)).Average())
                                : 0.0;
                            return (mean: m, std: s);
                        },
                        StringComparer.OrdinalIgnoreCase);

                var anomaliasList = cobrosActivos
                    .Select(c =>
                    {
                        double mean, std;
                        if (!string.IsNullOrEmpty(c.NumeroPoliza) &&
                            statsPerPoliza7.TryGetValue(c.NumeroPoliza, out var ps))
                        {
                            mean = ps.mean;
                            std  = ps.std > 100 ? ps.std : gStd;
                        }
                        else { mean = gMean; std = gStd; }
                        var z = Math.Abs((double)c.MontoTotal - mean) / std;
                        return (cobro: c, z, mean);
                    })
                    .Where(x => x.z >= 2.5)
                    .OrderByDescending(x => x.z)
                    .Take(50)
                    .Select(x => new AnomaliaCobroDto
                    {
                        Id            = x.cobro.Id,
                        NumeroRecibo  = x.cobro.NumeroRecibo,
                        ClienteNombre = x.cobro.ClienteNombreCompleto,
                        NumeroPoliza  = x.cobro.NumeroPoliza,
                        MontoTotal    = x.cobro.MontoTotal,
                        ZScore        = Math.Round(x.z, 2),
                        MediaCliente  = Math.Round(x.mean, 0),
                        TipoAnomalia  = (double)x.cobro.MontoTotal > x.mean ? "Alto" : "Bajo",
                        EstadoActual  = x.cobro.Estado.ToString()
                    })
                    .ToList();

                var anomaliasResult = new PredictivoAnomaliasCobrosDto
                {
                    Anomalias        = anomaliasList,
                    MediaGlobal      = Math.Round(gMean, 0),
                    DesviacionGlobal = Math.Round(gStd, 0),
                    TotalAnalizados  = cobrosActivos.Count
                };

                // ══ 7.4 — Renovación Proactiva (Lead Scoring) ════════════════
                var hoy               = now.Date;
                var polizasPorRenovar = pPolizas
                    .Where(p => p.FechaVigencia.Date >= hoy && p.FechaVigencia.Date <= hoy.AddDays(60))
                    .ToList();

                var leads = polizasPorRenovar.Select(p =>
                {
                    var numPoliza  = p.NumeroPoliza!;
                    var dias       = (p.FechaVigencia.Date - hoy).Days;
                    var neverVenc  = !polizasConVencido.Contains(numPoliza);
                    var numCliente = !string.IsNullOrEmpty(p.NumeroCedula) &&
                                    polizasPorCedula.TryGetValue(p.NumeroCedula!, out var nc) ? nc : 1;
                    var tipo       = tipoSeguroPorPoliza.TryGetValue(numPoliza, out var tp) ? tp : "OTROS";
                    var tieneEmail = !string.IsNullOrEmpty(p.Correo) && emailsRecientesSet.Contains(p.Correo);

                    int score    = 30;
                    var factores = new List<string> { "Vence en <=60 dias (+30)" };
                    if (neverVenc)   { score += 20; factores.Add("Sin historial vencido (+20)"); }
                    if (numCliente >= 2) { score += 15; factores.Add("Multi-poliza (+15)");      }
                    if (tieneEmail) { score += 10; factores.Add("Email activo (+10)");            }
                    if (tipo.Contains("VIDA") || tipo.Contains("EMPRESARIAL"))
                        { score += 10; factores.Add($"{tipo} (+10)"); }

                    return new PolizaLeadDto
                    {
                        NumeroPoliza    = numPoliza,
                        NombreAsegurado = p.NombreAsegurado ?? string.Empty,
                        Correo          = p.Correo          ?? string.Empty,
                        Telefono        = p.NumeroTelefono  ?? string.Empty,
                        Aseguradora     = p.Aseguradora     ?? string.Empty,
                        TipoSeguro      = tipo,
                        PrimaMensual    = Math.Round(NormalizePrimaMensual(p.Frecuencia, p.Prima), 0),
                        FechaVigencia   = p.FechaVigencia,
                        DiasParaVencer  = dias,
                        Score           = score,
                        Factores        = factores.ToArray()
                    };
                })
                .OrderByDescending(l => l.Score)
                .Take(20)
                .ToList();

                var renovacion = new PredictivoRenovacionDto
                {
                    TopPolizas                = leads,
                    TotalPolizasPorRenovar60d = polizasPorRenovar.Count,
                    ScorePromedio             = leads.Count > 0 ? Math.Round(leads.Average(l => l.Score), 1) : 0d,
                    DistribucionScore         = new ChartDataPointDto[]
                    {
                        new() { Name = "Alta (>=75)",   Value = leads.Count(l => l.Score >= 75) },
                        new() { Name = "Media (50-74)", Value = leads.Count(l => l.Score >= 50 && l.Score < 75) },
                        new() { Name = "Baja (<50)",    Value = leads.Count(l => l.Score < 50)  }
                    }
                };

                // ══ 7.5 — Forecast de Prima Mensual Proyectada ═══════════════
                var cobros6mH = pCobros
                    .Where(c => c.FechaVencimiento >= now.AddMonths(-6) &&
                                c.FechaVencimiento <  now &&
                                c.Estado != EstadoCobro.Cancelado)
                    .ToList();

                var esp6m  = cobros6mH.Sum(c => (double)c.MontoTotal);
                var cob6m  = cobros6mH
                    .Where(c => c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado)
                    .Sum(c => (double)c.MontoTotal);
                var tasa6m = esp6m > 0 ? cob6m / esp6m : 0.92;

                // Varianza mensual para IC 90% (±1.645σ)
                var varMeses = Enumerable.Range(1, 6).Select(n =>
                {
                    var ini = now.AddMonths(-n);
                    var fin = now.AddMonths(-n + 1);
                    return cobros6mH.Where(c => c.FechaVencimiento >= ini && c.FechaVencimiento < fin)
                                    .Sum(c => (double)c.MontoTotal);
                }).Where(v => v > 0).ToList();

                var sigma = varMeses.Count > 1
                    ? Math.Sqrt(varMeses.Select(v => Math.Pow(v - varMeses.Average(), 2)).Average())
                    : 0.0;

                var cobrosProx3m = pCobros
                    .Where(c => c.FechaVencimiento >= now &&
                                c.FechaVencimiento <= now.AddMonths(3) &&
                                c.Estado != EstadoCobro.Cancelado)
                    .ToList();

                // Últimos 3 meses (histórico real)
                var histForecast = Enumerable.Range(1, 3).Select(n =>
                {
                    var mes = mesBase.AddMonths(-n);
                    var fin = mes.AddMonths(1);
                    var e   = cobros6mH.Where(c => c.FechaVencimiento >= mes && c.FechaVencimiento < fin).Sum(c => (double)c.MontoTotal);
                    var cb  = cobros6mH.Where(c => c.FechaVencimiento >= mes && c.FechaVencimiento < fin &&
                                                   (c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado))
                                       .Sum(c => (double)c.MontoTotal);
                    return new ForecastMesDto
                    {
                        MesLabel        = mes.ToString("MMM yyyy", CultureInfo.CurrentCulture),
                        MontoEsperado   = (decimal)e,
                        MontoProyectado = (decimal)cb,
                        EsFuturo        = false
                    };
                }).Reverse().ToList();

                // Próximos 3 meses (proyección)
                var futForecast = Enumerable.Range(1, 3).Select(m =>
                {
                    var mes = mesBase.AddMonths(m);
                    var fin = mes.AddMonths(1);
                    var e   = cobrosProx3m.Where(c => c.FechaVencimiento >= mes && c.FechaVencimiento < fin).Sum(c => (double)c.MontoTotal);
                    return new ForecastMesDto
                    {
                        MesLabel        = mes.ToString("MMM yyyy", CultureInfo.CurrentCulture),
                        MontoEsperado   = (decimal)e,
                        MontoProyectado = (decimal)Math.Round(e * tasa6m, 0),
                        EsFuturo        = true
                    };
                }).ToList();

                var primerFut  = futForecast.FirstOrDefault();
                var primerEsp  = (double)(primerFut?.MontoEsperado   ?? 0m);
                var primerProy = (double)(primerFut?.MontoProyectado  ?? 0m);
                var ic         = sigma * 1.645;

                var forecastPrima = new PredictivoForecastPrimaDto
                {
                    CobrosProgramadosProximoMes = (decimal)Math.Round(primerEsp, 0),
                    TasaHistoricaCobro6m        = Math.Round(tasa6m * 100, 1),
                    PrimaProyectada             = (decimal)Math.Round(primerProy, 0),
                    IcInferior                  = (decimal)Math.Round(Math.Max(0, primerProy - ic), 0),
                    IcSuperior                  = (decimal)Math.Round(primerProy + ic, 0),
                    ProyeccionMensual           = histForecast.Concat(futForecast).ToList()
                };

                return Ok(new PredictivoAnalyticsDto
                {
                    ScoreMorosidad    = scoreMorosidad,
                    ReclamosTemporada = reclamosTemporada,
                    AnomaliasCobros   = anomaliasResult,
                    Renovacion        = renovacion,
                    ForecastPrima     = forecastPrima
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo analytics predictivos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // MÓDULO 14 — Agenda Inteligente del Agente
        // GET /api/analytics/agenda
        // ══════════════════════════════════════════════════════════════════════
        [HttpGet("agenda")]
        public async Task<ActionResult<AgendaDto>> GetAgenda()
        {
            try
            {
                var now      = DateTime.UtcNow;
                var hoy      = now.Date;
                var en2dias  = hoy.AddDays(2);
                var en7dias  = hoy.AddDays(7);

                // ── Carga base ─────────────────────────────────────────────
                var pPolizas = await _db.Polizas
                    .Where(p => p.EsActivo && !p.IsDeleted && p.NumeroPoliza != null)
                    .Select(p => new
                    {
                        p.NumeroPoliza, p.NombreAsegurado, p.NumeroCedula,
                        p.Prima, p.Frecuencia, p.FechaVigencia,
                        p.Aseguradora, p.Correo
                    })
                    .ToListAsync();

                var pCobros = await _db.Cobros
                    .Where(c => !c.IsDeleted)
                    .Select(c => new
                    {
                        c.NumeroRecibo, c.NumeroPoliza, c.Estado,
                        c.MontoTotal, c.FechaVencimiento, c.ClienteNombreCompleto
                    })
                    .ToListAsync();

                var pCotizaciones = await _db.Cotizaciones
                    .Where(c => !c.IsDeleted)
                    .Select(c => new
                    {
                        c.NumeroCotizacion, c.NombreAsegurado, c.Estado,
                        c.FechaCreacion, c.Prima, c.TipoSeguro, c.NumeroPoliza
                    })
                    .ToListAsync();

                var pReclamos = await _db.Reclamos
                    .Where(r => !r.IsDeleted &&
                                r.Estado != EstadoReclamo.Resuelto &&
                                r.Estado != EstadoReclamo.Cerrado)
                    .Select(r => new
                    {
                        r.NumeroReclamo, r.NumeroPoliza, r.Estado,
                        r.Prioridad, r.FechaLimiteRespuesta, r.NombreAsegurado
                    })
                    .ToListAsync();

                var pEmails = await _db.EmailLogs
                    .Where(e => e.IsSuccess && e.SentAt >= now.AddDays(-60))
                    .Select(e => e.ToEmail.ToLower())
                    .Distinct()
                    .ToListAsync();

                var emailsSet           = pEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var tipoSeguroPorPoliza = pCotizaciones
                    .Where(c => !string.IsNullOrEmpty(c.NumeroPoliza))
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => NormalizeTipoSeguro(g.OrderByDescending(x => x.FechaCreacion).First().TipoSeguro),
                        StringComparer.OrdinalIgnoreCase);

                var polizasPorCedula = pPolizas
                    .Where(p => !string.IsNullOrEmpty(p.NumeroCedula))
                    .GroupBy(p => p.NumeroCedula!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

                var vencidosPorPoliza = pCobros
                    .Where(c => c.Estado == EstadoCobro.Vencido && !string.IsNullOrEmpty(c.NumeroPoliza))
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

                var secciones = new List<AgendaSeccionDto>();

                // ══ SECCIÓN 1: URGENTE — cobros vencidos de clientes con mayor cartera ════
                var cobrosPorCedula = pCobros
                    .Where(c => c.Estado == EstadoCobro.Vencido && !string.IsNullOrEmpty(c.NumeroPoliza))
                    .Join(
                        pPolizas.Where(p => !string.IsNullOrEmpty(p.NumeroCedula)),
                        c => c.NumeroPoliza,
                        p => p.NumeroPoliza,
                        (c, p) => new { Cobro = c, Poliza = p },
                        StringComparer.OrdinalIgnoreCase)
                    .GroupBy(x => x.Poliza.NumeroCedula!, StringComparer.OrdinalIgnoreCase)
                    .Select(g =>
                    {
                        var primaAcum = g
                            .GroupBy(x => x.Poliza.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                            .Sum(pg => NormalizePrimaMensual(pg.First().Poliza.Frecuencia, pg.First().Poliza.Prima));
                        var montoVencido = g.Sum(x => x.Cobro.MontoTotal);
                        return new
                        {
                            Cedula         = g.Key,
                            Nombre         = g.First().Poliza.NombreAsegurado ?? string.Empty,
                            PrimaAcumulada = primaAcum,
                            MontoVencido   = montoVencido,
                            NumCobros      = g.Count(),
                            DiasMax        = g.Max(x => (int)(hoy - x.Cobro.FechaVencimiento.Date).TotalDays)
                        };
                    })
                    .OrderByDescending(x => x.PrimaAcumulada)
                    .Take(5)
                    .ToList();

                if (cobrosPorCedula.Any())
                {
                    var items = cobrosPorCedula.Select(x => new AgendaItemDto
                    {
                        Tipo        = "cobro",
                        Nivel       = "critico",
                        Titulo      = $"Cobro vencido — {x.Nombre}",
                        Descripcion = $"{x.NumCobros} cobro(s) · ₡{x.MontoVencido:N0} · {x.DiasMax} días vencido",
                        Cedula      = x.Cedula,
                        Monto       = x.MontoVencido,
                        DiasVencido = x.DiasMax,
                        NavLink     = $"/cobros?cedula={x.Cedula}"
                    }).ToList();

                    secciones.Add(new AgendaSeccionDto
                    {
                        Titulo = "URGENTE — Cobros Vencidos",
                        Nivel  = "critico",
                        Icono  = "crisis_alert",
                        Total  = items.Count,
                        Items  = items
                    });
                }

                // ══ SECCIÓN 2: VENCIMIENTOS HOY / PRÓXIMAS 48 HORAS ══════════
                var porVencer = pPolizas
                    .Where(p => p.FechaVigencia.Date >= hoy && p.FechaVigencia.Date <= en2dias)
                    .Select(p => new AgendaItemDto
                    {
                        Tipo        = "poliza",
                        Nivel       = p.FechaVigencia.Date == hoy ? "critico" : "alerta",
                        Titulo      = p.FechaVigencia.Date == hoy
                            ? $"Póliza vence HOY — {p.NombreAsegurado}"
                            : $"Póliza vence mañana — {p.NombreAsegurado}",
                        Descripcion = $"{p.NumeroPoliza} · {(tipoSeguroPorPoliza.TryGetValue(p.NumeroPoliza!, out var t) ? t : "N/D")} · {p.Aseguradora}",
                        NumeroRef   = p.NumeroPoliza,
                        Monto       = NormalizePrimaMensual(p.Frecuencia, p.Prima),
                        DiasVencido = (int)(p.FechaVigencia.Date - hoy).TotalDays,
                        NavLink     = $"/polizas?numero={p.NumeroPoliza}"
                    })
                    .OrderBy(x => x.DiasVencido)
                    .ToList();

                if (porVencer.Any())
                {
                    secciones.Add(new AgendaSeccionDto
                    {
                        Titulo = "VENCIMIENTOS HOY / MAÑANA",
                        Nivel  = "alerta",
                        Icono  = "event_busy",
                        Total  = porVencer.Count,
                        Items  = porVencer
                    });
                }

                // ══ SECCIÓN 3: RECLAMOS CRÍTICOS SIN RESOLVER ════════════════
                var reclamosCriticos = pReclamos
                    .Where(r => r.Prioridad == PrioridadReclamo.Critica || r.Prioridad == PrioridadReclamo.Alta)
                    .OrderByDescending(r => r.Prioridad)
                    .Take(5)
                    .Select(r => new AgendaItemDto
                    {
                        Tipo        = "reclamo",
                        Nivel       = r.Prioridad == PrioridadReclamo.Critica ? "critico" : "alerta",
                        Titulo      = $"Reclamo {r.Prioridad} activo — {r.NombreAsegurado}",
                        Descripcion = $"#{r.NumeroReclamo} · Estado: {r.Estado}" +
                                      (r.FechaLimiteRespuesta.HasValue
                                          ? $" · Límite: {r.FechaLimiteRespuesta.Value:dd/MM/yy}"
                                          : string.Empty),
                        NumeroRef   = r.NumeroReclamo,
                        NavLink     = "/reclamos"
                    })
                    .ToList();

                if (reclamosCriticos.Any())
                {
                    secciones.Add(new AgendaSeccionDto
                    {
                        Titulo = "RECLAMOS CRÍTICOS ACTIVOS",
                        Nivel  = "alerta",
                        Icono  = "report_problem",
                        Total  = reclamosCriticos.Count,
                        Items  = reclamosCriticos
                    });
                }

                // ══ SECCIÓN 4: LEADS CALIENTES — pólizas por vencer en 7 días ══
                var leads = pPolizas
                    .Where(p => p.FechaVigencia.Date > en2dias && p.FechaVigencia.Date <= en7dias)
                    .Select(p =>
                    {
                        var numPols = !string.IsNullOrEmpty(p.NumeroCedula) &&
                                      polizasPorCedula.TryGetValue(p.NumeroCedula!, out var np) ? np : 1;
                        var hayVencidos = vencidosPorPoliza.ContainsKey(p.NumeroPoliza!);
                        var tipo = tipoSeguroPorPoliza.TryGetValue(p.NumeroPoliza!, out var tp) ? tp : "OTROS";
                        var emailActivo = !string.IsNullOrEmpty(p.Correo) && emailsSet.Contains(p.Correo);

                        int score = 30
                            + (!hayVencidos ? 20 : 0)
                            + (numPols > 1   ? 15 : 0)
                            + (emailActivo   ? 10 : 0)
                            + (tipo is "VIDA" or "EMPRESARIAL" ? 10 : 0);

                        return new { Poliza = p, Score = score, Tipo = tipo };
                    })
                    .Where(x => x.Score >= 50)
                    .OrderByDescending(x => x.Score)
                    .Take(5)
                    .Select(x => new AgendaItemDto
                    {
                        Tipo        = "lead",
                        Nivel       = "aviso",
                        Titulo      = $"Lead caliente — {x.Poliza.NombreAsegurado}",
                        Descripcion = $"Score {x.Score}/100 · Vence en {(int)(x.Poliza.FechaVigencia.Date - hoy).TotalDays} días · {x.Tipo} · {x.Poliza.Aseguradora}",
                        NumeroRef   = x.Poliza.NumeroPoliza,
                        Score       = x.Score,
                        DiasVencido = (int)(x.Poliza.FechaVigencia.Date - hoy).TotalDays,
                        NavLink     = $"/polizas?numero={x.Poliza.NumeroPoliza}"
                    })
                    .ToList();

                if (leads.Any())
                {
                    secciones.Add(new AgendaSeccionDto
                    {
                        Titulo = "LEADS CALIENTES — Contactar esta semana",
                        Nivel  = "aviso",
                        Icono  = "local_fire_department",
                        Total  = leads.Count,
                        Items  = leads
                    });
                }

                // ══ SECCIÓN 5: COTIZACIONES SIN SEGUIMIENTO (+5 días PENDIENTE) ══
                var cotsPendientes = pCotizaciones
                    .Where(c => c.Estado == "PENDIENTE" && (now - c.FechaCreacion).TotalDays > 5)
                    .OrderByDescending(c => c.Prima)
                    .Take(5)
                    .Select(c => new AgendaItemDto
                    {
                        Tipo        = "cotizacion",
                        Nivel       = "info",
                        Titulo      = $"Cotización sin seguimiento — {c.NombreAsegurado}",
                        Descripcion = $"#{c.NumeroCotizacion} · {(int)(now - c.FechaCreacion).TotalDays} días pendiente · ₡{c.Prima:N0}",
                        NumeroRef   = c.NumeroCotizacion,
                        Monto       = c.Prima,
                        NavLink     = "/cotizaciones"
                    })
                    .ToList();

                if (cotsPendientes.Any())
                {
                    secciones.Add(new AgendaSeccionDto
                    {
                        Titulo = "COTIZACIONES SIN SEGUIMIENTO",
                        Nivel  = "info",
                        Icono  = "pending_actions",
                        Total  = cotsPendientes.Count,
                        Items  = cotsPendientes
                    });
                }

                return Ok(new AgendaDto
                {
                    FechaGeneracion = now,
                    TotalAcciones   = secciones.Sum(s => s.Total),
                    Secciones       = secciones
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando agenda inteligente");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MÓDULO 9 — Cliente 360°
        // GET /api/analytics/cliente360?cedula={cedula}
        // GET /api/analytics/cliente360?nombre={nombre}  → retorna sugerencias
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("cliente360")]
        public async Task<IActionResult> GetCliente360(
            [FromQuery] string? cedula,
            [FromQuery] string? nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cedula) && string.IsNullOrWhiteSpace(nombre))
                    return BadRequest("Debe indicar cedula o nombre.");

                // ── Búsqueda por nombre → retornar lista de sugerencias ──────
                if (!string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(cedula))
                {
                    var normalizedNombre = nombre.Trim();

                    var sugerencias = await _db.Polizas
                        .Where(p => !p.IsDeleted
                            && (p.NombreAsegurado != null && p.NombreAsegurado.Contains(normalizedNombre)
                             || p.NumeroCedula  != null && p.NumeroCedula.Contains(normalizedNombre)))
                        .GroupBy(p => new { p.NumeroCedula, p.NombreAsegurado, p.Correo })
                        .Select(g => new
                        {
                            Cedula        = g.Key.NumeroCedula ?? string.Empty,
                            NombreCompleto = g.Key.NombreAsegurado ?? string.Empty,
                            Email         = g.Key.Correo ?? string.Empty,
                            NumeroPolizas = g.Count(),
                            Polizas       = g.ToList()
                        })
                        .Take(10)
                        .ToListAsync();

                    var result = sugerencias.Select(s => new Cliente360SearchResultDto
                    {
                        Cedula         = s.Cedula,
                        NombreCompleto = s.NombreCompleto,
                        Email          = s.Email,
                        NumeroPolizas  = s.NumeroPolizas,
                        PrimaMensual   = s.Polizas.Sum(p => NormalizePrimaMensual(p.Frecuencia, p.Prima))
                    }).ToList();

                    return Ok(result);
                }

                // ── Vista completa por cédula ────────────────────────────────
                var polizas = await _db.Polizas
                    .Where(p => !p.IsDeleted && p.NumeroCedula == cedula)
                    .ToListAsync();

                if (!polizas.Any())
                    return NotFound($"No se encontraron pólizas para la cédula {cedula}.");

                var numPolizas = polizas
                    .Where(p => p.NumeroPoliza != null)
                    .Select(p => p.NumeroPoliza!)
                    .ToList();

                var cobros = await _db.Cobros
                    .Where(c => !c.IsDeleted && numPolizas.Contains(c.NumeroPoliza))
                    .ToListAsync();

                var reclamos = await _db.Reclamos
                    .Where(r => !r.IsDeleted && numPolizas.Contains(r.NumeroPoliza))
                    .ToListAsync();

                // Cotizaciones para resolver TipoSeguro por póliza
                // Materializamos primero para poder usar StringComparer en el GroupBy (EF Core no lo traduce a SQL).
                var cotizacionesRaw = await _db.Cotizaciones
                    .Where(c => !c.IsDeleted && numPolizas.Contains(c.NumeroPoliza))
                    .Select(c => new { c.NumeroPoliza, c.TipoSeguro, c.FechaCreacion })
                    .ToListAsync();

                var tipoSeguroByPoliza = cotizacionesRaw
                    .Where(c => !string.IsNullOrWhiteSpace(c.NumeroPoliza) && !string.IsNullOrWhiteSpace(c.TipoSeguro))
                    .GroupBy(c => c.NumeroPoliza!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(x => x.FechaCreacion)
                              .Select(x => x.TipoSeguro)
                              .FirstOrDefault() ?? "OTROS",
                        StringComparer.OrdinalIgnoreCase);

                // ── Métricas básicas ─────────────────────────────────────────
                var primerPoliza       = polizas.OrderBy(p => p.CreatedAt).First();
                var mesesComoCliente   = (int)Math.Max(1, (DateTime.Now - primerPoliza.CreatedAt).TotalDays / 30.0);
                var primaMensualActiva = polizas
                    .Where(p => p.EsActivo)
                    .Sum(p => NormalizePrimaMensual(p.Frecuencia, p.Prima));
                var totalCobrado  = cobros
                    .Where(c => c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado)
                    .Sum(c => c.MontoTotal);
                var totalReclamado = reclamos.Sum(r => r.MontoReclamado);
                var ltv = totalCobrado - totalReclamado;

                // ── Score Lealtad (0-100) ────────────────────────────────────
                int scoreLealtad = 0;
                if (mesesComoCliente >= 24) scoreLealtad += 30;
                else if (mesesComoCliente >= 12) scoreLealtad += 15;
                var cobrosVencidos = cobros.Count(c => c.Estado == EstadoCobro.Vencido);
                if (cobrosVencidos == 0) scoreLealtad += 30;
                else if (cobrosVencidos <= 2) scoreLealtad += 10;
                if (polizas.Count(p => p.EsActivo) > 1) scoreLealtad += 20;
                if (reclamos.Count == 0) scoreLealtad += 20;
                scoreLealtad = Math.Min(100, scoreLealtad);

                // ── Score Riesgo (0-100) ─────────────────────────────────────
                int scoreRiesgo = 0;
                var totalCobrosActivos = cobros.Count(c => c.Estado != EstadoCobro.Cancelado);
                if (totalCobrosActivos > 0)
                {
                    var tasaVencido = (double)cobrosVencidos / totalCobrosActivos;
                    if (tasaVencido >= 0.5) scoreRiesgo += 50;
                    else if (tasaVencido >= 0.25) scoreRiesgo += 25;
                }
                var cobrosVencidosMas30 = cobros.Count(c =>
                    c.Estado == EstadoCobro.Vencido &&
                    (DateTime.Now - c.FechaVencimiento).TotalDays > 30);
                scoreRiesgo += cobrosVencidosMas30 * 10;
                scoreRiesgo = Math.Min(100, scoreRiesgo);

                var categoriaRiesgo = scoreRiesgo >= 60 ? "Rojo"
                                    : scoreRiesgo >= 30 ? "Amarillo"
                                    : "Verde";

                // ── LTV Timeline (prima cobrada acumulada por mes) ────────────
                var ltvTimeline = cobros
                    .Where(c => c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado)
                    .GroupBy(c => new DateTime(c.FechaVencimiento.Year, c.FechaVencimiento.Month, 1))
                    .OrderBy(g => g.Key)
                    .Select(g => new ChartDataPointDto
                    {
                        Name  = g.Key.ToString("MMM yy", CultureInfo.InvariantCulture),
                        Value = g.Sum(c => c.MontoTotal)
                    })
                    .ToList();

                // ── Construir DTO ────────────────────────────────────────────
                var dto = new Cliente360Dto
                {
                    NombreCompleto     = primerPoliza.NombreAsegurado ?? string.Empty,
                    Cedula             = cedula!,
                    Email              = primerPoliza.Correo ?? string.Empty,
                    Telefono           = primerPoliza.NumeroTelefono ?? string.Empty,
                    LTV                = ltv,
                    PrimaMensualActiva = primaMensualActiva,
                    TotalReclamado     = totalReclamado,
                    TotalCobrado       = totalCobrado,
                    MesesComoCliente   = mesesComoCliente,
                    ScoreLealtad       = scoreLealtad,
                    ScoreRiesgo        = scoreRiesgo,
                    CategoriaRiesgo    = categoriaRiesgo,
                    LtvTimeline         = ltvTimeline,
                    Polizas = polizas
                        .Select(p => new Cliente360PolizaDto
                        {
                            NumeroPoliza   = p.NumeroPoliza ?? string.Empty,
                            TipoSeguro     = p.NumeroPoliza != null && tipoSeguroByPoliza.TryGetValue(p.NumeroPoliza, out var ts)
                                                ? ts
                                                : NormalizeTipoSeguro(p.Modalidad),
                            Aseguradora    = p.Aseguradora ?? string.Empty,
                            Prima          = NormalizePrimaMensual(p.Frecuencia, p.Prima),
                            Frecuencia     = p.Frecuencia ?? string.Empty,
                            FechaVigencia  = p.FechaVigencia,
                            EsActiva       = p.EsActivo,
                            DiasParaVencer = (int)(p.FechaVigencia - DateTime.Now).TotalDays
                        })
                        .OrderByDescending(p => p.EsActiva)
                        .ThenByDescending(p => p.Prima)
                        .ToList(),
                    Cobros = cobros
                        .OrderByDescending(c => c.FechaVencimiento)
                        .Take(20)
                        .Select(c => new Cliente360CobroDto
                        {
                            NumeroRecibo     = c.NumeroRecibo,
                            Estado           = c.Estado.ToString(),
                            MontoTotal       = c.MontoTotal,
                            FechaVencimiento = c.FechaVencimiento,
                            DiasVencido      = (int)(DateTime.Now - c.FechaVencimiento).TotalDays
                        })
                        .ToList(),
                    Reclamos = reclamos
                        .OrderByDescending(r => r.FechaReclamo)
                        .Select(r => new Cliente360ReclamoDto
                        {
                            NumeroReclamo  = r.NumeroReclamo,
                            TipoReclamo    = r.TipoReclamo.ToString(),
                            Estado         = r.Estado.ToString(),
                            Prioridad      = r.Prioridad.ToString(),
                            MontoClamado   = r.MontoReclamado,
                            MontoAprobado  = r.MontoAprobado ?? 0m,
                            DiasResolucion = r.FechaResolucion.HasValue
                                ? (int)(r.FechaResolucion.Value - r.FechaReclamo).TotalDays
                                : -1
                        })
                        .ToList()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo Cliente 360° para cédula {Cedula}", cedula);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private static decimal NormalizePrimaMensual(string? frecuencia, decimal prima)
        {
            var freq = (frecuencia ?? "MENSUAL").Trim().ToUpperInvariant();
            return freq switch
            {
                "ANUAL" => prima / 12m,
                "SEMESTRAL" => prima / 6m,
                "TRIMESTRAL" => prima / 3m,
                "BIMESTRAL" => prima / 2m,
                _ => prima
            };
        }

        private static string NormalizeTipoSeguro(string? tipoSeguro)
        {
            if (string.IsNullOrWhiteSpace(tipoSeguro))
            {
                return "OTROS";
            }

            return tipoSeguro.Trim().ToUpperInvariant();
        }

        private static string NormalizeTipoSeguro(string? tipoSeguro, string? modalidadFallback)
        {
            if (!string.IsNullOrWhiteSpace(tipoSeguro))
            {
                return NormalizeTipoSeguro(tipoSeguro);
            }

            return NormalizeTipoSeguro(modalidadFallback);
        }
    }
}
