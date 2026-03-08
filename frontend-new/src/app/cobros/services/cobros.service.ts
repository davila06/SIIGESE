import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
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

  // Obtener todos los cobros
  getCobros(): Observable<Cobro[]> {
    return this.http.get<Cobro[]>(this.apiUrl);
  }

  // Obtener cobros próximos basados en periodicidad:
  // - Mensual: siempre listados
  // - Otras periodicidades: dentro del próximo mes
  getCobrosProximos(): Observable<Cobro[]> {
    return this.http.get<Cobro[]>(`${this.apiUrl}/proximos`);
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

  /*
  // Métodos helper para datos mock (mientras no haya backend)
  getMockCobros(): Cobro[] {
    return [
      {
        id: 1,
        numeroRecibo: 'REC-001',
        polizaId: 1,
        numeroPoliza: 'POL-001',
        clienteNombre: 'Juan',
        clienteApellido: 'Pérez',
        fechaVencimiento: new Date('2025-10-15'),
        montoTotal: 2500.00,
        estado: EstadoCobro.Pendiente,
        fechaCreacion: new Date('2025-10-01')
      },
      {
        id: 2,
        numeroRecibo: 'REC-002',
        polizaId: 2,
        numeroPoliza: 'POL-002',
        clienteNombre: 'María',
        clienteApellido: 'González',
        fechaVencimiento: new Date('2025-09-30'),
        fechaCobro: new Date('2025-09-28'),
        montoTotal: 1800.00,
        montoCobrado: 1800.00,
        estado: EstadoCobro.Cobrado,
        metodoPago: MetodoPago.Transferencia,
        usuarioCobroNombre: 'Admin',
        fechaCreacion: new Date('2025-09-15')
      },
      {
        id: 3,
        numeroRecibo: 'REC-003',
        polizaId: 3,
        numeroPoliza: 'POL-003',
        clienteNombre: 'Carlos',
        clienteApellido: 'Rodríguez',
        fechaVencimiento: new Date('2025-09-20'),
        montoTotal: 3200.00,
        estado: EstadoCobro.Vencido,
        fechaCreacion: new Date('2025-09-01')
      }
    ];
  }

  getMockStats(): CobroStats {
    return {
      totalPendientes: 15,
      totalCobrados: 28,
      totalVencidos: 5,
      montoTotalPendiente: 45000.00,
      montoTotalCobrado: 128000.00,
      montoPorVencer: 22000.00
    };
  }
  */
}