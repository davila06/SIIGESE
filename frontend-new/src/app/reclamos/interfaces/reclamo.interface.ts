export interface Reclamo {
  id: number;
  numeroReclamo: string;
  polizaId: number;
  numeroPoliza: string;
  clienteNombre: string;
  clienteApellido: string;
  tipoReclamo: TipoReclamo;
  descripcion: string;
  fechaReclamo: Date;
  fechaOcurrencia?: Date;
  montoReclamado?: number;
  montoAprobado?: number;
  estado: EstadoReclamo;
  prioridad: PrioridadReclamo;
  asignadoA?: string;
  observaciones?: string;
  documentosAdjuntos?: DocumentoAdjunto[];
  fechaResolucion?: Date;
  motivoRechazo?: string;
  usuarioCreacion: string;
  fechaCreacion: Date;
  fechaActualizacion?: Date;
}

export enum TipoReclamo {
  SINIESTRO = 'Siniestro',
  REEMBOLSO = 'Reembolso',
  QUEJA_SERVICIO = 'Queja de Servicio',
  CANCELACION = 'Cancelación',
  CAMBIO_POLIZA = 'Cambio de Póliza',
  OTRO = 'Otro'
}

export enum EstadoReclamo {
  PENDIENTE = 'Pendiente',
  EN_REVISION = 'En Revisión',
  REQUIERE_DOCUMENTOS = 'Requiere Documentos',
  APROBADO = 'Aprobado',
  RECHAZADO = 'Rechazado',
  RESUELTO = 'Resuelto',
  CERRADO = 'Cerrado'
}

export enum PrioridadReclamo {
  BAJA = 'Baja',
  MEDIA = 'Media',
  ALTA = 'Alta',
  URGENTE = 'Urgente'
}

export interface DocumentoAdjunto {
  id: number;
  nombre: string;
  tipo: string;
  tamaño: number;
  url: string;
  fechaSubida: Date;
}

export interface ReclamoRequest {
  polizaId: number;
  tipoReclamo: TipoReclamo;
  descripcion: string;
  fechaOcurrencia?: Date;
  montoReclamado?: number;
  prioridad?: PrioridadReclamo;
}

export interface ActualizarEstadoRequest {
  reclamoId: number;
  nuevoEstado: EstadoReclamo;
  observaciones?: string;
  montoAprobado?: number;
  motivoRechazo?: string;
}

export interface ReclamosStats {
  totalPendientes: number;
  totalEnRevision: number;
  totalAprobados: number;
  totalRechazados: number;
  totalResueltos: number;
  montoTotalReclamado: number;
  montoTotalAprobado: number;
  tiempoPromedioResolucion: number; // en días
}

export interface FiltroReclamos {
  estado?: EstadoReclamo;
  tipoReclamo?: TipoReclamo;
  prioridad?: PrioridadReclamo;
  fechaDesde?: Date;
  fechaHasta?: Date;
  asignadoA?: string;
}