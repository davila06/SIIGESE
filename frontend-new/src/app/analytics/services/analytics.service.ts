import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// ── DTOs mirroring AnalyticsDtos.cs ──────────────────────────────────────────

export interface ChartDataPoint  { name: string; value: number; }
export interface ChartSeries     { name: string; value: number; extra?: any; }
export interface MultiSeriesChart { name: string; series: ChartDataPoint[]; }
export interface HeatmapCell     { diaSemana: number; hora: number; valor: number; }

export interface AlertaDto {
  tipo: string;       // CRITICO | ALERTA | AVISO | INFO
  icono: string;
  mensaje: string;
  cantidad: number;
  rutaAccion: string;
}

export interface ExecutiveDashboardDto {
  carteraActiva: number;
  primaMensualBruta: number;
  tasaCobro: number;
  montoEnRiesgo: number;
  reclamosActivos: number;
  tasaConversion: number;
  tasaEmailExito: number;
  slaReclamos: number;
  tasaCobroTrend: number;
  // KPIs adicionales — alto impacto
  polizasPorRenovar30d: number;
  reclamosSinAsignar: number;
  tiempoPromedioResolucionDias: number;
  tasaMorosidad: number;
  tasaMorosidadTrend: number;
  // Segunda tanda
  ratioPerdidas: number;
  ticketPromedioCobro: number;
  ticketPromedioCobroTrend: number;
  tasaReclamos: number;
  polizasNuevasMes: number;
  polizasNuevasMesTrend: number;
  // Tercera tanda
  primaPromedioPorPoliza: number;
  cobrosCanceladosMes: number;
  porcentajeCobroUsd: number;
  reclamosFueraDeSla: number;
  valorPipelineCotizaciones: number;
  cotizacionesEnPipeline: number;
  tiempoPromedioConversion: number;
  polizasInactivasMes: number;
  alertas: AlertaDto[];
  // Charts
  cobrosMensuales: MultiSeriesChart[];
  cobrosEstadoMensual: MultiSeriesChart[];
  distribucionAseguradoras: ChartSeries[];
  // Sparklines
  sparklineTasaCobro: ChartDataPoint[];
  sparklineMontoRiesgo: ChartDataPoint[];
}

export interface CobroMensualDto {
  mes: string;
  mesLabel: string;
  montoEsperado: number;
  montoCobrado: number;
  montoVencido: number;
  cantidadCobros: number;
}

export interface CobrosTrendDto {
  mensual: CobroMensualDto[];
  promedioMensualEsperado: number;
  promedioMensualCobrado: number;
}

export interface AgingBucketDto {
  rango: string;
  diasMin: number;
  diasMax: number;
  cantidad: number;
  monto: number;
  porcentajeMonto: number;
  color: string;
}

export interface AgingReportDto {
  buckets: AgingBucketDto[];
  totalVencido: number;
  totalCobrosVencidos: number;
}

export interface PagoMetodoDistribucionDto {
  metodoPago: string;
  cantidad: number;
  monto: number;
  porcentaje: number;
}

export interface CashflowSemanaDto {
  semana: string;
  fechaInicio: string;
  fechaFin: string;
  montoEsperado: number;
  montoCobrado: number;
  esFuturo: boolean;
}

export interface CashflowForecastDto {
  semanas: CashflowSemanaDto[];
  totalProjectado: number;
  totalCobrado: number;
  tasaHistorica: number;
}

export interface TopDeudorDto {
  clienteNombre: string;
  correoElectronico: string;
  numeroPolizas: number;
  montoVencidoTotal: number;
  antiguedadMaxDias: number;
  moneda: string;
  cobroId: number;
}

export interface AgenteCobrosDto {
  nombreAgente: string;
  cobrosProcesados: number;
  montoCobrado: number;
  tiempoPromedioResDias: number;
}

