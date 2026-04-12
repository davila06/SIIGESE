using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace WebApi.Services;

public interface IAnalyticsDashboardDomainService
{
    Task<ExecutiveDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}

public interface IAnalyticsPortfolioDomainService
{
    Task<PortfolioDistribucionDto> GetPortfolioAsync(CancellationToken cancellationToken = default);
    Task<VencimientosTimelineDto> GetVencimientosAsync(int months, CancellationToken cancellationToken = default);
    Task<VencimientoDetalleMesDto> GetVencimientosDetalleAsync(DateTime monthStart, string monthToken, CancellationToken cancellationToken = default);
}

public interface IAnalyticsReclamosDomainService
{
    Task<ReclamosFunnelDto> GetReclamosFunnelAsync(CancellationToken cancellationToken = default);
    Task<SlaReportDto> GetReclamosSlaAsync(CancellationToken cancellationToken = default);
    Task<ReclamosAnalyticsAdvancedDto> GetReclamosAdvancedAsync(int months, int? agenteId, string? aseguradora, CancellationToken cancellationToken = default);
}

public sealed class AnalyticsDashboardDomainService : IAnalyticsDashboardDomainService
{
    private readonly ApplicationDbContext _db;

    private static readonly Dictionary<string, decimal> FrecuenciaMultiplier = new(StringComparer.OrdinalIgnoreCase)
    {
        { "MENSUAL",        1m      },
        { "BIMESTRAL",      1m / 2m },
        { "TRIMESTRAL",     1m / 3m },
        { "CUATRIMESTRAL",  1m / 4m },
        { "SEMESTRAL",      1m / 6m },
        { "ANUAL",          1m / 12m }
    };

    private sealed record PolizaActivaDashboardRow(decimal Prima, string? Frecuencia, string? Aseguradora);
    private sealed record CobroDashboardRow(DateTime FechaVencimiento, decimal MontoTotal, decimal MontoCobrado, EstadoCobro Estado, string? Moneda);
    private sealed record DashboardMonthAggregate(string Label, decimal Cobrado, decimal Esperado);
    private sealed record DashboardChartBundle(
        List<MultiSeriesChartDto> CobrosMensuales,
        List<MultiSeriesChartDto> CobrosEstadoMensual,
        List<ChartSeriesDto> DistribucionAseguradoras,
        List<ChartDataPointDto> SparklineTasaCobro,
        List<ChartDataPointDto> SparklineMontoRiesgo);

    public AnalyticsDashboardDomainService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ExecutiveDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var mesActualInicio = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var mesAnteriorInicio = mesActualInicio.AddMonths(-1);

        var carteraActiva = await _db.Polizas.CountAsync(p => p.EsActivo && !p.IsDeleted, cancellationToken);

        var polizasActivas = await _db.Polizas
            .Where(p => p.EsActivo && !p.IsDeleted)
            .Select(p => new PolizaActivaDashboardRow(p.Prima, p.Frecuencia, p.Aseguradora))
            .ToListAsync(cancellationToken);

        var primaMensualBruta = polizasActivas.Sum(p => NormalizePrimaMensual(p.Frecuencia, p.Prima));

        var cobrosEstesMes = await _db.Cobros
            .Where(c => !c.IsDeleted &&
                        c.FechaVencimiento >= mesActualInicio &&
                        c.FechaVencimiento < mesActualInicio.AddMonths(1))
            .Select(c => new { c.MontoTotal, c.MontoCobrado, c.Estado })
            .ToListAsync(cancellationToken);

        var totalEsperado = cobrosEstesMes.Sum(c => c.MontoTotal);
        var totalCobrado = cobrosEstesMes.Sum(c => c.MontoCobrado);
        var tasaCobro = Percentage(totalCobrado, totalEsperado);

        var montoEnRiesgo = await _db.Cobros
            .Where(c => !c.IsDeleted && (c.Estado == EstadoCobro.Pendiente || c.Estado == EstadoCobro.Vencido))
            .SumAsync(c => c.MontoTotal, cancellationToken);

        var reclamosActivos = await _db.Reclamos
            .CountAsync(r => !r.IsDeleted
                && r.Estado != EstadoReclamo.Resuelto
                && r.Estado != EstadoReclamo.Cerrado, cancellationToken);

        var cotizacionesMes = await _db.Cotizaciones
            .Where(c => !c.IsDeleted && c.FechaCreacion >= mesActualInicio)
            .Select(c => c.Estado)
            .ToListAsync(cancellationToken);

        var tasaConversion = cotizacionesMes.Count > 0
            ? Math.Round((double)cotizacionesMes.Count(e => e == "CONVERTIDA") / cotizacionesMes.Count * 100, 1)
            : 0;

        var emailStatsRaw = await _db.EmailLogs
            .Where(e => !e.IsDeleted && e.SentAt >= mesActualInicio)
            .Select(e => e.IsSuccess)
            .ToListAsync(cancellationToken);

        var tasaEmail = emailStatsRaw.Count > 0
            ? Math.Round((double)emailStatsRaw.Count(s => s) / emailStatsRaw.Count * 100, 1)
            : 100;

        var reclamosConSla = await _db.Reclamos
            .Where(r => !r.IsDeleted && r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue)
            .Select(r => new { r.FechaResolucion, r.FechaLimiteRespuesta })
            .ToListAsync(cancellationToken);

