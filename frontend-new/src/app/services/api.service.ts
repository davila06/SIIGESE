import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DataUploadResult, LoginRequest, LoginResponse, ApiResponse } from '../interfaces/user.interface';

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

  // Polizas endpoints
  getPolizas(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/polizas`);
  }

  createPoliza(poliza: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/polizas`, poliza);
  }

  updatePoliza(id: number, poliza: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/polizas/${id}`, poliza);
  }

  deletePoliza(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/polizas/${id}`);
  }

  // Upload endpoints
  uploadExcelPolizas(perfilId: number, file: File): Observable<DataUploadResult> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('perfilId', perfilId.toString());

    return this.http.post<DataUploadResult>(`${this.apiUrl}/polizas/upload`, formData);
  }

  downloadPolizasTemplate(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/polizas/template`, {
      responseType: 'blob'
    });
  }

  // Users endpoints
  getUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/users`);
  }

  createUser(user: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/users`, user);
  }

  updateUser(id: number, user: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/users/${id}`, user);
  }

  deleteUser(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/users/${id}`);
  }

  // Cotizaciones endpoints
  getCotizaciones(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/cotizaciones`);
  }

  createCotizacion(cotizacion: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/cotizaciones`, cotizacion);
  }

  updateCotizacion(id: number, cotizacion: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/cotizaciones/${id}`, cotizacion);
  }

  deleteCotizacion(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/cotizaciones/${id}`);
  }

  // Cobros endpoints
  getCobros(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/cobros`);
  }

  getCobroById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/cobros/${id}`);
  }

  createCobro(cobro: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/cobros`, cobro);
  }

  registrarCobro(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/cobros/${id}/registrar`, data);
  }

  cancelarCobro(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/cobros/${id}/cancelar`, data);
  }

  // Reclamos endpoints
  getReclamos(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/reclamos`);
  }

  getReclamoById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/reclamos/${id}`);
  }

  createReclamo(reclamo: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/reclamos`, reclamo);
  }

  updateReclamo(id: number, reclamo: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/reclamos/${id}`, reclamo);
  }

  deleteReclamo(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/reclamos/${id}`);
  }

  changeEstadoReclamo(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/reclamos/${id}/estado`, data);
  }

  asignarReclamo(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/reclamos/${id}/asignar`, data);
  }

  resolverReclamo(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/reclamos/${id}/resolver`, data);
  }

  rechazarReclamo(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/reclamos/${id}/rechazar`, data);
  }

  // Email Config endpoints
  getEmailConfigs(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/emailconfig`);
  }

  createEmailConfig(config: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/emailconfig`, config);
  }

  updateEmailConfig(id: number, config: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/emailconfig/${id}`, config);
  }

  deleteEmailConfig(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/emailconfig/${id}`);
  }

  testEmailConfig(id: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/emailconfig/${id}/test`, {});
  }

  testEmailConfigDirect(config: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/emailconfig/test-direct`, config);
  }

  setDefaultEmailConfig(id: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/emailconfig/${id}/set-default`, {});
  }

  toggleEmailConfigStatus(id: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/emailconfig/${id}/toggle-status`, {});
  }

  // Roles endpoints
  getRoles(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/roles`);
  }

  createRole(role: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/roles`, role);
  }

  updateRole(id: number, role: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/roles/${id}`, role);
  }

  deleteRole(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/roles/${id}`);
  }

  assignRolesToUser(userId: number, roleIds: number[]): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/users/${userId}/roles`, { roleIds });
  }

  // Stats endpoints
  getCobroStats(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/cobros/stats`);
  }

  getReclamosStats(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/reclamos/stats`);
  }
}