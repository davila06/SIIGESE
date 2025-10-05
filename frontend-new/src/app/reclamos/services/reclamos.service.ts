import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { 
  Reclamo,
  CreateReclamoDto,
  UpdateReclamoDto,
  ReclamosStats,
  FiltroReclamos,
  TipoReclamo,
  EstadoReclamo,
  PrioridadReclamo
} from '../interfaces/reclamo.interface';

@Injectable({
  providedIn: 'root'
})
export class ReclamosService {
  private readonly apiUrl = `${environment.apiUrl}/reclamos`;

  constructor(private http: HttpClient) { }

  // Obtener todos los reclamos
  getReclamos(): Observable<any> {
    return this.http.get<any>(this.apiUrl);
  }

  // Obtener reclamo por ID
  getReclamo(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  // Crear nuevo reclamo
  createReclamo(reclamo: CreateReclamoDto): Observable<any> {
    return this.http.post<any>(this.apiUrl, reclamo);
  }

  // Actualizar reclamo
  updateReclamo(id: number, reclamo: UpdateReclamoDto): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, reclamo);
  }

  // Eliminar reclamo
  deleteReclamo(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  // Filtrar reclamos
  filtrarReclamos(filtros: FiltroReclamos): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/filtrar`, filtros);
  }

  // Obtener estadísticas de reclamos
  getReclamosStats(): Observable<ReclamosStats> {
    return this.http.get<ReclamosStats>(`${this.apiUrl}/stats`);
  }

  // Asignar reclamo a usuario
  asignarReclamo(id: number, usuarioId: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/asignar`, { usuarioId });
  }

  // Cambiar estado del reclamo
  cambiarEstado(id: number, estado: number, observaciones?: string): Observable<any> {
    const body = { estado, observaciones };
    return this.http.put<any>(`${this.apiUrl}/${id}/estado`, body);
  }

  // Resolver reclamo
  resolverReclamo(id: number, montoAprobado?: number, observaciones?: string): Observable<any> {
    const body = { montoAprobado, observaciones };
    return this.http.put<any>(`${this.apiUrl}/${id}/resolver`, body);
  }

  // Rechazar reclamo
  rechazarReclamo(id: number, observaciones: string): Observable<any> {
    const body = { observaciones };
    return this.http.put<any>(`${this.apiUrl}/${id}/rechazar`, body);
  }

  // Obtener reclamos por póliza
  getReclamosByPoliza(polizaId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/poliza/${polizaId}`);
  }

  // Obtener reclamos por usuario
  getReclamosByUsuario(usuarioId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/usuario/${usuarioId}`);
  }

  // Helper methods para obtener labels
  getTipoReclamoLabel(tipo: TipoReclamo): string {
    switch (tipo) {
      case TipoReclamo.Siniestro: return 'Siniestro';
      case TipoReclamo.Servicio: return 'Servicio';
      case TipoReclamo.Facturacion: return 'Facturación';
      case TipoReclamo.Cobertura: return 'Cobertura';
      case TipoReclamo.Proceso: return 'Proceso';
      case TipoReclamo.Otro: return 'Otro';
      default: return 'Desconocido';
    }
  }

  getEstadoReclamoLabel(estado: EstadoReclamo): string {
    switch (estado) {
      case EstadoReclamo.Abierto: return 'Abierto';
      case EstadoReclamo.EnProceso: return 'En Proceso';
      case EstadoReclamo.Resuelto: return 'Resuelto';
      case EstadoReclamo.Cerrado: return 'Cerrado';
      case EstadoReclamo.Rechazado: return 'Rechazado';
      case EstadoReclamo.Escalado: return 'Escalado';
      default: return 'Desconocido';
    }
  }

  getPrioridadReclamoLabel(prioridad: PrioridadReclamo): string {
    switch (prioridad) {
      case PrioridadReclamo.Baja: return 'Baja';
      case PrioridadReclamo.Media: return 'Media';
      case PrioridadReclamo.Alta: return 'Alta';
      case PrioridadReclamo.Critica: return 'Crítica';
      default: return 'Desconocido';
    }
  }

  // Helper para obtener clase CSS según el estado
  getEstadoClass(estado: EstadoReclamo): string {
    switch (estado) {
      case EstadoReclamo.Abierto: return 'estado-abierto';
      case EstadoReclamo.EnProceso: return 'estado-proceso';
      case EstadoReclamo.Resuelto: return 'estado-resuelto';
      case EstadoReclamo.Cerrado: return 'estado-cerrado';
      case EstadoReclamo.Rechazado: return 'estado-rechazado';
      case EstadoReclamo.Escalado: return 'estado-escalado';
      default: return '';
    }
  }

  // Helper para obtener clase CSS según la prioridad
  getPrioridadClass(prioridad: PrioridadReclamo): string {
    switch (prioridad) {
      case PrioridadReclamo.Baja: return 'prioridad-baja';
      case PrioridadReclamo.Media: return 'prioridad-media';
      case PrioridadReclamo.Alta: return 'prioridad-alta';
      case PrioridadReclamo.Critica: return 'prioridad-critica';
      default: return '';
    }
  }
}