        var slaReclamos = reclamosConSla.Count > 0
            ? Math.Round((double)reclamosConSla.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value) / reclamosConSla.Count * 100, 1)
            : 100;

        var cobrosMesAnt = await _db.Cobros
            .Where(c => !c.IsDeleted &&
                        c.FechaVencimiento >= mesAnteriorInicio &&
                        c.FechaVencimiento < mesActualInicio)
            .Select(c => new { c.MontoTotal, c.MontoCobrado })
            .ToListAsync(cancellationToken);

        var totalAntEsperado = cobrosMesAnt.Sum(c => c.MontoTotal);
        var totalAntCobrado = cobrosMesAnt.Sum(c => c.MontoCobrado);
        var tasaCobroAnt = Percentage(totalAntCobrado, totalAntEsperado);

        var cobrosHistorico = await _db.Cobros
            .Where(c => !c.IsDeleted && c.FechaVencimiento >= now.AddMonths(-12))
            .Select(c => new CobroDashboardRow(c.FechaVencimiento, c.MontoTotal, c.MontoCobrado, c.Estado, c.Moneda))
            .ToListAsync(cancellationToken);

        var chartBundle = BuildDashboardChartBundle(now, cobrosHistorico, polizasActivas);

        var polizasPorRenovar30d = await _db.Polizas
            .CountAsync(p => p.EsActivo && !p.IsDeleted
                && p.FechaVigencia >= now
                && p.FechaVigencia <= now.AddDays(30), cancellationToken);

        var reclamosSinAsignar = await _db.Reclamos
            .CountAsync(r => !r.IsDeleted
                && r.UsuarioAsignadoId == null
                && r.Estado != EstadoReclamo.Resuelto
                && r.Estado != EstadoReclamo.Cerrado, cancellationToken);

        var tiemposResolucion = await _db.Reclamos
            .Where(r => !r.IsDeleted && r.FechaResolucion.HasValue)
            .Select(r => new { r.FechaReclamo, FechaRes = r.FechaResolucion!.Value })
            .ToListAsync(cancellationToken);

        var tiempoPromedioResolucion = tiemposResolucion.Count > 0
            ? Math.Round((decimal)tiemposResolucion
                .Average(r => (r.FechaRes - r.FechaReclamo).TotalDays), 1)
            : 0m;

        var cobrosPeriodoAct = cobrosHistorico
            .Where(c => c.FechaVencimiento >= now.AddDays(-30)).ToList();

        var tasaMorosidadAct = cobrosPeriodoAct.Count > 0
            ? Math.Round((decimal)cobrosPeriodoAct.Count(c => c.Estado == EstadoCobro.Vencido)
                / cobrosPeriodoAct.Count * 100m, 1)
            : 0m;

        var cobrosPeriodoAnt = cobrosHistorico
            .Where(c => c.FechaVencimiento >= now.AddDays(-60)
                     && c.FechaVencimiento < now.AddDays(-30)).ToList();

        var tasaMorosidadAnt = cobrosPeriodoAnt.Count > 0
            ? Math.Round((decimal)cobrosPeriodoAnt.Count(c => c.Estado == EstadoCobro.Vencido)
                / cobrosPeriodoAnt.Count * 100m, 1)
            : 0m;

        var reclamosMontosRaw = await _db.Reclamos
            .Where(r => !r.IsDeleted && r.MontoAprobado.HasValue && r.MontoReclamado > 0)
            .Select(r => new { r.MontoReclamado, MontoAprobado = r.MontoAprobado!.Value })
            .ToListAsync(cancellationToken);

        var ratioPerdidas = reclamosMontosRaw.Count > 0
            ? Math.Round(reclamosMontosRaw.Sum(r => r.MontoAprobado)
                / reclamosMontosRaw.Sum(r => r.MontoReclamado) * 100m, 1)
            : 0m;

        var ticketAct = cobrosEstesMes.Count > 0
            ? Math.Round(cobrosEstesMes.Average(c => c.MontoTotal), 0)
            : 0m;

        var ticketAnt = cobrosMesAnt.Count > 0
            ? Math.Round(cobrosMesAnt.Average(c => c.MontoTotal), 0)
            : 0m;

        var tasaReclamos = carteraActiva > 0
            ? Math.Round((decimal)reclamosActivos / carteraActiva * 100m, 2)
            : 0m;

        var polizasNuevasMesAct = await _db.Polizas
            .CountAsync(p => !p.IsDeleted && p.CreatedAt >= mesActualInicio, cancellationToken);

        var polizasNuevasMesAnt = await _db.Polizas
            .CountAsync(p => !p.IsDeleted
                && p.CreatedAt >= mesAnteriorInicio
                && p.CreatedAt < mesActualInicio, cancellationToken);

        var primaPromedioPorPoliza = carteraActiva > 0
            ? Math.Round(primaMensualBruta / carteraActiva, 0)
            : 0m;

        var cobrosCanceladosMes = cobrosEstesMes.Count(c => c.Estado == EstadoCobro.Cancelado);

        var porcentajeCobroUsd = cobrosHistorico.Count > 0
            ? Math.Round((decimal)cobrosHistorico
                .Count(c => string.Equals(c.Moneda, "USD", StringComparison.OrdinalIgnoreCase))
                / cobrosHistorico.Count * 100m, 1)
            : 0m;

        var reclamosFueraDeSla = await _db.Reclamos
            .CountAsync(r => !r.IsDeleted
                && r.FechaLimiteRespuesta.HasValue
                && r.FechaLimiteRespuesta.Value < now
                && r.Estado != EstadoReclamo.Resuelto
                && r.Estado != EstadoReclamo.Cerrado, cancellationToken);

        var cotizacionesPipeline = await _db.Cotizaciones
            .Where(c => !c.IsDeleted && c.Estado == "PENDIENTE")
            .Select(c => c.Prima)
            .ToListAsync(cancellationToken);

        var cotizacionesEnPipeline = cotizacionesPipeline.Count;
        var valorPipelineCotizaciones = Math.Round(cotizacionesPipeline.Sum(), 0);

        var conversionesRaw = await _db.Cotizaciones
            .Where(c => !c.IsDeleted
                && c.Estado == "CONVERTIDA"
                && c.FechaActualizacion.HasValue)
            .Select(c => new { c.FechaCreacion, FechaConv = c.FechaActualizacion!.Value })
            .ToListAsync(cancellationToken);

        var tiempoPromedioConversion = conversionesRaw.Count > 0
            ? Math.Round((decimal)conversionesRaw
                .Average(c => (c.FechaConv - c.FechaCreacion).TotalDays), 1)
            : 0m;

        var polizasInactivasMes = await _db.Polizas
            .CountAsync(p => !p.IsDeleted
                && !p.EsActivo
                && p.UpdatedAt.HasValue
                && p.UpdatedAt.Value >= mesActualInicio, cancellationToken);

        var alertas = await BuildDashboardAlertsAsync(
            now,
            polizasPorRenovar30d,
            reclamosSinAsignar,
            reclamosFueraDeSla,
            polizasInactivasMes,
            cancellationToken);

        return new ExecutiveDashboardDto
        {
            CarteraActiva = carteraActiva,
            PrimaMensualBruta = Math.Round(primaMensualBruta, 0),
            TasaCobro = tasaCobro,
            MontoEnRiesgo = Math.Round(montoEnRiesgo, 0),
            ReclamosActivos = reclamosActivos,
            TasaConversion = (decimal)tasaConversion,
            TasaEmailExito = (decimal)tasaEmail,
            SlaReclamos = (decimal)slaReclamos,
            TasaCobroTrend = tasaCobroAnt > 0 ? Math.Round(tasaCobro - tasaCobroAnt, 1) : 0,
            PolizasPorRenovar30d = polizasPorRenovar30d,
            ReclamosSinAsignar = reclamosSinAsignar,
            TiempoPromedioResolucionDias = tiempoPromedioResolucion,
            TasaMorosidad = tasaMorosidadAct,
            TasaMorosidadTrend = Math.Round(tasaMorosidadAct - tasaMorosidadAnt, 1),
            RatioPerdidas = ratioPerdidas,
            TicketPromedioCobro = ticketAct,
            TicketPromedioCobroTrend = Math.Round(ticketAct - ticketAnt, 0),
            TasaReclamos = tasaReclamos,
            PolizasNuevasMes = polizasNuevasMesAct,
            PolizasNuevasMesTrend = polizasNuevasMesAct - polizasNuevasMesAnt,
            PrimaPromedioPorPoliza = primaPromedioPorPoliza,
            CobrosCanceladosMes = cobrosCanceladosMes,
            PorcentajeCobroUsd = porcentajeCobroUsd,
            ReclamosFueraDeSla = reclamosFueraDeSla,
            ValorPipelineCotizaciones = valorPipelineCotizaciones,
            CotizacionesEnPipeline = cotizacionesEnPipeline,
            TiempoPromedioConversion = tiempoPromedioConversion,
            PolizasInactivasMes = polizasInactivasMes,
            Alertas = alertas,
            CobrosMensuales = chartBundle.CobrosMensuales,
            CobrosEstadoMensual = chartBundle.CobrosEstadoMensual,
            DistribucionAseguradoras = chartBundle.DistribucionAseguradoras,
            SparklineTasaCobro = chartBundle.SparklineTasaCobro,
            SparklineMontoRiesgo = chartBundle.SparklineMontoRiesgo
        };
    }

    private static decimal NormalizePrimaMensual(string? frecuencia, decimal prima)
    {
        return FrecuenciaMultiplier.TryGetValue(frecuencia ?? "MENSUAL", out var multiplier)
            ? prima * multiplier
            : prima;
    }

    private static decimal Percentage(decimal numerator, decimal denominator, int decimals = 1)
        => denominator > 0 ? Math.Round(numerator / denominator * 100m, decimals) : 0m;

    private static DashboardChartBundle BuildDashboardChartBundle(
        DateTime now,
        IReadOnlyCollection<CobroDashboardRow> cobrosHistorico,
        IReadOnlyCollection<PolizaActivaDashboardRow> polizasActivas)
    {
        var cobrosMensualesGrupos = BuildDashboardMonthAggregates(cobrosHistorico);

        var cobrosMensuales = BuildCobrosMensualesSeries(cobrosMensualesGrupos);
        var cobrosEstadoMensual = BuildCobrosEstadoMensualSeries(cobrosHistorico, now);
        var distribAseg = BuildDistribucionAseguradoras(polizasActivas);
        var sparklineTasaCobro = BuildSparklineTasaCobro(cobrosMensualesGrupos);
        var sparklineMontoRiesgo = BuildSparklineMontoRiesgo(cobrosHistorico);

        return new DashboardChartBundle(
            CobrosMensuales: cobrosMensuales,
            CobrosEstadoMensual: cobrosEstadoMensual,
            DistribucionAseguradoras: distribAseg,
            SparklineTasaCobro: sparklineTasaCobro,
            SparklineMontoRiesgo: sparklineMontoRiesgo);
    }

    private static List<DashboardMonthAggregate> BuildDashboardMonthAggregates(
        IEnumerable<CobroDashboardRow> cobrosHistorico)
    {
        return cobrosHistorico
            .GroupBy(c => new { c.FechaVencimiento.Year, c.FechaVencimiento.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new DashboardMonthAggregate(
                Label: new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc).ToString("MMM yy"),
                Cobrado: g.Sum(c => c.MontoCobrado),
                Esperado: g.Sum(c => c.MontoTotal)))
            .ToList();
    }

    private static List<MultiSeriesChartDto> BuildCobrosMensualesSeries(
        IEnumerable<DashboardMonthAggregate> cobrosMensualesGrupos)
    {
        var grupos = cobrosMensualesGrupos.ToList();
        return new List<MultiSeriesChartDto>
        {
            new()
            {
                Name = "Cobrado",
                Series = grupos
                    .Select(g => new ChartDataPointDto { Name = g.Label, Value = g.Cobrado })
                    .ToList()
            },
            new()
            {
                Name = "Esperado",
                Series = grupos
                    .Select(g => new ChartDataPointDto { Name = g.Label, Value = g.Esperado })
                    .ToList()
            }
        };
    }

    private static List<MultiSeriesChartDto> BuildCobrosEstadoMensualSeries(
        IEnumerable<CobroDashboardRow> cobrosHistorico,
        DateTime now)
    {
        return cobrosHistorico
            .Where(c => c.FechaVencimiento >= now.AddMonths(-6))
            .GroupBy(c => new { c.FechaVencimiento.Year, c.FechaVencimiento.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g =>
            {
                var label = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc).ToString("MMM yy");
                return new MultiSeriesChartDto
                {
                    Name = label,
                    Series = new List<ChartDataPointDto>
                    {
                        new() { Name = "Cobrado", Value = g.Where(c => c.Estado == EstadoCobro.Cobrado || c.Estado == EstadoCobro.Pagado).Sum(c => c.MontoCobrado) },
                        new() { Name = "Pendiente", Value = g.Where(c => c.Estado == EstadoCobro.Pendiente).Sum(c => c.MontoTotal) },
                        new() { Name = "Vencido", Value = g.Where(c => c.Estado == EstadoCobro.Vencido).Sum(c => c.MontoTotal) },
                        new() { Name = "Cancelado", Value = g.Where(c => c.Estado == EstadoCobro.Cancelado).Sum(c => c.MontoTotal) }
                    }
                };
            })
            .ToList();
    }

    private static List<ChartSeriesDto> BuildDistribucionAseguradoras(
        IEnumerable<PolizaActivaDashboardRow> polizasActivas)
    {
        return polizasActivas
            .GroupBy(p => p.Aseguradora ?? "OTROS")
            .Select(g => new ChartSeriesDto
            {
                Name = g.Key,
                Value = g.Sum(p => NormalizePrimaMensual(p.Frecuencia, p.Prima))
            })
            .OrderByDescending(g => g.Value)
            .ToList();
    }

    private static List<ChartDataPointDto> BuildSparklineTasaCobro(
        IEnumerable<DashboardMonthAggregate> cobrosMensualesGrupos)
    {
        return cobrosMensualesGrupos
            .TakeLast(6)
            .Select(g => new ChartDataPointDto
            {
                Name = g.Label,
                Value = Percentage(g.Cobrado, g.Esperado)
            })
            .ToList();
    }

    private static List<ChartDataPointDto> BuildSparklineMontoRiesgo(
        IEnumerable<CobroDashboardRow> cobrosHistorico)
    {
        return cobrosHistorico
            .GroupBy(c => new { c.FechaVencimiento.Year, c.FechaVencimiento.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .TakeLast(6)
            .Select(g => new ChartDataPointDto
            {
                Name = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc).ToString("MMM yy"),
                Value = g.Where(c => c.Estado == EstadoCobro.Pendiente || c.Estado == EstadoCobro.Vencido)
                         .Sum(c => c.MontoTotal)
            })
            .ToList();
    }

    private async Task<List<AlertaDto>> BuildDashboardAlertsAsync(
        DateTime now,
        int polizasPorRenovar30d,
        int reclamosSinAsignar,
        int reclamosFueraDeSla,
        int polizasInactivasMes,
        CancellationToken cancellationToken)
    {
        var alertas = new List<AlertaDto>();

        var cobrosVencidosMas30 = await _db.Cobros
            .CountAsync(c => !c.IsDeleted && c.Estado == EstadoCobro.Vencido
                && c.FechaVencimiento < now.AddDays(-30), cancellationToken);
        if (cobrosVencidosMas30 > 0)
            alertas.Add(new AlertaDto { Tipo = "CRITICO", Icono = "error", Mensaje = $"{cobrosVencidosMas30} cobros vencidos hace más de 30 días sin gestión", Cantidad = cobrosVencidosMas30, RutaAccion = "/cobros" });

        var polizasPorVencer15 = await _db.Polizas
            .CountAsync(p => p.EsActivo && !p.IsDeleted
                && p.FechaVigencia >= now && p.FechaVigencia <= now.AddDays(15), cancellationToken);
        if (polizasPorVencer15 > 0)
            alertas.Add(new AlertaDto { Tipo = "ALERTA", Icono = "schedule", Mensaje = $"{polizasPorVencer15} pólizas vencen en los próximos 15 días", Cantidad = polizasPorVencer15, RutaAccion = "/polizas" });

        var reclamosCriticosSinAsignar = await _db.Reclamos
            .CountAsync(r => !r.IsDeleted && r.Prioridad == PrioridadReclamo.Critica && r.UsuarioAsignadoId == null, cancellationToken);
        if (reclamosCriticosSinAsignar > 0)
            alertas.Add(new AlertaDto { Tipo = "AVISO", Icono = "priority_high", Mensaje = $"{reclamosCriticosSinAsignar} reclamos críticos sin asignar", Cantidad = reclamosCriticosSinAsignar, RutaAccion = "/reclamos" });

        var cotizacionesPendientes48h = await _db.Cotizaciones
            .CountAsync(c => !c.IsDeleted && c.Estado == "PENDIENTE" && c.FechaCreacion <= now.AddHours(-48), cancellationToken);
        if (cotizacionesPendientes48h > 0)
            alertas.Add(new AlertaDto { Tipo = "INFO", Icono = "info", Mensaje = $"{cotizacionesPendientes48h} cotizaciones llevan más de 48h en pendiente", Cantidad = cotizacionesPendientes48h, RutaAccion = "/cotizaciones" });

        if (polizasPorRenovar30d > 0)
            alertas.Add(new AlertaDto { Tipo = "AVISO", Icono = "autorenew", Mensaje = $"{polizasPorRenovar30d} pól. vencen próximos 30 días — confirmar renovación", Cantidad = polizasPorRenovar30d, RutaAccion = "/polizas" });

        if (reclamosSinAsignar > 3)
            alertas.Add(new AlertaDto { Tipo = "ALERTA", Icono = "person_off", Mensaje = $"{reclamosSinAsignar} reclamos activos sin agente asignado", Cantidad = reclamosSinAsignar, RutaAccion = "/reclamos" });

        if (reclamosFueraDeSla > 0)
            alertas.Add(new AlertaDto { Tipo = "CRITICO", Icono = "timer_off", Mensaje = $"{reclamosFueraDeSla} reclamos activos han superado su fecha límite SLA", Cantidad = reclamosFueraDeSla, RutaAccion = "/reclamos" });

        if (polizasInactivasMes > 0)
            alertas.Add(new AlertaDto { Tipo = "AVISO", Icono = "cancel", Mensaje = $"{polizasInactivasMes} pólizas dadas de baja este mes (churn)", Cantidad = polizasInactivasMes, RutaAccion = "/polizas" });

        return alertas;
    }
}

