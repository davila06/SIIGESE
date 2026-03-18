import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  DataUploadResult,
  FailedRecord,
  LoginRequest,
  LoginResponse,
  User,
  CreateUser,
  UpdateUser,
  Poliza,
  CreatePoliza,
  Cobro,
  CreateCobro,
  RegistrarCobroRequest,
  CancelarCobroRequest,
  CobroStats,
  Reclamo,
  CreateReclamo,
  ChangeEstadoReclamoRequest,
  AsignarReclamoRequest,
  ResolverReclamoRequest,
  RechazarReclamoRequest,
  ReclamoFilterParams,
  ReclamoStats,
  PagedResult,
  Cotizacion,
  CreateCotizacion,
  EmailConfig,
  CreateEmailConfig,
  Role,
} from '../interfaces/user.interface';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = environment.apiUrl || 'https://localhost:7294/api';

  constructor(private http: HttpClient) {}

  // Auth endpoints
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials);
  }

  changePassword(request: { currentPassword: string; newPassword: string; confirmPassword: string }): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/auth/change-password`, request);
  }

  forgotPassword(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/auth/forgot-password`, { email });
  }

  // Polizas endpoints
  getPolizas(): Observable<Poliza[]> {
    return this.http.get<Poliza[]>(`${this.apiUrl}/polizas`);
  }

  createPoliza(poliza: CreatePoliza): Observable<Poliza> {
    return this.http.post<Poliza>(`${this.apiUrl}/polizas`, poliza);
  }

  updatePoliza(id: number, poliza: Partial<CreatePoliza>): Observable<Poliza> {
    return this.http.put<Poliza>(`${this.apiUrl}/polizas/${id}`, poliza);
  }

  deletePoliza(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/polizas/${id}`);
  }

  // Upload endpoints
  uploadExcelPolizas(perfilId: number, file: File): Observable<DataUploadResult> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('perfilId', perfilId.toString());
    return this.http.post<DataUploadResult>(`${this.apiUrl}/polizas/upload`, formData);
  }

  downloadPolizasTemplate(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/polizas/template`, { responseType: 'blob' });
  }

  /**
   * Generates and downloads a correctable Excel file containing only the
   * records that failed validation in a previous upload. The file has the
   * same columns as the original upload file plus a trailing MOTIVO_ERROR
   * column so the user can fix and re-upload directly.
   */
  downloadPolizasErrorsExcel(payload: {
    fileHeaders: string[];
    failedRecords: FailedRecord[];
    originalFileName: string;
  }): Observable<Blob> {
    return this.http.post(
      `${this.apiUrl}/polizas/errors-excel`,
      payload,
      { responseType: 'blob' }
    );
  }

  // Users endpoints
  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/users`);
  }

  createUser(user: CreateUser): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/users`, user);
  }

  updateUser(id: number, user: UpdateUser): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/users/${id}`, user);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/users/${id}`);
  }

  // Cotizaciones endpoints
  getCotizaciones(): Observable<Cotizacion[]> {
    return this.http.get<Cotizacion[]>(`${this.apiUrl}/cotizaciones`);
  }

  createCotizacion(cotizacion: CreateCotizacion): Observable<Cotizacion> {
    return this.http.post<Cotizacion>(`${this.apiUrl}/cotizaciones`, cotizacion);
  }

  updateCotizacion(id: number, cotizacion: Partial<CreateCotizacion>): Observable<Cotizacion> {
    return this.http.put<Cotizacion>(`${this.apiUrl}/cotizaciones/${id}`, cotizacion);
  }

  deleteCotizacion(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/cotizaciones/${id}`);
  }

  // Cobros endpoints
  getCobros(): Observable<Cobro[]> {
    return this.http.get<Cobro[]>(`${this.apiUrl}/cobros`);
  }

  getCobroById(id: number): Observable<Cobro> {
    return this.http.get<Cobro>(`${this.apiUrl}/cobros/${id}`);
  }

  createCobro(cobro: CreateCobro): Observable<Cobro> {
    return this.http.post<Cobro>(`${this.apiUrl}/cobros`, cobro);
  }

  registrarCobro(id: number, data: RegistrarCobroRequest): Observable<Cobro> {
    return this.http.put<Cobro>(`${this.apiUrl}/cobros/${id}/registrar`, data);
  }

  cancelarCobro(id: number, data: CancelarCobroRequest): Observable<Cobro> {
    return this.http.put<Cobro>(`${this.apiUrl}/cobros/${id}/cancelar`, data);
  }

  getCobroStats(): Observable<CobroStats> {
    return this.http.get<CobroStats>(`${this.apiUrl}/cobros/stats`);
  }

  // Reclamos endpoints
  getReclamos(): Observable<Reclamo[]> {
    return this.http.get<Reclamo[]>(`${this.apiUrl}/reclamos`);
  }

  getReclamoById(id: number): Observable<Reclamo> {
    return this.http.get<Reclamo>(`${this.apiUrl}/reclamos/${id}`);
  }

  getReclamosByFiltro(params: ReclamoFilterParams): Observable<PagedResult<Reclamo>> {
    return this.http.get<PagedResult<Reclamo>>(`${this.apiUrl}/reclamos/filtro`, { params: params as Record<string, string | number> });
  }

  createReclamo(reclamo: CreateReclamo): Observable<Reclamo> {
    return this.http.post<Reclamo>(`${this.apiUrl}/reclamos`, reclamo);
  }

  updateReclamo(id: number, reclamo: Partial<CreateReclamo>): Observable<Reclamo> {
    return this.http.put<Reclamo>(`${this.apiUrl}/reclamos/${id}`, reclamo);
  }

  deleteReclamo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/reclamos/${id}`);
  }

  changeEstadoReclamo(id: number, data: ChangeEstadoReclamoRequest): Observable<Reclamo> {
    return this.http.put<Reclamo>(`${this.apiUrl}/reclamos/${id}/estado`, data);
  }

  asignarReclamo(id: number, data: AsignarReclamoRequest): Observable<Reclamo> {
    return this.http.put<Reclamo>(`${this.apiUrl}/reclamos/${id}/asignar`, data);
  }

  resolverReclamo(id: number, data: ResolverReclamoRequest): Observable<Reclamo> {
    return this.http.put<Reclamo>(`${this.apiUrl}/reclamos/${id}/resolver`, data);
  }

  rechazarReclamo(id: number, data: RechazarReclamoRequest): Observable<Reclamo> {
    return this.http.put<Reclamo>(`${this.apiUrl}/reclamos/${id}/rechazar`, data);
  }

  getReclamoStats(): Observable<ReclamoStats> {
    return this.http.get<ReclamoStats>(`${this.apiUrl}/reclamos/stats`);
  }

  // Email Config endpoints
  getEmailConfigs(): Observable<EmailConfig[]> {
    return this.http.get<EmailConfig[]>(`${this.apiUrl}/emailconfig`);
  }

  createEmailConfig(config: CreateEmailConfig): Observable<EmailConfig> {
    return this.http.post<EmailConfig>(`${this.apiUrl}/emailconfig`, config);
  }

  updateEmailConfig(id: number, config: Partial<CreateEmailConfig>): Observable<EmailConfig> {
    return this.http.put<EmailConfig>(`${this.apiUrl}/emailconfig/${id}`, config);
  }

  deleteEmailConfig(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/emailconfig/${id}`);
  }

  testEmailConfig(id: number): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.apiUrl}/emailconfig/${id}/test`, {});
  }

  testEmailConfigDirect(config: CreateEmailConfig): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.apiUrl}/emailconfig/test-direct`, config);
  }

  setDefaultEmailConfig(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/emailconfig/${id}/set-default`, {});
  }

  toggleEmailConfigStatus(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/emailconfig/${id}/toggle-status`, {});
  }

  // Roles endpoints
  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(`${this.apiUrl}/roles`);
  }

  createRole(role: Partial<Role>): Observable<Role> {
    return this.http.post<Role>(`${this.apiUrl}/roles`, role);
  }

  updateRole(id: number, role: Partial<Role>): Observable<Role> {
    return this.http.put<Role>(`${this.apiUrl}/roles/${id}`, role);
  }

  deleteRole(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/roles/${id}`);
  }
}
