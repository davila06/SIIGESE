import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError } from 'rxjs';
import { CreateCotizacion, UpdateCotizacion, Cotizacion, CotizacionSearch } from '../models/cotizacion.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CotizacionService {
  private apiUrl = `${environment.apiUrl}/cotizaciones`;

  constructor(private http: HttpClient) { }

  private getHttpOptions() {
    const token = localStorage.getItem('token');
    return {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      }
    };
  }

  private handleError = (error: any): Observable<never> => {
    console.error('Error en CotizacionService:', error);
    throw error;
  };

  // Obtener todas las cotizaciones
  getCotizaciones(): Observable<Cotizacion[]> {
    return this.http.get<Cotizacion[]>(this.apiUrl, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  // Obtener cotización por ID
  getCotizacion(id: number): Observable<Cotizacion> {
    return this.http.get<Cotizacion>(`${this.apiUrl}/${id}`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  // Crear nueva cotización
  createCotizacion(cotizacion: CreateCotizacion): Observable<Cotizacion> {
    return this.http.post<Cotizacion>(this.apiUrl, cotizacion, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  // Actualizar cotización
  updateCotizacion(id: number, cotizacion: UpdateCotizacion): Observable<Cotizacion> {
    return this.http.put<Cotizacion>(`${this.apiUrl}/${id}`, cotizacion, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  // Eliminar cotización
  deleteCotizacion(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  // Buscar cotizaciones con filtros
  searchCotizaciones(searchParams: CotizacionSearch): Observable<Cotizacion[]> {
    return this.http.post<Cotizacion[]>(`${this.apiUrl}/search`, searchParams, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  // Actualizar estado de cotización
  updateEstado(id: number, estado: string): Observable<Cotizacion> {
    return this.http.patch<Cotizacion>(`${this.apiUrl}/${id}/estado`, estado, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }

  // Obtener mis cotizaciones (usuario actual)
  getMisCotizaciones(): Observable<Cotizacion[]> {
    return this.http.get<Cotizacion[]>(`${this.apiUrl}/mis-cotizaciones`, this.getHttpOptions())
      .pipe(catchError(this.handleError));
  }
}
