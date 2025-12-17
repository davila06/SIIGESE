import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { 
  Cobro, 
  CobroRequest, 
  RegistrarCobroRequest, 
  CobroStats,
  EstadoCobro,
  MetodoPago 
} from '../interfaces/cobro.interface';

@Injectable({
  providedIn: 'root'
})
export class MockCobrosService {
  private mockCobros: Cobro[] = [
    {
      id: 1,
      numeroRecibo: 'REC-2025-001',
      polizaId: 1,
      numeroPoliza: 'POL-2025-001',
      clienteNombreCompleto: 'Juan Carlos Pérez González',
      fechaVencimiento: new Date('2025-11-15'),
      montoTotal: 45000.00,
      estado: EstadoCobro.Pendiente,
      fechaCreacion: new Date('2025-10-01'),
      observaciones: 'Cobro mensual - Seguro de Vida'
    },
    {
      id: 2,
      numeroRecibo: 'REC-2025-002',
      polizaId: 2,
      numeroPoliza: 'POL-2025-002',
      clienteNombreCompleto: 'María Elena González Ramírez',
      fechaVencimiento: new Date('2025-10-30'),
      fechaCobro: new Date('2025-10-28'),
      montoTotal: 32500.00,
      montoCobrado: 32500.00,
      estado: EstadoCobro.Cobrado,
      metodoPago: MetodoPago.Transferencia,
      usuarioCobroNombre: 'Admin User',
      usuarioCobroId: 1,
      fechaCreacion: new Date('2025-09-15'),
      observaciones: 'Pago puntual - Seguro de Auto'
    },
    {
      id: 3,
      numeroRecibo: 'REC-2025-003',
      polizaId: 3,
      numeroPoliza: 'POL-2025-003',
      clienteNombreCompleto: 'Carlos Alberto Rodríguez Soto',
      fechaVencimiento: new Date('2025-09-20'),
      montoTotal: 67800.00,
      estado: EstadoCobro.Vencido,
      fechaCreacion: new Date('2025-08-01'),
      observaciones: 'Cobro vencido - Requiere seguimiento urgente'
    },
    {
      id: 4,
      numeroRecibo: 'REC-2025-004',
      polizaId: 4,
      numeroPoliza: 'POL-2025-004',
      clienteNombreCompleto: 'Ana Patricia Fernández Mora',
      fechaVencimiento: new Date('2025-11-20'),
      montoTotal: 28900.00,
      estado: EstadoCobro.Pendiente,
      fechaCreacion: new Date('2025-10-05'),
      observaciones: 'Seguro de Hogar - Primera cuota'
    },
    {
      id: 5,
      numeroRecibo: 'REC-2025-005',
      polizaId: 5,
      numeroPoliza: 'POL-2025-005',
      clienteNombreCompleto: 'Roberto Jiménez Castro',
      fechaVencimiento: new Date('2025-11-05'),
      fechaCobro: new Date('2025-11-04'),
      montoTotal: 54200.00,
      montoCobrado: 54200.00,
      estado: EstadoCobro.Cobrado,
      metodoPago: MetodoPago.TarjetaCredito,
      usuarioCobroNombre: 'Admin User',
      usuarioCobroId: 1,
      fechaCreacion: new Date('2025-10-01'),
      observaciones: 'Pago con tarjeta - Seguro Comercial'
    },
    {
      id: 6,
      numeroRecibo: 'REC-2025-006',
      polizaId: 6,
      numeroPoliza: 'POL-2025-006',
      clienteNombreCompleto: 'Sofía Vargas Núñez',
      fechaVencimiento: new Date('2025-08-15'),
      montoTotal: 41300.00,
      estado: EstadoCobro.Vencido,
      fechaCreacion: new Date('2025-07-10'),
      observaciones: 'Cobro vencido hace 2 meses - Cliente contactado'
    },
    {
      id: 7,
      numeroRecibo: 'REC-2025-007',
      polizaId: 7,
      numeroPoliza: 'POL-2025-007',
      clienteNombreCompleto: 'Luis Fernando Solís Quesada',
      fechaVencimiento: new Date('2025-11-25'),
      montoTotal: 73500.00,
      estado: EstadoCobro.Pendiente,
      fechaCreacion: new Date('2025-10-10'),
      observaciones: 'Seguro de Transporte - Carga pesada'
    },
    {
      id: 8,
      numeroRecibo: 'REC-2025-008',
      polizaId: 8,
      numeroPoliza: 'POL-2025-008',
      clienteNombreCompleto: 'Patricia Monge Villalobos',
      fechaVencimiento: new Date('2025-11-10'),
      fechaCobro: new Date('2025-11-08'),
      montoTotal: 19500.00,
      montoCobrado: 19500.00,
      estado: EstadoCobro.Cobrado,
      metodoPago: MetodoPago.Efectivo,
      usuarioCobroNombre: 'Admin User',
      usuarioCobroId: 1,
      fechaCreacion: new Date('2025-10-02'),
      observaciones: 'Pago en efectivo - Oficina central'
    },
    {
      id: 9,
      numeroRecibo: 'REC-2025-009',
      polizaId: 9,
      numeroPoliza: 'POL-2025-009',
      clienteNombreCompleto: 'Diego Andrés Campos Rojas',
      fechaVencimiento: new Date('2025-09-05'),
      montoTotal: 89200.00,
      estado: EstadoCobro.Vencido,
      fechaCreacion: new Date('2025-08-01'),
      observaciones: 'Cobro vencido - Plan de pago solicitado'
    },
    {
      id: 10,
      numeroRecibo: 'REC-2025-010',
      polizaId: 10,
      numeroPoliza: 'POL-2025-010',
      clienteNombreCompleto: 'Gabriela Araya Salazar',
      fechaVencimiento: new Date('2025-10-25'),
      fechaCobro: new Date('2025-10-23'),
      montoTotal: 36700.00,
      montoCobrado: 36700.00,
      estado: EstadoCobro.Cobrado,
      metodoPago: MetodoPago.TarjetaDebito,
      usuarioCobroNombre: 'Admin User',
      usuarioCobroId: 1,
      fechaCreacion: new Date('2025-09-20'),
      observaciones: 'Pago con débito automático'
    },
    {
      id: 11,
      numeroRecibo: 'REC-2025-011',
      polizaId: 11,
      numeroPoliza: 'POL-2025-011',
      clienteNombreCompleto: 'Ricardo Méndez Salas',
      fechaVencimiento: new Date('2025-12-01'),
      montoTotal: 52400.00,
      estado: EstadoCobro.Pendiente,
      fechaCreacion: new Date('2025-10-15'),
      observaciones: 'Seguro de Salud - Renovación anual'
    },
    {
      id: 12,
      numeroRecibo: 'REC-2025-012',
      polizaId: 12,
      numeroPoliza: 'POL-2025-012',
      clienteNombreCompleto: 'Valeria Herrera Brenes',
      fechaVencimiento: new Date('2025-09-10'),
      montoTotal: 61500.00,
      estado: EstadoCobro.Vencido,
      fechaCreacion: new Date('2025-08-05'),
      observaciones: 'Cobro vencido - Póliza suspendida temporalmente'
    }
  ];

