using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    // ── MÓDULO 0 — Dashboard Ejecutivo ───────────────────────────────────────
    public class ExecutiveDashboardDto
    {
        public int CarteraActiva { get; set; }
        public decimal PrimaMensualBruta { get; set; }
        public decimal TasaCobro { get; set; }
        public decimal MontoEnRiesgo { get; set; }
        public int ReclamosActivos { get; set; }
        public decimal TasaConversion { get; set; }
        public decimal TasaEmailExito { get; set; }
        public decimal SlaReclamos { get; set; }

        // Trends (comparativa vs mes anterior, porcentaje de cambio)
        public decimal CarteraActivaTrend { get; set; }
        public decimal PrimaMensualTrend { get; set; }
        public decimal TasaCobroTrend { get; set; }
        public decimal MontoEnRiesgoTrend { get; set; }
        public decimal ReclamosActivosTrend { get; set; }
        public decimal TasaConversionTrend { get; set; }

        // KPIs adicionales — alto impacto operacional y financiero
        public int     PolizasPorRenovar30d          { get; set; }
        public int     ReclamosSinAsignar            { get; set; }
        public decimal TiempoPromedioResolucionDias  { get; set; }
        public decimal TasaMorosidad                { get; set; }
        public decimal TasaMorosidadTrend           { get; set; }

        // Segunda tanda de KPIs
        public decimal RatioPerdidas                { get; set; }  // montoAprobado/montoReclamado %
        public decimal TicketPromedioCobro          { get; set; }  // promedio MontoTotal por cobro
        public decimal TicketPromedioCobroTrend     { get; set; }  // vs mes anterior
        public decimal TasaReclamos                 { get; set; }  // reclamosActivos / carteraActiva %
        public int     PolizasNuevasMes             { get; set; }  // pólizas creadas este mes
        public int     PolizasNuevasMesTrend        { get; set; }  // delta vs mes anterior (unidades)

        // Tercera tanda de KPIs
        public decimal PrimaPromedioPorPoliza       { get; set; }  // primaMensualBruta / carteraActiva
        public int     CobrosCanceladosMes          { get; set; }  // cobros cancelados este mes
        public decimal PorcentajeCobroUsd           { get; set; }  // % cobros históricos en USD
        public int     ReclamosFueraDeSla           { get; set; }  // activos con FechaLimite < now
        public decimal ValorPipelineCotizaciones    { get; set; }  // Sum(Prima) WHERE estado=PENDIENTE
        public int     CotizacionesEnPipeline       { get; set; }  // Count WHERE estado=PENDIENTE
        public decimal TiempoPromedioConversion     { get; set; }  // días promedio de cotización a póliza
        public int     PolizasInactivasMes          { get; set; }  // desactivadas este mes (churn)

        public IEnumerable<AlertaDto> Alertas { get; set; } = new List<AlertaDto>();
        // Chart data
        public IEnumerable<MultiSeriesChartDto>  CobrosMensuales          { get; set; } = new List<MultiSeriesChartDto>();
        public IEnumerable<MultiSeriesChartDto>  CobrosEstadoMensual      { get; set; } = new List<MultiSeriesChartDto>();
        public IEnumerable<ChartSeriesDto>       DistribucionAseguradoras { get; set; } = new List<ChartSeriesDto>();
        // Sparklines — últimos 6 meses
        public IEnumerable<ChartDataPointDto>    SparklineTasaCobro       { get; set; } = new List<ChartDataPointDto>();
        public IEnumerable<ChartDataPointDto>    SparklineMontoRiesgo     { get; set; } = new List<ChartDataPointDto>();
    }

    public class AlertaDto
    {
        public string Tipo { get; set; } = string.Empty;      // CRITICO/ALERTA/AVISO/INFO
        public string Icono { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string? Accion { get; set; }
        public string? RutaAccion { get; set; }
    }

    // ── MÓDULO 1 — Cobros Analytics ──────────────────────────────────────────
    public class CobrosTrendDto
    {
        public IEnumerable<CobroMensualDto> Mensual { get; set; } = new List<CobroMensualDto>();
        public decimal PromedioMensualEsperado { get; set; }
        public decimal PromedioMensualCobrado { get; set; }
    }

    public class CobroMensualDto
    {
        public string Mes { get; set; } = string.Empty;        // "2025-01"
        public string MesLabel { get; set; } = string.Empty;   // "Ene 2025"
        public decimal MontoEsperado { get; set; }
        public decimal MontoCobrado { get; set; }
        public decimal MontoVencido { get; set; }
        public int CantidadCobros { get; set; }
    }

    public class AgingReportDto
    {
        public IEnumerable<AgingBucketDto> Buckets { get; set; } = new List<AgingBucketDto>();
        public decimal TotalVencido { get; set; }
        public int TotalCobrosVencidos { get; set; }
    }

    public class AgingBucketDto
    {
        public string Rango { get; set; } = string.Empty;      // "1-15 días"
        public int DiasMin { get; set; }
        public int DiasMax { get; set; }                        // -1 = sin límite
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
        public decimal PorcentajeMonto { get; set; }
        public string Color { get; set; } = string.Empty;      // CSS variable name
    }

    public class PagoMetodoDistribucionDto
    {
        public string MetodoPago { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class CashflowForecastDto
    {
        public IEnumerable<CashflowSemanaDto> Semanas { get; set; } = new List<CashflowSemanaDto>();
        public decimal TotalProjectado { get; set; }
        public decimal TotalCobrado { get; set; }
        public decimal TasaHistorica { get; set; }
    }

    public class CashflowSemanaDto
    {
        public string Semana { get; set; } = string.Empty;   // "Sem 1"
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal MontoEsperado { get; set; }
        public decimal MontoCobrado { get; set; }
        public bool EsFuturo { get; set; }
    }

    public class HeatmapCellDto
    {
        public int DiaSemana { get; set; }   // 0=Lun, 6=Dom
        public int Hora { get; set; }
        public int Valor { get; set; }
    }

    public class TopDeudorDto
    {
        public string ClienteNombre { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public int NumeroPolizas { get; set; }
        public decimal MontoVencidoTotal { get; set; }
        public int AntiguedadMaxDias { get; set; }
        public string Moneda { get; set; } = "CRC";
        public int CobroId { get; set; }
    }

    public class AgenteCobrosDto
    {
        public string NombreAgente { get; set; } = string.Empty;
        public int CobrosProcesados { get; set; }
        public decimal MontoCobrado { get; set; }
        public double TiempoPromedioResDias { get; set; }
    }

    // ── MÓDULO 2 — Portfolio de Pólizas ──────────────────────────────────────
    public class PortfolioDistribucionDto
    {
        public IEnumerable<ChartSeriesDto> PorAseguradora { get; set; } = new List<ChartSeriesDto>();
        public IEnumerable<ChartSeriesDto> PorTipoSeguro { get; set; } = new List<ChartSeriesDto>();
        public IEnumerable<ChartSeriesDto> PorModalidad { get; set; } = new List<ChartSeriesDto>();
        public IEnumerable<ChartSeriesDto> PorFrecuencia { get; set; } = new List<ChartSeriesDto>();
        public IEnumerable<PortfolioBreakdownDto> ComposicionPorAseguradora { get; set; } = new List<PortfolioBreakdownDto>();
        public IEnumerable<PortfolioBreakdownDto> ComposicionPorTipoSeguro { get; set; } = new List<PortfolioBreakdownDto>();
        public IEnumerable<PortfolioBreakdownDto> ComposicionPorModalidad { get; set; } = new List<PortfolioBreakdownDto>();
        public IEnumerable<PortfolioBreakdownDto> ComposicionPorFrecuencia { get; set; } = new List<PortfolioBreakdownDto>();
        public IEnumerable<RadarDataDto> RadarAseguradoras { get; set; } = new List<RadarDataDto>();
        public RetencionRenovacionDto Retencion { get; set; } = new RetencionRenovacionDto();
        public IEnumerable<HistogramaPrimaBucketDto> HistogramaPrima { get; set; } = new List<HistogramaPrimaBucketDto>();
        public IEnumerable<RiesgoConcentracionDto> MapaRiesgo { get; set; } = new List<RiesgoConcentracionDto>();
        public decimal PrimaMensualTotal { get; set; }
        public int TotalPolizasActivas { get; set; }
    }

    public class PortfolioBreakdownDto
    {
        public string Name { get; set; } = string.Empty;
        public int TotalPolizas { get; set; }
        public decimal PrimaMensual { get; set; }
        public double PorcentajePolizas { get; set; }
        public double PorcentajePrima { get; set; }
    }

    public class RadarDataDto
    {
        public string Aseguradora { get; set; } = string.Empty;
        public decimal PrimaPromedio { get; set; }
        public int NumPolizas { get; set; }
        public decimal TasaReclamos { get; set; }
        public decimal TasaCobro { get; set; }
        public double AntiguedadPromedioDias { get; set; }
    }

    public class VencimientosTimelineDto
    {
        public IEnumerable<VencimientoMesDto> Meses { get; set; } = new List<VencimientoMesDto>();
        public int TotalPolizasProximasAnio { get; set; }
        public decimal PrimaEnRiesgoAnio { get; set; }
    }

    public class VencimientoMesDto
    {
        public string Mes { get; set; } = string.Empty;
        public string MesLabel { get; set; } = string.Empty;
        public int TotalPolizas { get; set; }
        public int Auto { get; set; }
        public int Vida { get; set; }
        public int Hogar { get; set; }
        public int Empresarial { get; set; }
        public decimal PrimaEnRiesgo { get; set; }
    }

    public class VencimientoDetalleMesDto
    {
        public string Mes { get; set; } = string.Empty;
        public string MesLabel { get; set; } = string.Empty;
        public IEnumerable<VencimientoPolizaDetalleDto> Polizas { get; set; } = new List<VencimientoPolizaDetalleDto>();
    }

    public class VencimientoPolizaDetalleDto
    {
        public string NumeroPoliza { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string Aseguradora { get; set; } = string.Empty;
        public string TipoSeguro { get; set; } = string.Empty;
        public DateTime FechaVigencia { get; set; }
        public decimal PrimaMensualNormalizada { get; set; }
    }

    public class RetencionRenovacionDto
    {
        public int TotalVencidas { get; set; }
        public int TotalRenovadas { get; set; }
        public int TotalNoRenovadas { get; set; }
        public double TasaRetencionPromedio { get; set; }
        public IEnumerable<ChartSeriesDto> Funnel { get; set; } = new List<ChartSeriesDto>();
        public IEnumerable<ChartDataPointDto> TasaRetencionMensual { get; set; } = new List<ChartDataPointDto>();
        public IEnumerable<ChartSeriesDto> RetencionPorTipoSeguro { get; set; } = new List<ChartSeriesDto>();
        public IEnumerable<ChartSeriesDto> ChurnPorAseguradora { get; set; } = new List<ChartSeriesDto>();
    }

    public class HistogramaPrimaBucketDto
    {
        public string Rango { get; set; } = string.Empty;
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public int Cantidad { get; set; }
        public decimal PrimaMensualTotal { get; set; }
    }

    public class RiesgoConcentracionDto
    {
        public string Segmento { get; set; } = string.Empty;
        public string Aseguradora { get; set; } = string.Empty;
        public string TipoSeguro { get; set; } = string.Empty;
        public decimal PrimaUnitariaPromedio { get; set; }
        public decimal MontoAseguradoProxy { get; set; }
        public int NumeroClientes { get; set; }
    }

    // ── MÓDULO 3 — Reclamos Analytics ────────────────────────────────────────
    public class ReclamosFunnelDto
    {
        public IEnumerable<FunnelEtapaDto> Etapas { get; set; } = new List<FunnelEtapaDto>();
        public decimal MontoTotalReclamado { get; set; }
        public decimal MontoTotalAprobado { get; set; }
        public decimal LossRatioGlobal { get; set; }
    }

    public class FunnelEtapaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
        public double TiempoPromedioHoras { get; set; }
        public double PorcentajeDel100 { get; set; }
    }

    public class SlaReportDto
    {
        public double PorcentajeGlobalDentroSLA { get; set; }
        public IEnumerable<SlaPorTipoDto> PorTipo { get; set; } = new List<SlaPorTipoDto>();
        public IEnumerable<SlaPorTipoDto> PorPrioridad { get; set; } = new List<SlaPorTipoDto>();
        public IEnumerable<SlaPorAgenteDto> PorAgente { get; set; } = new List<SlaPorAgenteDto>();
        public IEnumerable<ReclamoAlertoSlaDto> ProximosVencer { get; set; } = new List<ReclamoAlertoSlaDto>();
    }

    public class SlaPorTipoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int TotalReclamos { get; set; }
        public int DentroSla { get; set; }
        public int FueraSla { get; set; }
        public double PorcentajeDentroSla { get; set; }
        public double TiempoPromedioHoras { get; set; }
    }

    public class SlaPorAgenteDto
    {
        public string NombreAgente { get; set; } = string.Empty;
        public int TotalAsignados { get; set; }
        public int DentroSla { get; set; }
        public double PorcentajeDentroSla { get; set; }
        public double TiempoPromedioHoras { get; set; }
        public decimal MontoAprobado { get; set; }
    }

    public class ReclamoAlertoSlaDto
    {
        public int Id { get; set; }
        public string NumeroReclamo { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaLimite { get; set; }
        public int HorasRestantes { get; set; }
        public string Prioridad { get; set; } = string.Empty;
    }

    public class ReclamosAnalyticsAdvancedDto
    {
        public int Months { get; set; }
        public int? AgenteId { get; set; }
        public string? Aseguradora { get; set; }

        public IEnumerable<string> AseguradorasDisponibles { get; set; } = new List<string>();
        public IEnumerable<ReclamosAgenteFiltroDto> AgentesDisponibles { get; set; } = new List<ReclamosAgenteFiltroDto>();

        public IEnumerable<LossRatioItemDto> LossRatioPorAseguradora { get; set; } = new List<LossRatioItemDto>();
        public IEnumerable<LossRatioItemDto> LossRatioPorTipo { get; set; } = new List<LossRatioItemDto>();

        public IEnumerable<ResolucionTiempoStatsDto> ResolucionPorTipo { get; set; } = new List<ResolucionTiempoStatsDto>();
        public IEnumerable<ResolucionTiempoStatsDto> ResolucionPorPrioridad { get; set; } = new List<ResolucionTiempoStatsDto>();
        public IEnumerable<ResolucionTiempoStatsDto> ResolucionPorAgente { get; set; } = new List<ResolucionTiempoStatsDto>();
        public IEnumerable<ResolucionTiempoStatsDto> ResolucionPorAseguradora { get; set; } = new List<ResolucionTiempoStatsDto>();

        public IEnumerable<ReclamosHeatmapCellDto> HeatmapMesTipo { get; set; } = new List<ReclamosHeatmapCellDto>();
        public IEnumerable<ReclamoScatterPointDto> MontoReclamadoVsAprobado { get; set; } = new List<ReclamoScatterPointDto>();

        public IEnumerable<RendimientoAgenteDto> RendimientoAgentes { get; set; } = new List<RendimientoAgenteDto>();
    }

    public class ReclamosAgenteFiltroDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class LossRatioItemDto
    {
        public string Segmento { get; set; } = string.Empty;
        public decimal MontoAprobado { get; set; }
        public decimal PrimaPortafolio { get; set; }
        public double LossRatio { get; set; }
        public double BenchmarkMin { get; set; }
        public double BenchmarkMax { get; set; }
    }

    public class ResolucionTiempoStatsDto
    {
        public string Grupo { get; set; } = string.Empty;
        public int Total { get; set; }
        public double MinDias { get; set; }
        public double MaxDias { get; set; }
        public double PromedioDias { get; set; }
    }

    public class ReclamosHeatmapCellDto
    {
        public string Mes { get; set; } = string.Empty;
        public string TipoReclamo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal MontoReclamado { get; set; }
    }

    public class ReclamoScatterPointDto
    {
        public int ReclamoId { get; set; }
        public string NumeroReclamo { get; set; } = string.Empty;
        public string TipoReclamo { get; set; } = string.Empty;
        public decimal MontoReclamado { get; set; }
        public decimal MontoAprobado { get; set; }
        public double DuracionDias { get; set; }
    }

    public class RendimientoAgenteDto
    {
        public int AgenteId { get; set; }
        public string Agente { get; set; } = string.Empty;
        public int ReclamosCerrados { get; set; }
        public double TiempoPromedioDias { get; set; }
        public double PorcentajeDentroSla { get; set; }
        public double RatioAprobadoVsReclamado { get; set; }
        public IEnumerable<ChartDataPointDto> SparklineCierresMensuales { get; set; } = new List<ChartDataPointDto>();
    }

    public class TiempoResolucionDto
    {
        public IEnumerable<TiempoSegmentadoDto> PorTipo { get; set; } = new List<TiempoSegmentadoDto>();
        public IEnumerable<TiempoSegmentadoDto> PorPrioridad { get; set; } = new List<TiempoSegmentadoDto>();
        public double PromedioGlobalDias { get; set; }
    }

    public class TiempoSegmentadoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public double PromediosDias { get; set; }
        public double MinDias { get; set; }
        public double MaxDias { get; set; }
        public int Cantidad { get; set; }
    }

    // ── MÓDULO 4 — Cotizaciones / Sales Funnel ────────────────────────────────
    public class CotizacionesFunnelDto
    {
        public int Total { get; set; }
        public int Pendientes { get; set; }
        public int Aprobadas { get; set; }
        public int Convertidas { get; set; }
        public int Rechazadas { get; set; }
        public double TasaAprobacion { get; set; }
        public double TasaConversion { get; set; }
        public double VelocidadPromedioDias { get; set; }
        public IEnumerable<ChartSeriesDto> PorTipoSeguro { get; set; } = new List<ChartSeriesDto>();
        public IEnumerable<ChartSeriesDto> PorAseguradora { get; set; } = new List<ChartSeriesDto>();
        public IEnumerable<VelocidadBucketDto> VelocidadBuckets { get; set; } = new List<VelocidadBucketDto>();
        public IEnumerable<TicketPromedioDto> TicketPromedio { get; set; } = new List<TicketPromedioDto>();
        public IEnumerable<PipelineValorDto> PipelineValor { get; set; } = new List<PipelineValorDto>();
    }

    public class VelocidadBucketDto
    {
        public string Rango { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class TicketPromedioDto
    {
        public string TipoSeguro { get; set; } = string.Empty;
        public string Modalidad { get; set; } = string.Empty;
        public decimal PrimaPromedio { get; set; }
        public int Cantidad { get; set; }
    }

    public class PipelineValorDto
    {
        public string Mes { get; set; } = string.Empty;
        public decimal ValorPendiente { get; set; }
        public decimal ValorAprobado { get; set; }
        public decimal ValorConvertido { get; set; }
        public decimal ValorAnualProyectado { get; set; }
    }

    // ── MÓDULO 5 — Email Analytics ────────────────────────────────────────────
    public class EmailAnalyticsDto
    {
        public IEnumerable<EmailTipoStatsDto> PorTipo { get; set; } = new List<EmailTipoStatsDto>();
        public IEnumerable<EmailVolumenDiaDto> VolumenPorDia { get; set; } = new List<EmailVolumenDiaDto>();
        public EmailCoberturaCobrosDto CoberturaCobros { get; set; } = new EmailCoberturaCobrosDto();
        public IEnumerable<HeatmapCellDto> HeatmapEnvios { get; set; } = new List<HeatmapCellDto>();
        public EmailCorrelacionCanalDto CorrelacionCanal { get; set; } = new EmailCorrelacionCanalDto();
        public double TasaExitoGlobal { get; set; }
        public int TotalEnviadosPeriodo { get; set; }
        public int TotalFallidosPeriodo { get; set; }
    }

    public class EmailTipoStatsDto
    {
        public string Tipo { get; set; } = string.Empty;
        public int Enviados { get; set; }
        public int Exitosos { get; set; }
        public int Fallidos { get; set; }
        public double TasaExito { get; set; }
    }

    public class EmailVolumenDiaDto
    {
        public DateTime Fecha { get; set; }
        public string FechaLabel { get; set; } = string.Empty;
        public int TotalEnviados { get; set; }
        public int Exitosos { get; set; }
        public int Fallidos { get; set; }
    }

    public class EmailCoberturaCobrosDto
    {
        public int CobrosVencidosTotal { get; set; }
        public int ConEmailNotificado { get; set; }
        public int ConEmailNoNotificado { get; set; }
        public int SinEmail { get; set; }
        public double PorcentajeCoberturaNotificada { get; set; }
    }

    /// <summary>5.4 — Correlación Email → Cobro: ROI del canal</summary>
    public class EmailCorrelacionCanalDto
    {
        public int TotalCobrosVencidosConEmail { get; set; }
        public int TotalCobrosVencidosSinEmail { get; set; }
        public double TasaPagoConEmail { get; set; }
        public double TasaPagoSinEmail { get; set; }
        public IEnumerable<CorrelacionBucketDto> BucketsConEmail { get; set; } = new List<CorrelacionBucketDto>();
    }

    public class CorrelacionBucketDto
    {
        public string Etiqueta { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double Porcentaje { get; set; }
    }

    // ── MÓDULO 6 — Operacional ────────────────────────────────────────────────
    public class OperacionalStatsDto
    {
        public IEnumerable<HeatmapAgenteRowDto> ActividadHeatmapPorAgente { get; set; } = new List<HeatmapAgenteRowDto>();
        public IEnumerable<AgenteCargaDto> CargaReclamos { get; set; } = new List<AgenteCargaDto>();
        public bool RiesgoConcentracionCritica { get; set; }
        public string? AgenteConcentracionCritica { get; set; }
        public IEnumerable<UsuarioActividadDto> UltimoAcceso { get; set; } = new List<UsuarioActividadDto>();
        public ChatIAStatsDto ChatIA { get; set; } = new ChatIAStatsDto();
        public SistemaRendimientoDto SistemaRendimiento { get; set; } = new SistemaRendimientoDto();
        public IEnumerable<string> Alertas { get; set; } = new List<string>();
    }

    public class HeatmapAgenteRowDto
    {
        public string NombreAgente { get; set; } = string.Empty;
        public IEnumerable<HeatmapCellSemanaDto> Semanas { get; set; } = new List<HeatmapCellSemanaDto>();
    }

    public class HeatmapCellSemanaDto
    {
        public string Semana { get; set; } = string.Empty;
        public int Acciones { get; set; }
    }

    public class AgenteCargaDto
    {
        public string NombreAgente { get; set; } = string.Empty;
        public int ReclamosActivos { get; set; }
        public int ReclamosAltaPrioridad { get; set; }
        public int ReclamosCriticos { get; set; }
        public double PorcentajeSla { get; set; }
        public bool TieneCriticosExclusivos { get; set; }
        public int TotalAccionesUltimas12Semanas { get; set; }
    }

    public class UsuarioActividadDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime? UltimoLogin { get; set; }
        public int DiasSinAcceso { get; set; }
        public int AccionesAyer { get; set; }
        public bool EsActivo { get; set; }
    }

    public class ChatIAStatsDto
    {
        public int TotalSesiones { get; set; }
        public int SesionesActivas { get; set; }
        public int SesionesCerradas { get; set; }
        public double MensajesPorSesionPromedio { get; set; }
        public double TiempoProcesPromedioMs { get; set; }
        public int MensajesPositivos { get; set; }
        public int MensajesNegativos { get; set; }
        public double TasaSatisfaccion { get; set; }
        public IEnumerable<ChatSesionSemanalDto> SesionesPorSemana { get; set; } = new List<ChatSesionSemanalDto>();
        public IEnumerable<TopPalabraDto> TopPalabras { get; set; } = new List<TopPalabraDto>();
    }

    // ── MÓDULO 7 — Predictivo (ML Ligero) ────────────────────────────────────

    /// <summary>Top-level M7 response DTO</summary>
    public class PredictivoAnalyticsDto
    {
        public PredictivoScoreMorosidadDto  ScoreMorosidad    { get; set; } = new PredictivoScoreMorosidadDto();
        public PredictivoReclamosDto        ReclamosTemporada { get; set; } = new PredictivoReclamosDto();
        public PredictivoAnomaliasCobrosDto AnomaliasCobros   { get; set; } = new PredictivoAnomaliasCobrosDto();
        public PredictivoRenovacionDto      Renovacion        { get; set; } = new PredictivoRenovacionDto();
        public PredictivoForecastPrimaDto   ForecastPrima     { get; set; } = new PredictivoForecastPrimaDto();
    }

    /// <summary>7.1 — Score de Riesgo de Morosidad</summary>
    public class PredictivoScoreMorosidadDto
    {
        public IEnumerable<PolizaRiesgoDto>   Polizas            { get; set; } = new List<PolizaRiesgoDto>();
        public IEnumerable<ChartDataPointDto> DistribucionRiesgo { get; set; } = new List<ChartDataPointDto>();
    }

    public class PolizaRiesgoDto
    {
        public string NumeroPoliza    { get; set; } = string.Empty;
        public string NombreAsegurado { get; set; } = string.Empty;
        public string Aseguradora     { get; set; } = string.Empty;
        public string TipoSeguro      { get; set; } = string.Empty;
        public string Frecuencia      { get; set; } = string.Empty;
        public int    Score           { get; set; }
        public string Categoria       { get; set; } = string.Empty;
        public double TasaVencido     { get; set; }
        public int    TotalCobros     { get; set; }
        public int    CobrosVencidos  { get; set; }
    }

    /// <summary>7.2 — Predicción de Reclamos por Temporada</summary>
    public class PredictivoReclamosDto
    {
        public IEnumerable<PrediccionMesDto>   ProximosMeses  { get; set; } = new List<PrediccionMesDto>();
        public IEnumerable<MultiSeriesChartDto> HistoricoSerie { get; set; } = new List<MultiSeriesChartDto>();
    }

    public class PrediccionMesDto
    {
        public string MesLabel    { get; set; } = string.Empty;
        public int    Esperado    { get; set; }
        public int    AnioAnterior { get; set; }
        public double CambioPorc  { get; set; }
    }

    /// <summary>7.3 — Detección de Anomalías en Cobros (Z-score)</summary>
    public class PredictivoAnomaliasCobrosDto
    {
        public IEnumerable<AnomaliaCobroDto> Anomalias       { get; set; } = new List<AnomaliaCobroDto>();
        public double MediaGlobal      { get; set; }
        public double DesviacionGlobal { get; set; }
        public int    TotalAnalizados  { get; set; }
    }

    public class AnomaliaCobroDto
    {
        public int     Id            { get; set; }
        public string  NumeroRecibo  { get; set; } = string.Empty;
        public string  ClienteNombre { get; set; } = string.Empty;
        public string  NumeroPoliza  { get; set; } = string.Empty;
        public decimal MontoTotal    { get; set; }
        public double  ZScore        { get; set; }
        public double  MediaCliente  { get; set; }
        public string  TipoAnomalia  { get; set; } = string.Empty;
        public string  EstadoActual  { get; set; } = string.Empty;
    }

    /// <summary>7.4 — Renovación Proactiva (Lead Scoring)</summary>
    public class PredictivoRenovacionDto
    {
        public IEnumerable<PolizaLeadDto>     TopPolizas                { get; set; } = new List<PolizaLeadDto>();
        public int    TotalPolizasPorRenovar60d { get; set; }
        public double ScorePromedio              { get; set; }
        public IEnumerable<ChartDataPointDto>  DistribucionScore        { get; set; } = new List<ChartDataPointDto>();
    }

    public class PolizaLeadDto
    {
        public string   NumeroPoliza    { get; set; } = string.Empty;
        public string   NombreAsegurado { get; set; } = string.Empty;
        public string   Correo          { get; set; } = string.Empty;
        public string   Telefono        { get; set; } = string.Empty;
        public string   Aseguradora     { get; set; } = string.Empty;
        public string   TipoSeguro      { get; set; } = string.Empty;
        public decimal  PrimaMensual    { get; set; }
        public DateTime FechaVigencia   { get; set; }
        public int      DiasParaVencer  { get; set; }
        public int      Score           { get; set; }
        public string[] Factores        { get; set; } = Array.Empty<string>();
    }

    /// <summary>7.5 — Forecast de Prima Mensual Proyectada</summary>
    public class PredictivoForecastPrimaDto
    {
        public decimal CobrosProgramadosProximoMes { get; set; }
        public double  TasaHistoricaCobro6m        { get; set; }
        public decimal PrimaProyectada             { get; set; }
        public decimal IcInferior                  { get; set; }
        public decimal IcSuperior                  { get; set; }
        public IEnumerable<ForecastMesDto> ProyeccionMensual { get; set; } = new List<ForecastMesDto>();
    }

    public class ForecastMesDto
    {
        public string  MesLabel        { get; set; } = string.Empty;
        public decimal MontoEsperado   { get; set; }
        public decimal MontoProyectado { get; set; }
        public bool    EsFuturo        { get; set; }
    }

    public class ChatSesionSemanalDto
    {
        public string Semana { get; set; } = string.Empty;
        public int Activas { get; set; }
        public int Cerradas { get; set; }
    }

    public class TopPalabraDto
    {
        public string Palabra { get; set; } = string.Empty;
        public int Frecuencia { get; set; }
    }

    public class SistemaRendimientoDto
    {
        public bool Disponible { get; set; }
        public IEnumerable<EndpointPercentilDto> Endpoints { get; set; } = new List<EndpointPercentilDto>();
    }

    public class EndpointPercentilDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public double P50Ms { get; set; }
        public double P90Ms { get; set; }
        public double P99Ms { get; set; }
        public int Samples { get; set; }
    }

    // ── MÓDULO 8 — Reportes Exportables ─────────────────────────────────────
    public class ReportFileDto
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }

    public class CarteraAseguradoraRowDto
    {
        public string Aseguradora { get; set; } = string.Empty;
        public string NumeroPoliza { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public decimal PrimaMensual { get; set; }
        public decimal MontoEsperado { get; set; }
        public decimal MontoCobrado { get; set; }
        public decimal TasaCobroPct { get; set; }
        public int ReclamosPendientes { get; set; }
        public string EstadoCobros { get; set; } = string.Empty;
    }

    public class MorosidadReportRowDto
    {
        public string NumeroPoliza { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int DiasMora { get; set; }
        public decimal MontoVencido { get; set; }
        public decimal HistorialCobrado { get; set; }
        public int HistorialCobros { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }

    public class ReportEmailRequestDto
    {
        public IEnumerable<string> Recipients { get; set; } = Array.Empty<string>();
    }

    public class ReportDispatchResultDto
    {
        public int EmailsEnviados { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // ── MÓDULO 9 — Cliente 360° ───────────────────────────────────────────────
    public class Cliente360Dto
    {
        public string  NombreCompleto      { get; set; } = string.Empty;
        public string  Cedula              { get; set; } = string.Empty;
        public string  Email               { get; set; } = string.Empty;
        public string  Telefono            { get; set; } = string.Empty;
        public decimal LTV                 { get; set; }
        public decimal PrimaMensualActiva  { get; set; }
        public decimal TotalReclamado      { get; set; }
        public decimal TotalCobrado        { get; set; }
        public int     MesesComoCliente    { get; set; }
        public int     ScoreLealtad        { get; set; }
        public int     ScoreRiesgo         { get; set; }
        public string  CategoriaRiesgo     { get; set; } = string.Empty;
        public List<Cliente360PolizaDto>  Polizas     { get; set; } = new();
        public List<Cliente360CobroDto>   Cobros      { get; set; } = new();
        public List<Cliente360ReclamoDto> Reclamos    { get; set; } = new();
        public List<ChartDataPointDto>    LtvTimeline { get; set; } = new();
    }

    public class Cliente360PolizaDto
    {
        public string   NumeroPoliza   { get; set; } = string.Empty;
        public string   TipoSeguro     { get; set; } = string.Empty;
        public string   Aseguradora    { get; set; } = string.Empty;
        public decimal  Prima          { get; set; }
        public string   Frecuencia     { get; set; } = string.Empty;
        public DateTime FechaVigencia  { get; set; }
        public bool     EsActiva       { get; set; }
        public int      DiasParaVencer { get; set; }
    }

    public class Cliente360CobroDto
    {
        public string   NumeroRecibo     { get; set; } = string.Empty;
        public string   Estado           { get; set; } = string.Empty;
        public decimal  MontoTotal       { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int      DiasVencido      { get; set; }
    }

    public class Cliente360ReclamoDto
    {
        public string  NumeroReclamo  { get; set; } = string.Empty;
        public string  TipoReclamo    { get; set; } = string.Empty;
        public string  Estado         { get; set; } = string.Empty;
        public string  Prioridad      { get; set; } = string.Empty;
        public decimal MontoClamado   { get; set; }
        public decimal MontoAprobado  { get; set; }
        public int     DiasResolucion { get; set; }
    }

    public class Cliente360SearchResultDto
    {
        public string  Cedula          { get; set; } = string.Empty;
        public string  NombreCompleto  { get; set; } = string.Empty;
        public string  Email           { get; set; } = string.Empty;
        public int     NumeroPolizas   { get; set; }
        public decimal PrimaMensual    { get; set; }
    }

    // ── MÓDULO 14 — Agenda Inteligente ────────────────────────────────────────
    public class AgendaDto
    {
        public DateTime          FechaGeneracion { get; set; }
        public int               TotalAcciones   { get; set; }
        public List<AgendaSeccionDto> Secciones  { get; set; } = new();
    }

    public class AgendaSeccionDto
    {
        public string               Titulo { get; set; } = string.Empty;
        public string               Nivel  { get; set; } = string.Empty; // critico/alerta/aviso/info
        public string               Icono  { get; set; } = string.Empty; // material icon name
        public int                  Total  { get; set; }
        public List<AgendaItemDto>  Items  { get; set; } = new();
    }

    public class AgendaItemDto
    {
        public string   Tipo        { get; set; } = string.Empty; // cobro/poliza/cotizacion/lead
        public string   Titulo      { get; set; } = string.Empty;
        public string   Descripcion { get; set; } = string.Empty;
        public string   Nivel       { get; set; } = string.Empty; // critico/alerta/aviso/info
        public string?  Cedula      { get; set; }
        public string?  NumeroRef   { get; set; }
        public string?  NavLink     { get; set; }
        public decimal? Monto       { get; set; }
        public int?     DiasVencido { get; set; }
        public int?     Score       { get; set; }
    }

    // ── Shared primitives ─────────────────────────────────────────────────────
    public class ChartDataPointDto
    {
        public string Name  { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Extra { get; set; }
    }

    public class ChartSeriesDto
    {
        public string Name  { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    // Multi-series container for line / area / stacked-bar charts
    public class MultiSeriesChartDto
    {
        public string Name { get; set; } = string.Empty;
        public IEnumerable<ChartDataPointDto> Series { get; set; } = new List<ChartDataPointDto>();
    }
}
