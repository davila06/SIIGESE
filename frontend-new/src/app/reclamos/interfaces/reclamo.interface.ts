export interface Reclamo {
  id: number;
  numeroReclamo: string;
  polizaId: number;
  numeroPoliza: string;
  clienteNombreCompleto: string;
  fechaReclamo: string;
  fechaResolucion?: string;
  tipoReclamo: TipoReclamo;
  estado: EstadoReclamo;
  descripcion: string;
  montoReclamado?: number;
  montoAprobado?: number;
  moneda: string;
  prioridad: PrioridadReclamo;
  observaciones?: string;
  documentosAdjuntos?: string;
  usuarioAsignadoId?: number;
  usuarioAsignadoNombre?: string;
  fechaLimiteRespuesta?: string;
  createdAt: string;
  updatedAt?: string;
  createdBy: string;
  updatedBy?: string;
  isDeleted: boolean;
  poliza?: any;
  usuarioAsignado?: any;
}

export interface CreateReclamoDto {
  polizaId: number;
  numeroPoliza: string;
  clienteNombreCompleto: string;
  tipoReclamo: number;
  descripcion: string;
  montoReclamado?: number;
  moneda: string;
  prioridad: number;
  observaciones?: string;
  documentosAdjuntos?: string;
  usuarioAsignadoId?: number;
  fechaLimiteRespuesta?: string;
}

export interface UpdateReclamoDto {
  numeroPoliza?: string;
  clienteNombreCompleto?: string;
  tipoReclamo?: number;
  estado?: number;
  descripcion?: string;
  montoReclamado?: number;
  montoAprobado?: number;
  moneda?: string;
  prioridad?: number;
  observaciones?: string;
  documentosAdjuntos?: string;
  usuarioAsignadoId?: number;
  fechaLimiteRespuesta?: string;
}

export interface ReclamosStats {
  totalReclamos: number;
  reclamosAbiertos: number;
  reclamosEnProceso: number;
  reclamosResueltos: number;
  reclamosCerrados: number;
  reclamosRechazados: number;
  totalMontoReclamado: number;
  totalMontoAprobado: number;
  monedaPrincipal: string;
  reclamosPrioridadAlta: number;
  reclamosPrioridadCritica: number;
  reclamosVencidos: number;
}

export interface FiltroReclamos {
  estado?: number;
  tipoReclamo?: number;
  prioridad?: number;
  clienteNombreCompleto?: string;
  numeroPoliza?: string;
  fechaDesde?: string;
  fechaHasta?: string;
  usuarioAsignadoId?: number;
  soloVencidos?: boolean;
  moneda?: string;
}

export enum TipoReclamo {
  Siniestro = 0,
  Servicio = 1,
  Facturacion = 2,
  Cobertura = 3,
  Proceso = 4,
  Otro = 5
}

export enum EstadoReclamo {
  Abierto = 0,
  EnProceso = 1,
  Resuelto = 2,
  Cerrado = 3,
  Rechazado = 4,
  Escalado = 5
}

export enum PrioridadReclamo {
  Baja = 0,
  Media = 1,
  Alta = 2,
  Critica = 3
}

// Helper functions para mostrar labels
export function getTipoReclamoLabel(tipo: TipoReclamo): string {
  switch (tipo) {
    case TipoReclamo.Siniestro: return 'Siniestro';
    case TipoReclamo.Servicio: return 'Servicio';
    case TipoReclamo.Facturacion: return 'Facturación';
    case TipoReclamo.Cobertura: return 'Cobertura';
    case TipoReclamo.Proceso: return 'Proceso';
    case TipoReclamo.Otro: return 'Otro';
    default: return 'Desconocido';
  }
}

export function getEstadoReclamoLabel(estado: EstadoReclamo): string {
  switch (estado) {
    case EstadoReclamo.Abierto: return 'Abierto';
    case EstadoReclamo.EnProceso: return 'En Proceso';
    case EstadoReclamo.Resuelto: return 'Resuelto';
    case EstadoReclamo.Cerrado: return 'Cerrado';
    case EstadoReclamo.Rechazado: return 'Rechazado';
    case EstadoReclamo.Escalado: return 'Escalado';
    default: return 'Desconocido';
  }
}

export function getPrioridadReclamoLabel(prioridad: PrioridadReclamo): string {
  switch (prioridad) {
    case PrioridadReclamo.Baja: return 'Baja';
    case PrioridadReclamo.Media: return 'Media';
    case PrioridadReclamo.Alta: return 'Alta';
    case PrioridadReclamo.Critica: return 'Crítica';
    default: return 'Desconocido';
  }
}