  private nextId = 13;

  constructor() {
    console.log('🟢 MockCobrosService inicializado con', this.mockCobros.length, 'cobros');
  }

  // Obtener todos los cobros
  getCobros(): Observable<Cobro[]> {
    console.log('📊 MockCobrosService.getCobros() - Retornando', this.mockCobros.length, 'cobros');
    return of([...this.mockCobros]).pipe(delay(500));
  }

  // Obtener cobro por ID
  getCobro(id: number): Observable<Cobro> {
    console.log('🔍 MockCobrosService.getCobro() - ID:', id);
    const cobro = this.mockCobros.find(c => c.id === id);
    if (!cobro) {
      throw new Error(`Cobro con ID ${id} no encontrado`);
    }
    return of({...cobro}).pipe(delay(300));
  }

  // Crear nuevo cobro
  createCobro(cobro: CobroRequest): Observable<Cobro> {
    console.log('➕ MockCobrosService.createCobro()', cobro);
    const newCobro: Cobro = {
      id: this.nextId++,
      numeroRecibo: `REC-2025-${String(this.nextId).padStart(3, '0')}`,
      polizaId: cobro.polizaId,
      numeroPoliza: `POL-${cobro.polizaId}`,
      clienteNombreCompleto: 'Cliente Nuevo',
      fechaVencimiento: new Date(cobro.fechaVencimiento),
      montoTotal: cobro.montoTotal,
      estado: EstadoCobro.Pendiente,
      fechaCreacion: new Date(),
      observaciones: cobro.observaciones,
      moneda: cobro.moneda || 'CRC'
    };
    this.mockCobros.unshift(newCobro);
    return of({...newCobro}).pipe(delay(500));
  }

