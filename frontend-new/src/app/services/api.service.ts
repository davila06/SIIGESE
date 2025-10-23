import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { 
  User, 
  CreateUser,
  UpdateUser,
  Role,
  LoginRequest, 
  LoginResponse, 
  Cliente, 
  CreateCliente, 
  DataUploadResult,
  Poliza,
  CreatePoliza
} from '../interfaces/user.interface';
import { ResponseMapper } from '../utils/response-mapper';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Métodos de autenticación
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(catchError(this.handleError));
  }

  refreshToken(refreshToken: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(catchError(this.handleError));
  }

  // Métodos de usuarios
  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/users`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  getUserById(id: number): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/users/${id}`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  createUser(user: CreateUser): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/users`, user, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  updateUser(id: number, user: UpdateUser): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/users/${id}`, user, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/users/${id}`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  // Métodos de roles
  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(`${this.apiUrl}/roles`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  assignRolesToUser(userId: number, roleIds: number[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/users/${userId}/roles`, { roleIds }, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  // Métodos de clientes
  getClientes(): Observable<Cliente[]> {
    return this.http.get<Cliente[]>(`${this.apiUrl}/clientes`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  createCliente(cliente: CreateCliente): Observable<Cliente> {
    return this.http.post<Cliente>(`${this.apiUrl}/clientes`, cliente, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  uploadExcel(perfilId: number, file: File): Observable<DataUploadResult> {
    const formData = new FormData();
    formData.append('perfilId', perfilId.toString());
    formData.append('file', file);

    const token = localStorage.getItem('sinseg_token');
    let headers = new HttpHeaders();
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return this.http.post<DataUploadResult>(
      `${this.apiUrl}/clientes/upload`,
      formData,
      { headers }
    ).pipe(catchError(this.handleError));
  }

  // Métodos de pólizas
  getPolizas(): Observable<Poliza[]> {
    return this.http.get<Poliza[]>(`${this.apiUrl}/polizas`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  getPolizasByPerfil(perfilId: number): Observable<Poliza[]> {
    return this.http.get<Poliza[]>(`${this.apiUrl}/polizas/perfil/${perfilId}`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  getPolizaById(id: number): Observable<Poliza> {
    return this.http.get<Poliza>(`${this.apiUrl}/polizas/${id}`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  getPolizaByNumero(numeroPoliza: string): Observable<Poliza> {
    return this.http.get<Poliza>(`${this.apiUrl}/polizas/numero/${numeroPoliza}`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  getPolizasByAseguradora(aseguradora: string): Observable<Poliza[]> {
    return this.http.get<Poliza[]>(`${this.apiUrl}/polizas/aseguradora/${aseguradora}`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  createPoliza(poliza: CreatePoliza): Observable<Poliza> {
    return this.http.post<Poliza>(`${this.apiUrl}/polizas`, poliza, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  updatePoliza(id: number, poliza: CreatePoliza): Observable<Poliza> {
    return this.http.put<Poliza>(`${this.apiUrl}/polizas/${id}`, poliza, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  deletePoliza(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/polizas/${id}`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  uploadExcelPolizas(perfilId: number, file: File): Observable<DataUploadResult> {
    const formData = new FormData();
    formData.append('perfilId', perfilId.toString());
    formData.append('file', file);

    const token = localStorage.getItem('sinseg_token');
    let headers = new HttpHeaders();
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return this.http.post<any>(
      `${this.apiUrl}/polizas/upload`,
      formData,
      { headers }
    ).pipe(
      map(response => {
        console.log('🔄 Raw API response:', response);
        return ResponseMapper.mapDataUploadResult(response);
      }),
      catchError(this.handleError)
    );
  }

  downloadPolizasTemplate(): Observable<Blob> {
    const token = localStorage.getItem('sinseg_token');
    let headers = new HttpHeaders();
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return this.http.get(
      `${this.apiUrl}/polizas/template`,
      { 
        headers,
        responseType: 'blob'
      }
    ).pipe(catchError(this.handleError));
  }

  private getHttpOptions() {
    const token = localStorage.getItem('sinseg_token');
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        ...(token && { 'Authorization': `Bearer ${token}` })
      })
    };
  }

  private handleError(error: any): Observable<never> {
    console.error('API Error:', error);
    let errorMessage = 'Ha ocurrido un error inesperado';
    
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else if (error.status) {
      switch (error.status) {
        case 401:
          errorMessage = error.error?.message || 'No autorizado. Credenciales inválidas.';
          break;
        case 403:
          errorMessage = 'No tiene permisos para realizar esta acción.';
          break;
        case 404:
          errorMessage = 'Recurso no encontrado.';
          break;
        case 500:
          errorMessage = 'Error interno del servidor.';
          break;
        default:
          errorMessage = error.error?.message || `Error: ${error.status} - ${error.statusText}`;
      }
    }

    return throwError(() => new Error(errorMessage));
  }
}