export interface PortfolioDistribucionDto {
  porAseguradora: ChartSeries[];
  porTipoSeguro: ChartSeries[];
  porModalidad: ChartSeries[];
  porFrecuencia: ChartSeries[];
  composicionPorAseguradora: PortfolioBreakdownDto[];
  composicionPorTipoSeguro: PortfolioBreakdownDto[];
  composicionPorModalidad: PortfolioBreakdownDto[];
  composicionPorFrecuencia: PortfolioBreakdownDto[];
  radarAseguradoras: RadarDataDto[];
  retencion: RetencionRenovacionDto;
  histogramaPrima: HistogramaPrimaBucketDto[];
  mapaRiesgo: RiesgoConcentracionDto[];
  primaMensualTotal: number;
  totalPolizasActivas: number;
}

export interface PortfolioBreakdownDto {
  name: string;
  totalPolizas: number;
  primaMensual: number;
  porcentajePolizas: number;
  porcentajePrima: number;
}

export interface RadarDataDto {
  aseguradora: string;
  primaPromedio: number;
  numPolizas: number;
  tasaReclamos: number;
  tasaCobro: number;
  antiguedadPromedioDias: number;
}

export interface VencimientoMesDto {
  mes: string;
  mesLabel: string;
  totalPolizas: number;
  auto: number;
  vida: number;
  hogar: number;
  empresarial: number;
  primaEnRiesgo: number;
}

export interface VencimientosTimelineDto {
  meses: VencimientoMesDto[];
  totalPolizasProximasAnio: number;
  primaEnRiesgoAnio: number;
}

export interface VencimientoPolizaDetalleDto {
  numeroPoliza: string;
  clienteNombre: string;
  aseguradora: string;
  tipoSeguro: string;
  fechaVigencia: string;
  primaMensualNormalizada: number;
}

export interface VencimientoDetalleMesDto {
  mes: string;
  mesLabel: string;
  polizas: VencimientoPolizaDetalleDto[];
}

export interface RetencionRenovacionDto {
  totalVencidas: number;
  totalRenovadas: number;
  totalNoRenovadas: number;
  tasaRetencionPromedio: number;
  funnel: ChartSeries[];
  tasaRetencionMensual: ChartDataPoint[];
  retencionPorTipoSeguro: ChartSeries[];
  churnPorAseguradora: ChartSeries[];
}

export interface HistogramaPrimaBucketDto {
  rango: string;
  min: number;
  max: number;
  cantidad: number;
  primaMensualTotal: number;
}

export interface RiesgoConcentracionDto {
  segmento: string;
  aseguradora: string;
  tipoSeguro: string;
  primaUnitariaPromedio: number;
  montoAseguradoProxy: number;
  numeroClientes: number;
}

export interface FunnelEtapaDto {
  nombre: string;
  estado: string;
  cantidad: number;
  monto: number;
  tiempoPromedioHoras: number;
  porcentajeDel100: number;
}

export interface ReclamosFunnelDto {
  etapas: FunnelEtapaDto[];
  montoTotalReclamado: number;
  montoTotalAprobado: number;
  lossRatioGlobal: number;
}

export interface SlaPorTipoDto {
  nombre: string;
  totalReclamos: number;
  dentroSla: number;
  fueraSla: number;
  porcentajeDentroSla: number;
  tiempoPromedioHoras: number;
}

export interface SlaPorAgenteDto {
  nombreAgente: string;
  totalAsignados: number;
  dentroSla: number;
  porcentajeDentroSla: number;
  tiempoPromedioHoras: number;
  montoAprobado: number;
}

export interface ReclamoAlertoSlaDto {
  id: number;
  numeroReclamo: string;
  clienteNombre: string;
  fechaLimite: string;
  horasRestantes: number;
  prioridad: string;
}

export interface SlaReportDto {
  porcentajeGlobalDentroSLA: number;
  porTipo: SlaPorTipoDto[];
  porPrioridad: SlaPorTipoDto[];
  porAgente: SlaPorAgenteDto[];
  proximosVencer: ReclamoAlertoSlaDto[];
}

export interface ReclamosAgenteFiltroDto {
  id: number;
  nombre: string;
}