  // Registrar pago de cobro
  registrarCobro(request: RegistrarCobroRequest): Observable<Cobro> {
    console.log('💰 MockCobrosService.registrarCobro()', request);
    const cobro = this.mockCobros.find(c => c.id === request.cobroId);
    if (!cobro) {
      throw new Error(`Cobro con ID ${request.cobroId} no encontrado`);
    }

    cobro.estado = EstadoCobro.Cobrado;
    cobro.fechaCobro = new Date(request.fechaCobro);
    cobro.montoCobrado = request.montoCobrado;
    cobro.metodoPago = request.metodoPago;
    cobro.usuarioCobroNombre = 'Admin User';
    cobro.observaciones = request.observaciones || cobro.observaciones;
    cobro.fechaActualizacion = new Date();

    return of({...cobro}).pipe(delay(500));
  }

  // Cancelar cobro
  cancelarCobro(id: number, motivo?: string): Observable<Cobro> {
    console.log('❌ MockCobrosService.cancelarCobro() - ID:', id, 'Motivo:', motivo);
    const cobro = this.mockCobros.find(c => c.id === id);
    if (!cobro) {
      throw new Error(`Cobro con ID ${id} no encontrado`);
    }

    cobro.estado = EstadoCobro.Cancelado;
    cobro.observaciones = motivo ? `Cancelado: ${motivo}` : 'Cancelado';
    cobro.fechaActualizacion = new Date();

    return of({...cobro}).pipe(delay(500));
  }

  // Obtener estadísticas de cobros
  getCobroStats(): Observable<CobroStats> {
    console.log('📈 MockCobrosService.getCobroStats()');
    
    const pendientes = this.mockCobros.filter(c => c.estado === EstadoCobro.Pendiente);
    const cobrados = this.mockCobros.filter(c => c.estado === EstadoCobro.Cobrado);
    const vencidos = this.mockCobros.filter(c => c.estado === EstadoCobro.Vencido);

    const stats: CobroStats = {
      totalPendientes: pendientes.length,
      totalCobrados: cobrados.length,
      totalVencidos: vencidos.length,
      montoTotalPendiente: pendientes.reduce((sum, c) => sum + c.montoTotal, 0),
      montoTotalCobrado: cobrados.reduce((sum, c) => sum + (c.montoCobrado || 0), 0),
      montoPorVencer: pendientes
        .filter(c => new Date(c.fechaVencimiento) > new Date())
        .reduce((sum, c) => sum + c.montoTotal, 0)
    };

    console.log('📊 Stats:', stats);
    return of(stats).pipe(delay(400));
  }

  // Obtener cobros por estado
  getCobrosByEstado(estado: EstadoCobro): Observable<Cobro[]> {
    console.log('🔍 MockCobrosService.getCobrosByEstado() - Estado:', estado);
    const filtered = this.mockCobros.filter(c => c.estado === estado);
    return of([...filtered]).pipe(delay(400));
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
    console.log('📅 MockCobrosService.getCobrosByFechas()', fechaInicio, '-', fechaFin);
    const filtered = this.mockCobros.filter(c => {
      const fechaVenc = new Date(c.fechaVencimiento);
      return fechaVenc >= fechaInicio && fechaVenc <= fechaFin;
    });
    return of([...filtered]).pipe(delay(400));
  }
}
