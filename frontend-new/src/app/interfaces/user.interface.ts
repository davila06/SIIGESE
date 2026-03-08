export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  userName?: string;
  roles: Role[];
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateUser {
  email: string;
  firstName: string;
  lastName: string;
  userName?: string;
  password: string;
  roleIds: number[];
  isActive?: boolean;
}

export interface UpdateUser {
  email?: string;
  firstName?: string;
  lastName?: string;
  userName?: string;
  roleIds?: number[];
  isActive?: boolean;
}

export interface Role {
  id: number;
  name: string;
  description: string;
  permissions: string[];
  isActive: boolean;
}

export interface Poliza {
  id: number;
  numeroPoliza: string;
  modalidad: string;
  nombreAsegurado: string;
  numeroCedula: string;
  prima: number;
  moneda: string;
  fechaVigencia: string;
  frecuencia: string;
  aseguradora: string;
  placa: string;
  marca: string;
  modelo: string;
  año: string;
  correo: string;
  numeroTelefono: string;
  perfilId: number;
  esActivo: boolean;
  observaciones?: string;
  fechaCreacion: string;
  usuarioCreacion: string;
}

export interface CreatePoliza {
  numeroPoliza: string;
  modalidad: string;
  nombreAsegurado: string;
  numeroCedula: string;
  prima: number;
  moneda: string;
  fechaVigencia: string;
  frecuencia: string;
  aseguradora: string;
  placa: string;
  marca: string;
  modelo: string;
  año: string;
  correo: string;
  numeroTelefono: string;
  perfilId: number;
  observaciones?: string;
}

export interface Cobro {
  id: number;
  numeroRecibo: string;
  polizaId: number;
  numeroPoliza: string;
  clienteNombreCompleto: string;
  correoElectronico: string;
  montoTotal: number;
  montoCobrado: number;
  fechaVencimiento: string;
  fechaCobro: string | null;
  estado: string;
  metodoPago: string;
  moneda: string;
  observaciones: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCobro {
  polizaId: number;
  correoElectronico?: string;
  montoTotal: number;
  fechaVencimiento: string;
  metodoPago: string;
  moneda: string;
  observaciones?: string;
}

export interface RegistrarCobroRequest {
  metodoPago: string;
  montoCobrado: number;
  fechaCobro?: string;
  observaciones?: string;
}

export interface CancelarCobroRequest {
  observaciones?: string;
}

export interface CobroStats {
  totalCobros: number;
  cobrosPendientes: number;
  cobrosPagados: number;
  cobrosVencidos: number;
  montoTotalPendiente: number;
  montoTotalCobrado: number;
  porcentajeCobrado: number;
  cobrosProximosVencer: number;
}

export interface Reclamo {
  id: number;
  numeroReclamo: string;
  numeroPoliza: string;
  tipoReclamo: string;
  descripcion: string;
  fechaReclamo: string;
  fechaLimiteRespuesta: string | null;
  fechaResolucion: string | null;
  estado: string;
  prioridad: string;
  montoReclamado: number;
  montoAprobado: number | null;
  nombreAsegurado: string;
  clienteNombreCompleto: string;
  observaciones: string;
  documentosAdjuntos: string;
  usuarioAsignadoId: number | null;
  moneda: string;
  createdBy: string;
  updatedBy: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateReclamo {
  numeroPoliza: string;
  tipoReclamo: string;
  descripcion: string;
  fechaLimiteRespuesta?: string;
  prioridad: string;
  montoReclamado: number;
  nombreAsegurado: string;
  clienteNombreCompleto?: string;
  observaciones?: string;
  usuarioAsignadoId?: number;
  moneda?: string;
}

export interface ChangeEstadoReclamoRequest {
  estado: string;
  observaciones?: string;
}

export interface AsignarReclamoRequest {
  usuarioId: number;
}

export interface ResolverReclamoRequest {
  montoAprobado: number;
  observaciones?: string;
}

export interface RechazarReclamoRequest {
  motivo: string;
  observaciones?: string;
}

export interface ReclamoFilterParams {
  estado?: string;
  prioridad?: string;
  tipoReclamo?: string;
  numeroPoliza?: string;
  fechaDesde?: string;
  fechaHasta?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface ReclamoStats {
  totalReclamos: number;
  reclamosPendientes: number;
  reclamosEnProceso: number;
  reclamosResueltos: number;
  reclamosRechazados: number;
  montoTotalReclamado: number;
  montoTotalAprobado: number;
  tasaAprobacion: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface Cotizacion {
  id: number;
  numeroCotizacion: string;
  nombreCliente: string;
  correoCliente: string;
  telefonoCliente: string;
  tipoSeguro: string;
  descripcion: string;
  montoEstimado: number;
  moneda: string;
  estado: string;
  vigencia: string;
  observaciones: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCotizacion {
  nombreCliente: string;
  correoCliente: string;
  telefonoCliente?: string;
  tipoSeguro: string;
  descripcion: string;
  montoEstimado: number;
  moneda: string;
  vigencia?: string;
  observaciones?: string;
}

export interface EmailConfig {
  id: number;
  nombre: string;
  servidor: string;
  puerto: number;
  usarSsl: boolean;
  correoRemitente: string;
  nombreRemitente: string;
  usuario: string;
  esDefault: boolean;
  esActivo: boolean;
  createdAt: string;
}

export interface CreateEmailConfig {
  nombre: string;
  servidor: string;
  puerto: number;
  usarSsl: boolean;
  correoRemitente: string;
  nombreRemitente: string;
  usuario: string;
  password: string;
}

export interface DataUploadResult {
  success: boolean;
  totalRecords: number;
  processedRecords: number;
  errorRecords: number;
  errors: string[];
  failedRecords: FailedRecord[];
  message?: string;
  status?: string;
}

export interface FailedRecord {
  rowNumber: number;
  error: string;
  originalData: Record<string, unknown>;
}

export interface TokenInfo {
  usuario: string;
  nombre: string;
  roles: string[];
  expira: Date;
  expirado: boolean;
  necesitaRenovacion: boolean;
  minutosRestantes: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: User;
  expiresAt?: string;
}

export interface ApiResponse<T = unknown> {
  data: T;
  message: string;
  success: boolean;
}