export interface LossRatioItemDto {
  segmento: string;
  montoAprobado: number;
  primaPortafolio: number;
  lossRatio: number;
  benchmarkMin: number;
  benchmarkMax: number;
}

export interface ResolucionTiempoStatsDto {
  grupo: string;
  total: number;
  minDias: number;
  maxDias: number;
  promedioDias: number;
}

export interface ReclamosHeatmapCellDto {
  mes: string;
  tipoReclamo: string;
  cantidad: number;
  montoReclamado: number;
}

export interface ReclamoScatterPointDto {
  reclamoId: number;
  numeroReclamo: string;
  tipoReclamo: string;
  montoReclamado: number;
  montoAprobado: number;
  duracionDias: number;
}

export interface RendimientoAgenteDto {
  agenteId: number;
  agente: string;
  reclamosCerrados: number;
  tiempoPromedioDias: number;
  porcentajeDentroSla: number;
  ratioAprobadoVsReclamado: number;
  sparklineCierresMensuales: ChartDataPoint[];
}

export interface ReclamosAnalyticsAdvancedDto {
  months: number;
  agenteId?: number;
  aseguradora?: string;
  aseguradorasDisponibles: string[];
  agentesDisponibles: ReclamosAgenteFiltroDto[];
  lossRatioPorAseguradora: LossRatioItemDto[];
  lossRatioPorTipo: LossRatioItemDto[];
  resolucionPorTipo: ResolucionTiempoStatsDto[];
  resolucionPorPrioridad: ResolucionTiempoStatsDto[];
  resolucionPorAgente: ResolucionTiempoStatsDto[];
  resolucionPorAseguradora: ResolucionTiempoStatsDto[];
  heatmapMesTipo: ReclamosHeatmapCellDto[];
  montoReclamadoVsAprobado: ReclamoScatterPointDto[];
  rendimientoAgentes: RendimientoAgenteDto[];
}

export interface TicketPromedioDto {
  tipoSeguro: string;
  modalidad: string;
  primaPromedio: number;
  cantidad: number;
}

export interface PipelineValorDto {
  mes: string;
  valorPendiente: number;
  valorAprobado: number;
  valorConvertido: number;
  valorAnualProyectado: number;
}

export interface VelocidadBucketDto {
  rango: string;
  cantidad: number;
}

export interface CotizacionesFunnelDto {
  total: number;
  pendientes: number;
  aprobadas: number;
  convertidas: number;
  rechazadas: number;
  tasaAprobacion: number;
  tasaConversion: number;
  velocidadPromedioDias: number;
  porTipoSeguro: ChartSeries[];
  porAseguradora: ChartSeries[];
  velocidadBuckets: VelocidadBucketDto[];
  ticketPromedio: TicketPromedioDto[];
  pipelineValor: PipelineValorDto[];
}

export interface EmailTipoStatsDto {
  tipo: string;
  enviados: number;
  exitosos: number;
  fallidos: number;
  tasaExito: number;
}

export interface EmailVolumenDiaDto {
  fecha: string;
  fechaLabel: string;
  totalEnviados: number;
  exitosos: number;
  fallidos: number;
}

export interface EmailCoberturaCobrosDto {
  cobrosVencidosTotal: number;
  conEmailNotificado: number;
  conEmailNoNotificado: number;
  sinEmail: number;
  porcentajeCoberturaNotificada: number;
}

export interface CorrelacionBucketDto {
  etiqueta: string;
  cantidad: number;
  porcentaje: number;
}

export interface EmailCorrelacionCanalDto {
  totalCobrosVencidosConEmail: number;
  totalCobrosVencidosSinEmail: number;
  tasaPagoConEmail: number;
  tasaPagoSinEmail: number;
  bucketsConEmail: CorrelacionBucketDto[];
}

export interface EmailAnalyticsDto {
  porTipo: EmailTipoStatsDto[];
  volumenPorDia: EmailVolumenDiaDto[];
  heatmapEnvios: HeatmapCell[];
  tasaExitoGlobal: number;
  totalEnviadosPeriodo: number;
  totalFallidosPeriodo: number;
  coberturaCobros: EmailCoberturaCobrosDto;
  correlacionCanal: EmailCorrelacionCanalDto;
}

