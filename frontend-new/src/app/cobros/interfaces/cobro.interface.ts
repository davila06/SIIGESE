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
  estado: number; // EstadoCobro como número
  metodoPago?: number; // MetodoPago como número nullable
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
export function getEstadoCobroLabel(estado: any): string {
  // Verificar tipos y valores
  if (estado === null || estado === undefined) {
    return 'NULL/UNDEFINED';
  }
  
  const tipo = typeof estado;
  
  if (tipo === 'string') {
    if (estado === '') {
      return 'STRING_VACIO';
    }
    const num = parseInt(estado, 10);
    if (isNaN(num)) {
      return `STR:"${estado}"`;
    }
    return getEstadoFromNumber(num);
  } else if (tipo === 'number') {
    return getEstadoFromNumber(estado);
  } else {
    return `TIPO:${tipo}`;
  }
}

function getEstadoFromNumber(num: number): string {
  switch (num) {
    case 0: return 'Pendiente';
    case 1: return 'Cobrado';
    case 2: return 'Vencido';
    case 3: return 'Cancelado';
    default: return `NUM:${num}`;
  }
}

export function getMetodoPagoLabel(metodo: MetodoPago | number | string | null | undefined): string {
  if (metodo === null || metodo === undefined) {
    return 'NULL/UNDEFINED';
  }
  
  const tipo = typeof metodo;
  
  if (tipo === 'string') {
    if (metodo === '') {
      return 'STRING_VACIO';
    }
    const num = parseInt(metodo as string, 10);
    if (isNaN(num)) {
      return `STR:"${metodo}"`;
    }
    return getMetodoFromNumber(num);
  } else if (tipo === 'number') {
    return getMetodoFromNumber(metodo as number);
  } else {
    return `TIPO:${tipo}`;
  }
}

function getMetodoFromNumber(num: number): string {
  switch (num) {
    case 0: return 'Efectivo';
    case 1: return 'Transferencia';
    case 2: return 'Cheque';
    case 3: return 'Tarjeta Crédito';
    case 4: return 'Tarjeta Débito';
    default: return `NUM:${num}`;
  }
}