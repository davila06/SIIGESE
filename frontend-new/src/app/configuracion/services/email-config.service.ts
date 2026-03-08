import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  EmailConfig, 
  EmailConfigCreate, 
  EmailConfigUpdate, 
  EmailTestRequest, 
  EmailConfigTestRequest,
  EmailTestResponse, 
  ApiResponse 
} from '../models/email-config.model';@Injectable({
  providedIn: 'root'
})
export class EmailConfigService {
  private readonly apiUrl = `${environment.apiUrl}/emailconfig`;

  constructor(private http: HttpClient) {}

  // Obtener todas las configuraciones
  getAll(): Observable<ApiResponse<EmailConfig[]>> {
    return this.http.get<ApiResponse<EmailConfig[]>>(this.apiUrl);
  }

  // Obtener configuración por ID
  getById(id: number): Observable<ApiResponse<EmailConfig>> {
    return this.http.get<ApiResponse<EmailConfig>>(`${this.apiUrl}/${id}`);
  }

  // Obtener configuración por defecto
  getDefault(): Observable<ApiResponse<EmailConfig>> {
    return this.http.get<ApiResponse<EmailConfig>>(`${this.apiUrl}/default`);
  }

  // Crear nueva configuración
  create(config: EmailConfigCreate): Observable<ApiResponse<EmailConfig>> {
    return this.http.post<ApiResponse<EmailConfig>>(this.apiUrl, config);
  }

  // Actualizar configuración
  update(id: number, config: EmailConfigUpdate): Observable<ApiResponse<EmailConfig>> {
    return this.http.put<ApiResponse<EmailConfig>>(`${this.apiUrl}/${id}`, config);
  }

  // Eliminar configuración
  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  // Establecer como predeterminada
  setAsDefault(id: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/${id}/set-default`, {});
  }

  // Cambiar estado activo/inactivo
  toggleActiveStatus(id: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/${id}/toggle-status`, {});
  }

  // Probar configuración
  testConfiguration(testRequest: EmailTestRequest): Observable<ApiResponse<EmailTestResponse>> {
    return this.http.post<ApiResponse<EmailTestResponse>>(`${this.apiUrl}/test`, testRequest);
  }

  // Probar configuración directa (sin guardar)
  testConfigurationDirect(testRequest: EmailConfigTestRequest): Observable<ApiResponse<EmailTestResponse>> {
    return this.http.post<ApiResponse<EmailTestResponse>>(`${this.apiUrl}/test-direct`, testRequest);
  }
}