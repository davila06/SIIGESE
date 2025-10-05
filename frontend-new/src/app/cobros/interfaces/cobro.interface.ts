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
  PENDIENTE = 'Pendiente',
  COBRADO = 'Cobrado',
  VENCIDO = 'Vencido',
  CANCELADO = 'Cancelado'
}

export enum MetodoPago {
  EFECTIVO = 'Efectivo',
  TRANSFERENCIA = 'Transferencia',
  CHEQUE = 'Cheque',
  TARJETA_CREDITO = 'Tarjeta de Crédito',
  TARJETA_DEBITO = 'Tarjeta de Débito'
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