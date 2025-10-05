export interface Cobro {
  id: number;
  numeroRecibo: string;
  polizaId: number;
  numeroPoliza: string;
  clienteNombre: string;
  clienteApellido: string;
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

export enum EstadoCobro {
  Pendiente = 0,
  Cobrado = 1,
  Vencido = 2,
  Cancelado = 3
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

// Funciones helper para convertir enums a labels
export function getEstadoCobroLabel(estado: EstadoCobro): string {
  switch (estado) {
    case EstadoCobro.Pendiente: return 'Pendiente';
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