export interface AgenteCargaDto {
  nombreAgente: string;
  reclamosActivos: number;
  reclamosAltaPrioridad: number;
  reclamosCriticos: number;
  porcentajeSla: number;
  tieneCriticosExclusivos: boolean;
  totalAccionesUltimas12Semanas: number;
}

export interface HeatmapCellSemanaDto {
  semana: string;
  acciones: number;
}

export interface HeatmapAgenteRowDto {
  nombreAgente: string;
  semanas: HeatmapCellSemanaDto[];
}

export interface UsuarioActividadDto {
  nombreUsuario: string;
  email: string;
  rol: string;
  ultimoLogin: string | null;
  diasSinAcceso: number;
  accionesAyer: number;
  esActivo: boolean;
}

export interface ChatSesionSemanalDto {
  semana: string;
  activas: number;
  cerradas: number;
}

export interface TopPalabraDto {
  palabra: string;
  frecuencia: number;
}

export interface ChatIAStatsDto {
  totalSesiones: number;
  sesionesActivas: number;
  sesionesCerradas: number;
  mensajesPorSesionPromedio: number;
  tiempoProcesPromedioMs: number;
  mensajesPositivos: number;
  mensajesNegativos: number;
  tasaSatisfaccion: number;
  sesionesPorSemana: ChatSesionSemanalDto[];
  topPalabras: TopPalabraDto[];
}

export interface EndpointPercentilDto {
  endpoint: string;
  p50Ms: number;
  p90Ms: number;
  p99Ms: number;
  samples: number;
}

export interface SistemaRendimientoDto {
  disponible: boolean;
  endpoints: EndpointPercentilDto[];
}

export interface OperacionalStatsDto {
  actividadHeatmapPorAgente: HeatmapAgenteRowDto[];
  cargaReclamos: AgenteCargaDto[];
  riesgoConcentracionCritica: boolean;
  agenteConcentracionCritica: string | null;
  ultimoAcceso: UsuarioActividadDto[];
  chatIA: ChatIAStatsDto;
  sistemaRendimiento: SistemaRendimientoDto;
  alertas: string[];
}

export interface ReportDispatchResultDto {
  emailsEnviados: number;
  message: string;
}

// ── MÓDULO 7 — Analítica Predictiva ─────────────────────────────────────────

export interface PolizaRiesgoDto {
  numeroPoliza: string;
  nombreAsegurado: string;
  aseguradora: string;
  tipoSeguro: string;
  frecuencia: string;
  score: number;
  categoria: string;
  tasaVencido: number;
  totalCobros: number;
  cobrosVencidos: number;
}

export interface PredictivoScoreMorosidadDto {
  polizas: PolizaRiesgoDto[];
  distribucionRiesgo: ChartDataPoint[];
}

export interface PrediccionMesDto {
  mesLabel: string;
  esperado: number;
  anioAnterior: number;
  cambioPorc: number;
}

export interface PredictivoReclamosDto {
  proximosMeses: PrediccionMesDto[];
  historicoSerie: MultiSeriesChart[];
}

export interface AnomaliaCobroDto {
  id: number;
  numeroRecibo: string;
  clienteNombre: string;
  numeroPoliza: string;
  montoTotal: number;
  zScore: number;
  mediaCliente: number;
  tipoAnomalia: string;
  estadoActual: string;
}

export interface PredictivoAnomaliasCobrosDto {
  anomalias: AnomaliaCobroDto[];
  mediaGlobal: number;
  desviacionGlobal: number;
  totalAnalizados: number;
}

export interface PolizaLeadDto {
  numeroPoliza: string;
  nombreAsegurado: string;
  correo: string;
  telefono: string;
  aseguradora: string;
  tipoSeguro: string;
  primaMensual: number;
  fechaVigencia: string;
  diasParaVencer: number;
  score: number;
  factores: string[];
}

