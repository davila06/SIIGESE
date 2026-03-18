import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { parseBackendDate } from '../../shared/constants/currency.constants';
import { 
  Cobro, 
  CobroRequest, 
  RegistrarCobroRequest, 
  CobroStats,
  EstadoCobro,
  MetodoPago,
  GenerarCobrosResult
} from '../interfaces/cobro.interface';

@Injectable({
  providedIn: 'root'
})
export class CobrosService {
  private readonly apiUrl = `${environment.apiUrl}/cobros`;

  constructor(private http: HttpClient) { }

  /** Normaliza un cobro recibido del API: convierte DateTime.MinValue ("0001-...") en undefined */
  private normalizeCobro(cobro: Cobro): Cobro {
    const fechaCobro = cobro.fechaCobro ? parseBackendDate(cobro.fechaCobro) : undefined;
    return {
      ...cobro,
      fechaCobro: (fechaCobro && fechaCobro.getFullYear() > 1) ? fechaCobro : undefined
    };
  }

  // Obtener todos los cobros
  getCobros(): Observable<Cobro[]> {
    return this.http.get<Cobro[]>(this.apiUrl).pipe(
      map(cobros => cobros.map(c => this.normalizeCobro(c)))
    );
  }

  // Obtener cobros próximos basados en periodicidad:
  // - Mensual: siempre listados
  // - Otras periodicidades: dentro del próximo mes
  getCobrosProximos(): Observable<Cobro[]> {
    return this.http.get<Cobro[]>(`${this.apiUrl}/proximos`).pipe(
      map(cobros => cobros.map(c => this.normalizeCobro(c)))
    );
  }

  // Obtener cobro por ID
  getCobro(id: number): Observable<Cobro> {
    return this.http.get<Cobro>(`${this.apiUrl}/${id}`);
  }

  // Crear nuevo cobro
  createCobro(cobro: CobroRequest): Observable<Cobro> {
    return this.http.post<Cobro>(this.apiUrl, cobro);
  }

  // Registrar pago de cobro
  registrarCobro(request: RegistrarCobroRequest): Observable<Cobro> {
    return this.http.put<Cobro>(`${this.apiUrl}/${request.cobroId}/registrar`, request);
  }

  // Cancelar cobro
  cancelarCobro(id: number, motivo?: string): Observable<Cobro> {
    return this.http.put<Cobro>(`${this.apiUrl}/${id}/cancelar`, { motivo });
  }

  // Obtener estadísticas de cobros
  getCobroStats(): Observable<CobroStats> {
    return this.http.get<CobroStats>(`${this.apiUrl}/stats`);
  }

  // Obtener cobros por frecuencia de póliza (MENSUAL, TRIMESTRAL, SEMESTRAL, ANUAL, etc.)
  getCobrosByFrecuencia(frecuencia: string): Observable<Cobro[]> {
    return this.http.get<Cobro[]>(`${this.apiUrl}/frecuencia/${frecuencia}`).pipe(
      map(cobros => cobros.map(c => this.normalizeCobro(c)))
    );
  }

  // Obtener cobros por estado
  getCobrosByEstado(estado: EstadoCobro): Observable<Cobro[]> {
    return this.http.get<Cobro[]>(`${this.apiUrl}/estado/${estado}`);
  }

  // Obtener cobros pendientes
  getCobrosPendientes(): Observable<Cobro[]> {
    return this.getCobrosByEstado(EstadoCobro.Pendiente);
  }

  // Obtener cobros vencidos
  getCobrosVencidos(): Observable<Cobro[]> {
    return this.getCobrosByEstado(EstadoCobro.Vencido);
  }

  // Obtener cobros por rango de fechas
  getCobrosByFechas(fechaInicio: Date, fechaFin: Date): Observable<Cobro[]> {
    const params = {
      fechaInicio: fechaInicio.toISOString().split('T')[0],
      fechaFin: fechaFin.toISOString().split('T')[0]
    };
    return this.http.get<Cobro[]>(`${this.apiUrl}/rango-fechas`, { params });
  }

  // Generar cobros automáticamente para todas las pólizas activas
  generarCobrosAutomaticos(mesesAdelante: number = 3): Observable<GenerarCobrosResult> {
    return this.http.post<GenerarCobrosResult>(
      `${this.apiUrl}/generar-automaticos?mesesAdelante=${mesesAdelante}`, 
      {}
    );
  }

  // Generar cobros automáticamente para una póliza específica
  generarCobrosPorPoliza(polizaId: number, mesesAdelante: number = 3): Observable<GenerarCobrosResult> {
    return this.http.post<GenerarCobrosResult>(
      `${this.apiUrl}/generar-por-poliza/${polizaId}?mesesAdelante=${mesesAdelante}`, 
      {}
    );
  }

  // Enviar correo electrónico de notificación de cobro
  enviarEmailCobro(id: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/enviar-email`, {});
  }
}