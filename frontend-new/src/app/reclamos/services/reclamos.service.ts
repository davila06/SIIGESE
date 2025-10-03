import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { 
  Reclamo,
  ReclamoRequest,
  ActualizarEstadoRequest,
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
  getReclamos(filtros?: FiltroReclamos): Observable<Reclamo[]> {
    return this.http.get<Reclamo[]>(this.apiUrl, { params: filtros as any });
  }

  // Obtener reclamo por ID
  getReclamo(id: number): Observable<Reclamo> {
    return this.http.get<Reclamo>(`${this.apiUrl}/${id}`);
  }

  // Crear nuevo reclamo
  createReclamo(reclamo: ReclamoRequest): Observable<Reclamo> {
    return this.http.post<Reclamo>(this.apiUrl, reclamo);
  }

  // Actualizar estado del reclamo
  actualizarEstado(request: ActualizarEstadoRequest): Observable<Reclamo> {
    return this.http.put<Reclamo>(`${this.apiUrl}/${request.reclamoId}/estado`, request);
  }

  // Asignar reclamo a usuario
  asignarReclamo(reclamoId: number, usuarioId: string): Observable<Reclamo> {
    return this.http.put<Reclamo>(`${this.apiUrl}/${reclamoId}/asignar`, { usuarioId });
  }

  // Obtener estadísticas de reclamos
  getReclamosStats(): Observable<ReclamosStats> {
    return this.http.get<ReclamosStats>(`${this.apiUrl}/stats`);
  }

  // Obtener reclamos por estado
  getReclamosByEstado(estado: EstadoReclamo): Observable<Reclamo[]> {
    return this.http.get<Reclamo[]>(`${this.apiUrl}/estado/${estado}`);
  }

  // Obtener reclamos por tipo
  getReclamosByTipo(tipo: TipoReclamo): Observable<Reclamo[]> {
    return this.http.get<Reclamo[]>(`${this.apiUrl}/tipo/${tipo}`);
  }

  // Obtener reclamos por prioridad
  getReclamosByPrioridad(prioridad: PrioridadReclamo): Observable<Reclamo[]> {
    return this.http.get<Reclamo[]>(`${this.apiUrl}/prioridad/${prioridad}`);
  }

  // Subir documento adjunto
  subirDocumento(reclamoId: number, archivo: File): Observable<any> {
    const formData = new FormData();
    formData.append('archivo', archivo);
    return this.http.post(`${this.apiUrl}/${reclamoId}/documentos`, formData);
  }

  // Métodos helper para datos mock (mientras no haya backend)
  getMockReclamos(): Reclamo[] {
    return [
      {
        id: 1,
        numeroReclamo: 'REC-2025-001',
        polizaId: 1,
        numeroPoliza: 'POL-001',
        clienteNombre: 'Juan',
        clienteApellido: 'Pérez',
        tipoReclamo: TipoReclamo.SINIESTRO,
        descripcion: 'Accidente vehicular en intersección principal. Daños en parte frontal del vehículo.',
        fechaReclamo: new Date('2025-10-01'),
        fechaOcurrencia: new Date('2025-09-28'),
        montoReclamado: 15000.00,
        estado: EstadoReclamo.EN_REVISION,
        prioridad: PrioridadReclamo.ALTA,
        asignadoA: 'María González',
        usuarioCreacion: 'Juan Pérez',
        fechaCreacion: new Date('2025-10-01')
      },
      {
        id: 2,
        numeroReclamo: 'REC-2025-002',
        polizaId: 2,
        numeroPoliza: 'POL-002',
        clienteNombre: 'Ana',
        clienteApellido: 'Martínez',
        tipoReclamo: TipoReclamo.REEMBOLSO,
        descripcion: 'Solicitud de reembolso por consulta médica especializada.',
        fechaReclamo: new Date('2025-09-25'),
        fechaOcurrencia: new Date('2025-09-20'),
        montoReclamado: 2500.00,
        montoAprobado: 2500.00,
        estado: EstadoReclamo.APROBADO,
        prioridad: PrioridadReclamo.MEDIA,
        asignadoA: 'Carlos Rodríguez',
        fechaResolucion: new Date('2025-09-30'),
        usuarioCreacion: 'Ana Martínez',
        fechaCreacion: new Date('2025-09-25')
      },
      {
        id: 3,
        numeroReclamo: 'REC-2025-003',
        polizaId: 3,
        numeroPoliza: 'POL-003',
        clienteNombre: 'Luis',
        clienteApellido: 'García',
        tipoReclamo: TipoReclamo.QUEJA_SERVICIO,
        descripcion: 'Queja por demora en respuesta de solicitud anterior.',
        fechaReclamo: new Date('2025-10-02'),
        estado: EstadoReclamo.PENDIENTE,
        prioridad: PrioridadReclamo.BAJA,
        usuarioCreacion: 'Luis García',
        fechaCreacion: new Date('2025-10-02')
      },
      {
        id: 4,
        numeroReclamo: 'REC-2025-004',
        polizaId: 4,
        numeroPoliza: 'POL-004',
        clienteNombre: 'Carmen',
        clienteApellido: 'López',
        tipoReclamo: TipoReclamo.SINIESTRO,
        descripcion: 'Robo de vehículo en estacionamiento del centro comercial.',
        fechaReclamo: new Date('2025-09-30'),
        fechaOcurrencia: new Date('2025-09-29'),
        montoReclamado: 35000.00,
        estado: EstadoReclamo.REQUIERE_DOCUMENTOS,
        prioridad: PrioridadReclamo.URGENTE,
        asignadoA: 'María González',
        observaciones: 'Se requiere informe policial y fotos del lugar.',
        usuarioCreacion: 'Carmen López',
        fechaCreacion: new Date('2025-09-30')
      },
      {
        id: 5,
        numeroReclamo: 'REC-2025-005',
        polizaId: 5,
        numeroPoliza: 'POL-005',
        clienteNombre: 'Roberto',
        clienteApellido: 'Sánchez',
        tipoReclamo: TipoReclamo.CANCELACION,
        descripcion: 'Solicitud de cancelación de póliza por cambio de residencia.',
        fechaReclamo: new Date('2025-09-20'),
        estado: EstadoReclamo.RESUELTO,
        prioridad: PrioridadReclamo.MEDIA,
        asignadoA: 'Carlos Rodríguez',
        fechaResolucion: new Date('2025-09-25'),
        usuarioCreacion: 'Roberto Sánchez',
        fechaCreacion: new Date('2025-09-20')
      }
    ];
  }

  getMockStats(): ReclamosStats {
    return {
      totalPendientes: 8,
      totalEnRevision: 12,
      totalAprobados: 25,
      totalRechazados: 3,
      totalResueltos: 45,
      montoTotalReclamado: 850000.00,
      montoTotalAprobado: 680000.00,
      tiempoPromedioResolucion: 7.5
    };
  }
}