export interface PredictivoRenovacionDto {
  topPolizas: PolizaLeadDto[];
  totalPolizasPorRenovar60d: number;
  scorePromedio: number;
  distribucionScore: ChartDataPoint[];
}

export interface ForecastMesDto {
  mesLabel: string;
  montoEsperado: number;
  montoProyectado: number;
  esFuturo: boolean;
}

export interface PredictivoForecastPrimaDto {
  cobrosProgramadosProximoMes: number;
  tasaHistoricaCobro6m: number;
  primaProyectada: number;
  icInferior: number;
  icSuperior: number;
  proyeccionMensual: ForecastMesDto[];
}

export interface PredictivoAnalyticsDto {
  scoreMorosidad: PredictivoScoreMorosidadDto;
  reclamosTemporada: PredictivoReclamosDto;
  anomaliasCobros: PredictivoAnomaliasCobrosDto;
  renovacion: PredictivoRenovacionDto;
  forecastPrima: PredictivoForecastPrimaDto;
}

// ─────────────────────────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly base = `${environment.apiUrl}/analytics`;

  constructor(private readonly http: HttpClient) {}

  getDashboard(): Observable<ExecutiveDashboardDto> {
    return this.http.get<ExecutiveDashboardDto>(`${this.base}/dashboard`);
  }

  getCobrosTrend(months = 18): Observable<CobrosTrendDto> {
    return this.http.get<CobrosTrendDto>(`${this.base}/cobros/trend`, {
      params: new HttpParams().set('months', months)
    });
  }

  getCobrosAging(): Observable<AgingReportDto> {
    return this.http.get<AgingReportDto>(`${this.base}/cobros/aging`);
  }

  getCobrosMetodosPago(): Observable<PagoMetodoDistribucionDto[]> {
    return this.http.get<PagoMetodoDistribucionDto[]>(`${this.base}/cobros/metodos-pago`);
  }

  getCashflowForecast(weeks = 12): Observable<CashflowForecastDto> {
    return this.http.get<CashflowForecastDto>(`${this.base}/cobros/cashflow-forecast`, {
      params: new HttpParams().set('weeks', weeks)
    });
  }

  getTopDeudores(top = 10): Observable<TopDeudorDto[]> {
    return this.http.get<TopDeudorDto[]>(`${this.base}/cobros/top-deudores`, {
      params: new HttpParams().set('top', top)
    });
  }

  getCobrosHeatmap(): Observable<HeatmapCell[]> {
    return this.http.get<HeatmapCell[]>(`${this.base}/cobros/heatmap`);
  }

  getCobrosPorAgente(): Observable<AgenteCobrosDto[]> {
    return this.http.get<AgenteCobrosDto[]>(`${this.base}/cobros/por-agente`);
  }

  getPortfolio(): Observable<PortfolioDistribucionDto> {
    return this.http.get<PortfolioDistribucionDto>(`${this.base}/portfolio`);
  }

  getVencimientos(months = 12): Observable<VencimientosTimelineDto> {
    return this.http.get<VencimientosTimelineDto>(`${this.base}/portfolio/vencimientos`, {
      params: new HttpParams().set('months', months)
    });
  }

  getVencimientosDetalle(month: string): Observable<VencimientoDetalleMesDto> {
    return this.http.get<VencimientoDetalleMesDto>(`${this.base}/portfolio/vencimientos/detalle`, {
      params: new HttpParams().set('month', month)
    });
  }

  getReclamosFunnel(): Observable<ReclamosFunnelDto> {
    return this.http.get<ReclamosFunnelDto>(`${this.base}/reclamos/funnel`);
  }

  getReclamosSla(): Observable<SlaReportDto> {
    return this.http.get<SlaReportDto>(`${this.base}/reclamos/sla`);
  }

  getReclamosAdvanced(months = 12, agenteId?: number, aseguradora?: string): Observable<ReclamosAnalyticsAdvancedDto> {
    let params = new HttpParams().set('months', months);
    if (typeof agenteId === 'number') {
      params = params.set('agenteId', agenteId);
    }
    if (aseguradora) {
      params = params.set('aseguradora', aseguradora);
    }
    return this.http.get<ReclamosAnalyticsAdvancedDto>(`${this.base}/reclamos/advanced`, { params });
  }

  getCotizacionesFunnel(): Observable<CotizacionesFunnelDto> {
    return this.http.get<CotizacionesFunnelDto>(`${this.base}/cotizaciones/funnel`);
  }

  getEmailAnalytics(days = 30): Observable<EmailAnalyticsDto> {
    return this.http.get<EmailAnalyticsDto>(`${this.base}/emails`, {
      params: new HttpParams().set('days', days)
    });
  }

  getCarteraAseguradoraExcel(aseguradora?: string): Observable<Blob> {
    let params = new HttpParams();
    if (aseguradora) {
      params = params.set('aseguradora', aseguradora);
    }
    return this.http.get(`${this.base}/reportes/cartera-aseguradora/excel`, {
      params,
      responseType: 'blob'
    });
  }

  getCarteraAseguradoraPdf(aseguradora?: string): Observable<Blob> {
    let params = new HttpParams();
    if (aseguradora) {
      params = params.set('aseguradora', aseguradora);
    }
    return this.http.get(`${this.base}/reportes/cartera-aseguradora/pdf`, {
      params,
      responseType: 'blob'
    });
  }

  getMorosidadExcel(): Observable<Blob> {
    return this.http.get(`${this.base}/reportes/morosidad/excel`, {
      responseType: 'blob'
    });
  }

  sendMorosidadByEmail(recipients: string[]): Observable<ReportDispatchResultDto> {
    return this.http.post<ReportDispatchResultDto>(`${this.base}/reportes/morosidad/email`, {
      recipients
    });
  }

  getReclamosSlaPdf(): Observable<Blob> {
    return this.http.get(`${this.base}/reportes/reclamos-sla/pdf`, {
      responseType: 'blob'
    });
  }

  getEstadoPortafolioPdf(): Observable<Blob> {
    return this.http.get(`${this.base}/reportes/estado-portafolio/pdf`, {
      responseType: 'blob'
    });
  }

  sendEstadoPortafolioAdmin(): Observable<ReportDispatchResultDto> {
    return this.http.post<ReportDispatchResultDto>(`${this.base}/reportes/estado-portafolio/email-admins`, {});
  }

  getOperacional(): Observable<OperacionalStatsDto> {
    return this.http.get<OperacionalStatsDto>(`${this.base}/operacional`);
  }

  getPredictivo(): Observable<PredictivoAnalyticsDto> {
    return this.http.get<PredictivoAnalyticsDto>(`${this.base}/predictivo`);
  }

  getAgenda(): Observable<AgendaDto> {
    return this.http.get<AgendaDto>(`${this.base}/agenda`);
  }

  getCliente360(cedula: string): Observable<Cliente360Dto> {
    return this.http.get<Cliente360Dto>(`${this.base}/cliente360`, {
      params: new HttpParams().set('cedula', cedula)
    });
  }

  searchClientes(query: string): Observable<Cliente360SearchResultDto[]> {
    return this.http.get<Cliente360SearchResultDto[]>(`${this.base}/cliente360`, {
      params: new HttpParams().set('nombre', query)
    });
  }
}

// ── M14 Agenda Inteligente ────────────────────────────────────────────────────
export interface AgendaItemDto {
  tipo: string;
  titulo: string;
  descripcion: string;
  nivel: string;
  cedula?: string;
  numeroRef?: string;
  navLink?: string;
  monto?: number;
  diasVencido?: number;
  score?: number;
}

export interface AgendaSeccionDto {
  titulo: string;
  nivel: string;
  icono: string;
  total: number;
  items: AgendaItemDto[];
}

export interface AgendaDto {
  fechaGeneracion: string;
  totalAcciones: number;
  secciones: AgendaSeccionDto[];
}

// ── M9 Cliente 360° ───────────────────────────────────────────────────────────
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