public sealed class AnalyticsPortfolioDomainService : IAnalyticsPortfolioDomainService
{
    private readonly ApplicationDbContext _db;

    private static readonly Dictionary<string, decimal> FrecuenciaMultiplier = new(StringComparer.OrdinalIgnoreCase)
    {
        { "MENSUAL",        1m      },
        { "BIMESTRAL",      1m / 2m },
        { "TRIMESTRAL",     1m / 3m },
        { "CUATRIMESTRAL",  1m / 4m },
        { "SEMESTRAL",      1m / 6m },
        { "ANUAL",          1m / 12m }
    };

    public AnalyticsPortfolioDomainService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PortfolioDistribucionDto> GetPortfolioAsync(CancellationToken cancellationToken = default)
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
            .ToListAsync(cancellationToken);

        if (!polizasActivas.Any())
        {
            return new PortfolioDistribucionDto();
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
            .ToListAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);

        var cobrosPorPolizaDict = cobrosPorPoliza
            .ToDictionary(x => x.NumeroPoliza, x => new { x.TotalEsperado, x.TotalCobrado }, StringComparer.OrdinalIgnoreCase);

        var reclamosPorPoliza = await _db.Reclamos
            .Where(r => !r.IsDeleted && !string.IsNullOrWhiteSpace(r.NumeroPoliza))
            .GroupBy(r => r.NumeroPoliza!)
            .Select(g => new { NumeroPoliza = g.Key, Total = g.Count() })
            .ToListAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);

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
                    if (!string.IsNullOrWhiteSpace(x.NumeroPoliza) && montoAseguradoProxyByPoliza.TryGetValue(x.NumeroPoliza, out var val))
                        return val;
                    return x.PrimaMensual * 12m;
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

        return new PortfolioDistribucionDto
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
        };
    }

    public async Task<VencimientosTimelineDto> GetVencimientosAsync(int months, CancellationToken cancellationToken = default)
    {
        months = Math.Clamp(months, 1, 24);

        var now = DateTime.UtcNow;
        var inicioMes = new DateTime(now.Year, now.Month, 1);
        var finHorizonte = inicioMes.AddMonths(months);

        var cotizaciones = await _db.Cotizaciones
            .Where(c => !c.IsDeleted)
            .Select(c => new { c.NumeroPoliza, c.TipoSeguro, c.FechaCreacion })
            .ToListAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);

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

        return new VencimientosTimelineDto
        {
            Meses = porMes,
            TotalPolizasProximasAnio = polizas.Count,
            PrimaEnRiesgoAnio = Math.Round(porMes.Sum(x => x.PrimaEnRiesgo), 2)
        };
    }

    public async Task<VencimientoDetalleMesDto> GetVencimientosDetalleAsync(DateTime monthStart, string monthToken, CancellationToken cancellationToken = default)
    {
        var inicioMes = new DateTime(monthStart.Year, monthStart.Month, 1);
        var finMes = inicioMes.AddMonths(1);

        var cotizaciones = await _db.Cotizaciones
            .Where(c => !c.IsDeleted)
            .Select(c => new { c.NumeroPoliza, c.TipoSeguro, c.FechaCreacion })
            .ToListAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);

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

        return new VencimientoDetalleMesDto
        {
            Mes = monthToken,
            MesLabel = inicioMes.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
            Polizas = detalle
        };
    }

    private static decimal NormalizePrimaMensual(string? frecuencia, decimal prima)
    {
        return FrecuenciaMultiplier.TryGetValue(frecuencia ?? "MENSUAL", out var multiplier)
            ? prima * multiplier
            : prima;
    }

    private static string NormalizeTipoSeguro(string? tipoSeguro, string? modalidad)
    {
        var raw = (tipoSeguro ?? modalidad ?? string.Empty).Trim().ToUpperInvariant();

        if (raw.Contains("AUTO") || raw.Contains("VEHIC"))
            return "AUTO";
        if (raw.Contains("VIDA") || raw.Contains("SALUD"))
            return "VIDA";
        if (raw.Contains("HOGAR") || raw.Contains("CASA") || raw.Contains("VIVIEN"))
            return "HOGAR";
        if (raw.Contains("EMPRESA") || raw.Contains("COMERCIAL") || raw.Contains("PYME"))
            return "EMPRESARIAL";

        return string.IsNullOrWhiteSpace(raw) ? "OTROS" : raw;
    }
}

