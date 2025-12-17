export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  userName?: string; // Para compatibilidad con código existente
  roles: Role[];
  isActive: boolean;
  lastLoginAt?: string; // Agregar esta propiedad opcional
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
  isActive?: boolean; // Agregar esta propiedad
}

export interface UpdateUser {
  email?: string;
  firstName?: string;
  lastName?: string;
  userName?: string;
  roleIds?: number[];
  isActive?: boolean;
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

export interface Role {
  id: number;
  name: string;
  description: string;
  permissions: string[];
  isActive: boolean;
}

export interface DataUploadResult {
  success: boolean;
  totalRecords: number;
  processedRecords: number;
  errorRecords: number;
  errors: string[];
  failedRecords: FailedRecord[];
  message?: string;
  status?: string; // Para compatibilidad con response-mapper
}

export interface FailedRecord {
  rowNumber: number;
  error: string;
  originalData: Record<string, any>;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: User;
}

export interface ApiResponse<T = any> {
  data: T;
  message: string;
  success: boolean;
}