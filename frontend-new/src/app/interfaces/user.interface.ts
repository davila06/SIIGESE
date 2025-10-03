export interface User {
  id: number;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  roles: Role[];
  createdAt?: Date;
  lastLoginAt?: Date;
}

export interface CreateUser {
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  isActive: boolean;
  roleIds: number[];
}

export interface UpdateUser {
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  roleIds: number[];
}

export interface UserRole {
  id: number;
  userId: number;
  roleId: number;
  user?: User;
  role?: Role;
}

export interface Role {
  id: number;
  name: string;
  description: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: Date;
  user: User;
}

export interface Cliente {
  id: number;
  codigo: string;
  razonSocial: string;
  nombreComercial: string;
  nit: string;
  telefono: string;
  email: string;
  direccion: string;
  ciudad: string;
  departamento: string;
  pais: string;
  esActivo: boolean;
  fechaRegistro?: Date;
  perfilId: number;
}

export interface CreateCliente {
  codigo: string;
  razonSocial: string;
  nombreComercial: string;
  nit: string;
  telefono: string;
  email: string;
  direccion: string;
  ciudad: string;
  departamento: string;
  pais: string;
  perfilId: number;
}

export interface CreateDataRecord {
  perfilId: number;
  nombre: string;
  documento: string;
  telefono?: string;
  email?: string;
  direccion?: string;
}

export interface Poliza {
  id: number;
  perfilId: number;
  numeroPoliza: string;
  modalidad: string;
  nombreAsegurado: string;
  prima: number;
  moneda: string;
  fechaVigencia: Date;
  frecuencia: string;
  aseguradora: string;
  placa?: string;
  marca?: string;
  modelo?: string;
  fechaCreacion: Date;
  fechaModificacion?: Date;
  usuarioCreacion: string;
  usuarioModificacion?: string;
  esActivo: boolean;
}

export interface CreatePoliza {
  perfilId: number;
  numeroPoliza: string;
  modalidad: string;
  nombreAsegurado: string;
  prima: number;
  moneda: string;
  fechaVigencia: Date;
  frecuencia: string;
  aseguradora: string;
  placa?: string;
  marca?: string;
  modelo?: string;
}

export interface DataUploadResult {
  success: boolean;
  message: string;
  totalRecords: number;
  processedRecords: number;
  errorRecords: number;
  errors: string[];
  failedRecords: FailedRecord[];
  status: string;
}

export interface FailedRecord {
  rowNumber: number;
  error: string;
  originalData: { [key: string]: string };
}

export interface ApiResponse<T> {
  data?: T;
  message?: string;
  errors?: string[];
}