public sealed class AnalyticsReclamosDomainService : IAnalyticsReclamosDomainService
{
    private readonly ApplicationDbContext _db;
    private static readonly Dictionary<string, decimal> FrecuenciaMultiplier = new(StringComparer.OrdinalIgnoreCase)
    {
        { "MENSUAL", 1m },
        { "BIMESTRAL", 1m / 2m },
        { "TRIMESTRAL", 1m / 3m },
        { "CUATRIMESTRAL", 1m / 4m },
        { "SEMESTRAL", 1m / 6m },
        { "ANUAL", 1m / 12m }
    };

    public AnalyticsReclamosDomainService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ReclamosFunnelDto> GetReclamosFunnelAsync(CancellationToken cancellationToken = default)
    {
        var reclamos = await _db.Reclamos
            .Where(r => !r.IsDeleted)
            .Select(r => new { r.Estado, r.MontoReclamado, r.MontoAprobado, r.FechaReclamo, r.FechaResolucion })
            .ToListAsync(cancellationToken);

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
                Nombre = e.Item1,
                Estado = e.Item2[0],
                Cantidad = items.Count,
                Monto = items.Sum(r => r.MontoReclamado),
                TiempoPromedioHoras = Math.Round(tiempoPromedio, 1),
                PorcentajeDel100 = total > 0 ? Math.Round((double)items.Count / total * 100, 1) : 0
            };
        }).ToList();

        var totalReclamado = reclamos.Sum(r => r.MontoReclamado);
        var totalAprobado = reclamos.Sum(r => r.MontoAprobado ?? 0);

        return new ReclamosFunnelDto
        {
            Etapas = etapas,
            MontoTotalReclamado = totalReclamado,
            MontoTotalAprobado = totalAprobado,
            LossRatioGlobal = totalReclamado > 0 ? Math.Round(totalAprobado / totalReclamado * 100, 1) : 0
        };
    }

    public async Task<SlaReportDto> GetReclamosSlaAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var reclamos = await _db.Reclamos
            .Where(r => !r.IsDeleted)
            .Select(r => new
            {
                r.Id,
                r.NumeroReclamo,
                r.Estado,
                r.TipoReclamo,
                r.Prioridad,
                r.FechaReclamo,
                r.FechaLimiteRespuesta,
                r.FechaResolucion,
                r.UsuarioAsignadoId,
                r.ClienteNombreCompleto,
                r.MontoAprobado
            })
            .ToListAsync(cancellationToken);

        var usuarios = await _db.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new { u.Id, Nombre = u.FirstName + " " + u.LastName })
            .ToDictionaryAsync(u => u.Id, u => u.Nombre, cancellationToken);

        var conSla = reclamos.Where(r => r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue).ToList();
        var globalPct = conSla.Count > 0
            ? Math.Round((double)conSla.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value) / conSla.Count * 100, 1)
            : 100;

        var porTipo = reclamos
            .GroupBy(r => r.TipoReclamo.ToString())
            .Select(g =>
            {
                var conSlaG = g.Where(r => r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue).ToList();
                var dentroG = conSlaG.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value);
                var conResol = g.Where(r => r.FechaResolucion.HasValue).ToList();
                return new SlaPorTipoDto
                {
                    Nombre = g.Key,
                    TotalReclamos = g.Count(),
                    DentroSla = dentroG,
                    FueraSla = conSlaG.Count - dentroG,
                    PorcentajeDentroSla = conSlaG.Count > 0 ? Math.Round((double)dentroG / conSlaG.Count * 100, 1) : 100,
                    TiempoPromedioHoras = conResol.Count > 0 ? Math.Round(conResol.Average(r => (r.FechaResolucion!.Value - r.FechaReclamo).TotalHours), 1) : 0
                };
            }).ToList();

        var porPrioridad = reclamos
            .GroupBy(r => r.Prioridad.ToString())
            .Select(g =>
            {
                var conSlaG = g.Where(r => r.FechaResolucion.HasValue && r.FechaLimiteRespuesta.HasValue).ToList();
                var dentroG = conSlaG.Count(r => r.FechaResolucion!.Value <= r.FechaLimiteRespuesta!.Value);
                return new SlaPorTipoDto
                {
                    Nombre = g.Key,
                    TotalReclamos = g.Count(),
                    DentroSla = dentroG,
                    FueraSla = conSlaG.Count - dentroG,
                    PorcentajeDentroSla = conSlaG.Count > 0 ? Math.Round((double)dentroG / conSlaG.Count * 100, 1) : 100
                };
            }).ToList();

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
                    NombreAgente = nombre,
                    TotalAsignados = g.Count(),
                    DentroSla = dentroG,
                    PorcentajeDentroSla = conSlaG.Count > 0 ? Math.Round((double)dentroG / conSlaG.Count * 100, 1) : 100,
                    TiempoPromedioHoras = g.Where(r => r.FechaResolucion.HasValue).Any()
                        ? Math.Round(g.Where(r => r.FechaResolucion.HasValue).Average(r => (r.FechaResolucion!.Value - r.FechaReclamo).TotalHours), 1)
                        : 0,
                    MontoAprobado = g.Sum(r => r.MontoAprobado ?? 0)
                };
            }).ToList();

        var proximosVencer = reclamos
            .Where(r => r.FechaLimiteRespuesta.HasValue
                && r.FechaLimiteRespuesta.Value > now
                && r.Estado != EstadoReclamo.Resuelto
                && r.Estado != EstadoReclamo.Cerrado
                && (r.FechaLimiteRespuesta.Value - now).TotalHours <= 24)
            .Select(r => new ReclamoAlertoSlaDto
            {
                Id = r.Id,
                NumeroReclamo = r.NumeroReclamo,
                ClienteNombre = r.ClienteNombreCompleto,
                FechaLimite = r.FechaLimiteRespuesta!.Value,
                HorasRestantes = (int)(r.FechaLimiteRespuesta.Value - now).TotalHours,
                Prioridad = r.Prioridad.ToString()
            })
            .OrderBy(r => r.HorasRestantes)
            .ToList();

        return new SlaReportDto
        {
            PorcentajeGlobalDentroSLA = globalPct,
            PorTipo = porTipo,
            PorPrioridad = porPrioridad,
            PorAgente = porAgente,
            ProximosVencer = proximosVencer
        };
    }

    public async Task<ReclamosAnalyticsAdvancedDto> GetReclamosAdvancedAsync(
        int months,
        int? agenteId,
        string? aseguradora,
        CancellationToken cancellationToken = default)
    {
        if (months < 1)
        {
            months = 1;
        }

        if (months > 24)
        {
            months = 24;
        }

        var fromDate = DateTime.UtcNow.AddMonths(-months);
        var aseguradoraFilter = string.IsNullOrWhiteSpace(aseguradora)
            ? null
            : aseguradora.Trim().ToUpperInvariant();

        var polizas = await _db.Polizas
            .Where(p => !p.IsDeleted)
            .Select(p => new
            {
                p.Id,
                NumeroPoliza = p.NumeroPoliza ?? string.Empty,
                Aseguradora = p.Aseguradora ?? "OTROS",
                p.Prima,
                p.Frecuencia,
                p.Modalidad,
                p.EsActivo
            })
            .ToListAsync(cancellationToken);

        var usuarios = await _db.Users
            .Where(u => !u.IsDeleted)
            .Select(u => new { u.Id, Nombre = (u.FirstName + " " + u.LastName).Trim() })
            .ToListAsync(cancellationToken);

        var polizaById = polizas.ToDictionary(p => p.Id, p => p);
        var polizaByNumero = polizas
            .Where(p => !string.IsNullOrWhiteSpace(p.NumeroPoliza))
            .GroupBy(p => p.NumeroPoliza, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var baseReclamos = await _db.Reclamos
            .Where(r => !r.IsDeleted && r.FechaReclamo >= fromDate)
            .Select(r => new
            {
                r.Id,
                r.NumeroReclamo,
                r.NumeroPoliza,
                r.TipoReclamo,
                r.Prioridad,
                r.Estado,
                r.FechaReclamo,
                r.FechaResolucion,
                r.FechaLimiteRespuesta,
                r.MontoReclamado,
                r.MontoAprobado,
                r.UsuarioAsignadoId
            })
            .ToListAsync(cancellationToken);

        var reclamos = baseReclamos
            .Select(r =>
            {
                string aseguradoraReclamo = "OTROS";
                if (!string.IsNullOrWhiteSpace(r.NumeroPoliza) && polizaByNumero.TryGetValue(r.NumeroPoliza, out var polizaMatched))
                {
                    aseguradoraReclamo = polizaMatched.Aseguradora;
                }

                var agenteNombre = r.UsuarioAsignadoId.HasValue
                    ? usuarios.FirstOrDefault(u => u.Id == r.UsuarioAsignadoId.Value)?.Nombre ?? $"Agente {r.UsuarioAsignadoId.Value}"
                    : "Sin asignar";

                return new
                {
                    r.Id,
                    r.NumeroReclamo,
                    TipoReclamo = r.TipoReclamo.ToString(),
                    Prioridad = r.Prioridad.ToString(),
                    r.Estado,
                    r.FechaReclamo,
                    r.FechaResolucion,
                    r.FechaLimiteRespuesta,
                    r.MontoReclamado,
                    MontoAprobado = r.MontoAprobado ?? 0m,
                    r.UsuarioAsignadoId,
                    AgenteNombre = string.IsNullOrWhiteSpace(agenteNombre) ? "Sin asignar" : agenteNombre,
                    Aseguradora = aseguradoraReclamo
                };
            })
            .Where(r => !agenteId.HasValue || (r.UsuarioAsignadoId.HasValue && r.UsuarioAsignadoId.Value == agenteId.Value))
            .Where(r => aseguradoraFilter == null || string.Equals(r.Aseguradora.ToUpperInvariant(), aseguradoraFilter, StringComparison.Ordinal))
            .ToList();

        var primasActivasPorAseguradora = polizas
            .Where(p => p.EsActivo)
            .GroupBy(p => p.Aseguradora)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => NormalizePrimaMensual(x.Frecuencia, x.Prima)),
                StringComparer.OrdinalIgnoreCase);

        var lossRatioPorAseguradora = reclamos
            .GroupBy(r => r.Aseguradora)
            .Select(g =>
            {
                var prima = primasActivasPorAseguradora.TryGetValue(g.Key, out var p) ? p : 0m;
                var montoAprobado = g.Sum(x => x.MontoAprobado);
                return new LossRatioItemDto
                {
                    Segmento = g.Key,
                    MontoAprobado = Math.Round(montoAprobado, 2),
                    PrimaPortafolio = Math.Round(prima, 2),
                    LossRatio = prima > 0 ? Math.Round((double)(montoAprobado / prima * 100m), 1) : 0,
                    BenchmarkMin = InferBenchmarkMin(g.Key),
                    BenchmarkMax = InferBenchmarkMax(g.Key)
                };
            })
            .OrderByDescending(x => x.LossRatio)
            .ToList();

        var primasActivasPorTipo = polizas
            .Where(p => p.EsActivo)
            .GroupBy(p => NormalizeTipoSeguroFromPoliza(null, p.Modalidad))
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => NormalizePrimaMensual(x.Frecuencia, x.Prima)),
                StringComparer.OrdinalIgnoreCase);

        var lossRatioPorTipo = reclamos
            .GroupBy(r => r.TipoReclamo)
            .Select(g =>
            {
                var tipo = NormalizeTipoSeguroFromPoliza(g.Key, g.Key);
                var prima = primasActivasPorTipo.TryGetValue(tipo, out var p) ? p : 0m;
                var montoAprobado = g.Sum(x => x.MontoAprobado);
                return new LossRatioItemDto
                {
                    Segmento = g.Key,
                    MontoAprobado = Math.Round(montoAprobado, 2),
                    PrimaPortafolio = Math.Round(prima, 2),
                    LossRatio = prima > 0 ? Math.Round((double)(montoAprobado / prima * 100m), 1) : 0,
                    BenchmarkMin = InferBenchmarkMin(g.Key),
                    BenchmarkMax = InferBenchmarkMax(g.Key)
                };
            })
            .OrderByDescending(x => x.LossRatio)
            .ToList();

        var resolucionBase = reclamos
            .Where(r => r.FechaResolucion.HasValue)
            .Select(r => new
            {
                r.TipoReclamo,
                r.Prioridad,
                r.AgenteNombre,
                r.Aseguradora,
                Dias = (r.FechaResolucion!.Value - r.FechaReclamo).TotalDays
            })
            .Where(x => x.Dias >= 0)
            .ToList();

        static List<ResolucionTiempoStatsDto> ToResolucionStats<T>(IEnumerable<T> source, Func<T, string> selector, Func<T, double> daysSelector)
        {
            return source
                .GroupBy(selector)
                .Select(g => new ResolucionTiempoStatsDto
                {
                    Grupo = g.Key,
                    Total = g.Count(),
                    MinDias = Math.Round(g.Min(daysSelector), 1),
                    MaxDias = Math.Round(g.Max(daysSelector), 1),
                    PromedioDias = Math.Round(g.Average(daysSelector), 1)
                })
                .OrderByDescending(x => x.PromedioDias)
                .ToList();
        }

        var resolucionPorTipo = ToResolucionStats(resolucionBase, x => x.TipoReclamo, x => x.Dias);
        var resolucionPorPrioridad = ToResolucionStats(resolucionBase, x => x.Prioridad, x => x.Dias);
        var resolucionPorAgente = ToResolucionStats(resolucionBase, x => x.AgenteNombre, x => x.Dias);
        var resolucionPorAseguradora = ToResolucionStats(resolucionBase, x => x.Aseguradora, x => x.Dias);

        var heatmapMesTipo = reclamos
            .GroupBy(r => new { Mes = new DateTime(r.FechaReclamo.Year, r.FechaReclamo.Month, 1), r.TipoReclamo })
            .OrderBy(g => g.Key.Mes)
            .Select(g => new ReclamosHeatmapCellDto
            {
                Mes = g.Key.Mes.ToString("yyyy-MM"),
                TipoReclamo = g.Key.TipoReclamo,
                Cantidad = g.Count(),
                MontoReclamado = Math.Round(g.Sum(x => x.MontoReclamado), 2)
            })
            .ToList();

        var scatter = reclamos
            .Select(r => new ReclamoScatterPointDto
            {
                ReclamoId = r.Id,
                NumeroReclamo = r.NumeroReclamo,
                TipoReclamo = r.TipoReclamo,
                MontoReclamado = r.MontoReclamado,
                MontoAprobado = r.MontoAprobado,
                DuracionDias = r.FechaResolucion.HasValue
                    ? Math.Round(Math.Max(0, (r.FechaResolucion.Value - r.FechaReclamo).TotalDays), 1)
                    : 0
            })
            .OrderByDescending(x => x.MontoReclamado)
            .Take(400)
            .ToList();

        var now = DateTime.UtcNow;
        var rendimiento = reclamos
            .Where(r => r.UsuarioAsignadoId.HasValue)
            .GroupBy(r => new { r.UsuarioAsignadoId, r.AgenteNombre })
            .Select(g =>
            {
                var cerrados = g.Where(x => x.Estado == EstadoReclamo.Resuelto || x.Estado == EstadoReclamo.Cerrado).ToList();
                var conSla = g.Where(x => x.FechaResolucion.HasValue && x.FechaLimiteRespuesta.HasValue).ToList();
                var dentroSla = conSla.Count(x => x.FechaResolucion!.Value <= x.FechaLimiteRespuesta!.Value);
                var montoRec = g.Sum(x => x.MontoReclamado);
                var montoApr = g.Sum(x => x.MontoAprobado);

                var sparkline = Enumerable.Range(0, months)
                    .Select(i => new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1 - i)))
                    .Select(m => new ChartDataPointDto
                    {
                        Name = m.ToString("MMM", CultureInfo.InvariantCulture),
                        Value = cerrados.Count(c => c.FechaResolucion.HasValue && c.FechaResolucion.Value.Year == m.Year && c.FechaResolucion.Value.Month == m.Month)
                    })
                    .ToList();

                return new RendimientoAgenteDto
                {
                    AgenteId = g.Key.UsuarioAsignadoId!.Value,
                    Agente = g.Key.AgenteNombre,
                    ReclamosCerrados = cerrados.Count,
                    TiempoPromedioDias = cerrados.Count > 0
                        ? Math.Round(cerrados.Average(x => (x.FechaResolucion!.Value - x.FechaReclamo).TotalDays), 1)
                        : 0,
                    PorcentajeDentroSla = conSla.Count > 0 ? Math.Round((double)dentroSla / conSla.Count * 100, 1) : 100,
                    RatioAprobadoVsReclamado = montoRec > 0 ? Math.Round((double)(montoApr / montoRec * 100m), 1) : 0,
                    SparklineCierresMensuales = sparkline
                };
            })
            .OrderByDescending(x => x.ReclamosCerrados)
            .ToList();

        var aseguradorasDisponibles = polizas
            .Select(p => p.Aseguradora)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        var agentesDisponibles = usuarios
            .Select(u => new ReclamosAgenteFiltroDto { Id = u.Id, Nombre = u.Nombre })
            .OrderBy(u => u.Nombre)
            .ToList();

        return new ReclamosAnalyticsAdvancedDto
        {
            Months = months,
            AgenteId = agenteId,
            Aseguradora = aseguradora,
            AseguradorasDisponibles = aseguradorasDisponibles,
            AgentesDisponibles = agentesDisponibles,
            LossRatioPorAseguradora = lossRatioPorAseguradora,
            LossRatioPorTipo = lossRatioPorTipo,
            ResolucionPorTipo = resolucionPorTipo,
            ResolucionPorPrioridad = resolucionPorPrioridad,
            ResolucionPorAgente = resolucionPorAgente,
            ResolucionPorAseguradora = resolucionPorAseguradora,
            HeatmapMesTipo = heatmapMesTipo,
            MontoReclamadoVsAprobado = scatter,
            RendimientoAgentes = rendimiento
        };
    }

    private static decimal NormalizePrimaMensual(string? frecuencia, decimal prima)
    {
        return FrecuenciaMultiplier.TryGetValue(frecuencia ?? "MENSUAL", out var mult)
            ? prima * mult
            : prima;
    }

    private static string NormalizeTipoSeguroFromPoliza(string? tipoSeguro, string? modalidad)
    {
        var raw = (tipoSeguro ?? modalidad ?? string.Empty).Trim().ToUpperInvariant();
        if (raw.Contains("AUTO") || raw.Contains("VEHIC")) return "AUTO";
        if (raw.Contains("VIDA") || raw.Contains("SALUD")) return "VIDA";
        if (raw.Contains("HOGAR") || raw.Contains("CASA") || raw.Contains("VIVIEN")) return "HOGAR";
        if (raw.Contains("EMPRESA") || raw.Contains("COMERCIAL") || raw.Contains("PYME")) return "EMPRESARIAL";
        return string.IsNullOrWhiteSpace(raw) ? "OTROS" : raw;
    }

    private static double InferBenchmarkMin(string segment)
    {
        var raw = segment.ToUpperInvariant();
        if (raw.Contains("VIDA")) return 60;
        if (raw.Contains("AUTO")) return 40;
        return 45;
    }

    private static double InferBenchmarkMax(string segment)
    {
        var raw = segment.ToUpperInvariant();
        if (raw.Contains("VIDA")) return 70;
        if (raw.Contains("AUTO")) return 50;
        return 65;
    }
}
