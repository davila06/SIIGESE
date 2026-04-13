export interface Cobro {
  id: number;
  numeroRecibo: string;
  polizaId: number;
  numeroPoliza: string;
  clienteNombreCompleto: string;
  correoElectronico?: string;
  frecuencia?: string;
  fechaVencimiento: Date;
  fechaCobro?: Date;
  montoTotal: number;
  montoCobrado?: number;
  moneda?: string; // Código de moneda (CRC, USD, EUR)
  estado: EstadoCobro;
  metodoPago?: MetodoPago;
  observaciones?: string;
  usuarioCobroId?: number;
  usuarioCobroNombre?: string;
  fechaCreacion: Date;
  fechaActualizacion?: Date;
}

export interface GenerarCobrosResult {
  cobrosGenerados: number;
  polizasProcesadas: number;
  polizasSaltadas: number;
  errores: string[];
  cobrosCreados: CobroGenerado[];
}

export interface CobroGenerado {
  numeroRecibo: string;
  numeroPoliza: string;
  fechaVencimiento: Date;
  montoTotal: number;
  moneda: string;
}

export enum EstadoCobro {
  Pendiente = 0,
  Pagado = 1,
  Cobrado = 2,
  Vencido = 3,
  Cancelado = 4
}

export enum MetodoPago {
  Efectivo = 0,
  Transferencia = 1,
  Cheque = 2,
  TarjetaCredito = 3,
  TarjetaDebito = 4
}

export interface CobroRequest {
  polizaId: number;
  fechaVencimiento: Date;
  montoTotal: number;
  moneda?: string; // Código de moneda (CRC por defecto)
  correoElectronico?: string;
  observaciones?: string;
}

export interface RegistrarCobroRequest {
  cobroId: number;
  fechaCobro: Date;
  montoCobrado: number;
  metodoPago: MetodoPago;
  observaciones?: string;
}

export interface CobroStats {
  totalPendientes: number;
  totalCobrados: number;
  totalVencidos: number;
  montoTotalPendiente: number;
  montoTotalCobrado: number;
  montoPorVencer: number;
}

export enum EstadoSolicitudCambioCobro {
  Pendiente = 0,
  Aprobada = 1,
  Rechazada = 2
}

export interface SolicitarCambioEstadoCobroRequest {
  estadoSolicitado: EstadoCobro;
  motivo?: string;
}

export interface ResolverCambioEstadoCobroRequest {
  motivo?: string;
}

export interface CobroEstadoChangeRequest {
  id: number;
  cobroId: number;
  numeroRecibo: string;
  numeroPoliza: string;
  clienteNombreCompleto: string;
  estadoActual: EstadoCobro;
  estadoSolicitado: EstadoCobro;
  estadoSolicitud: EstadoSolicitudCambioCobro;
  motivoSolicitud?: string;
  motivoDecision?: string;
  solicitadoPorUserId: number;
  solicitadoPorNombre: string;
  solicitadoPorEmail: string;
  resueltoPorUserId?: number;
  resueltoPorNombre?: string;
  createdAt: Date;
  resueltoAt?: Date;
}

export interface CobroChangeRequestActionResult {
  request: CobroEstadoChangeRequest;
  cobro: Cobro;
}

// Funciones helper para convertir enums a labels
export function getEstadoCobroLabel(estado: EstadoCobro): string {
  switch (estado) {
    case EstadoCobro.Pendiente: return 'Pendiente';
    case EstadoCobro.Pagado: return 'Cobrado';
    case EstadoCobro.Cobrado: return 'Cobrado';
    case EstadoCobro.Vencido: return 'Vencido';
    case EstadoCobro.Cancelado: return 'Cancelado';
    default: return 'Desconocido';
  }
}

export function getMetodoPagoLabel(metodo: MetodoPago): string {
  switch (metodo) {
    case MetodoPago.Efectivo: return 'Efectivo';
    case MetodoPago.Transferencia: return 'Transferencia';
    case MetodoPago.Cheque: return 'Cheque';
    case MetodoPago.TarjetaCredito: return 'Tarjeta de Crédito';
    case MetodoPago.TarjetaDebito: return 'Tarjeta de Débito';
    default: return 'Desconocido';
  }
}

export function getEstadoSolicitudLabel(estado: EstadoSolicitudCambioCobro): string {
  switch (estado) {
    case EstadoSolicitudCambioCobro.Pendiente: return 'Pendiente';
    case EstadoSolicitudCambioCobro.Aprobada: return 'Aprobada';
    case EstadoSolicitudCambioCobro.Rechazada: return 'Rechazada';
    default: return 'Desconocido';
  }
}