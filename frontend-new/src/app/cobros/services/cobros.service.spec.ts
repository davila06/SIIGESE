/**
 * @file cobros.service.spec.ts
 * @description Unit tests for CobrosService.
 *
 * Strategy: HttpClientTestingModule intercepts all HTTP calls so no real
 * network requests are made.  Each test flushes a mock response and asserts
 * on the returned data, HTTP method, and URL.
 */
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { environment } from '../../../environments/environment';

import { CobrosService } from './cobros.service';
import {
  Cobro,
  CobroRequest,
  CobroStats,
  EstadoCobro,
  GenerarCobrosResult,
  MetodoPago,
  RegistrarCobroRequest,
} from '../interfaces/cobro.interface';

const API = `${environment.apiUrl}/cobros`;

// ─── Fixtures ───────────────────────────────────────────────────────────────
const MOCK_COBRO: Cobro = {
  id: 1,
  numeroRecibo: 'REC-001',
  polizaId: 1,
  numeroPoliza: 'POL-001',
  clienteNombreCompleto: 'Juan Pérez',
  montoTotal: 2_500,
  estado: EstadoCobro.Pendiente,
  fechaVencimiento: new Date('2025-12-31T00:00:00'),
  fechaCreacion: new Date('2025-01-01T00:00:00'),
};

const MOCK_STATS: CobroStats = {
  totalPendientes: 10,
  totalCobrados: 5,
  totalVencidos: 2,
  montoTotalPendiente: 50_000,
  montoTotalCobrado: 25_000,
  montoPorVencer: 10_000,
};

// ════════════════════════════════════════════════════════════════════════════
describe('CobrosService', () => {
  let service: CobrosService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service  = TestBed.inject(CobrosService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify()); // ensures no unexpected HTTP calls remain

  // ── getCobros ────────────────────────────────────────────────────────────
  describe('getCobros', () => {
    it('should send GET to /cobros and return a mapped array', () => {
      let result: Cobro[] | undefined;

      service.getCobros().subscribe(c => (result = c));
      httpMock.expectOne(API).flush([MOCK_COBRO]);

      expect(result!.length).toBe(1);
      expect(result![0].id).toBe(1);
      expect(result![0].clienteNombreCompleto).toBe('Juan Pérez');
    });

    it('should normalize a valid fechaCobro string to a Date instance', () => {
      const raw = { ...MOCK_COBRO, fechaCobro: '2025-11-15T10:00:00' };
      let result: Cobro[] | undefined;

      service.getCobros().subscribe(c => (result = c));
      httpMock.expectOne(API).flush([raw]);

      expect(result![0].fechaCobro).toBeInstanceOf(Date);
      expect((result![0].fechaCobro as Date).getFullYear()).toBe(2025);
    });

    it('should normalize .NET DateTime.MinValue ("0001-01-01T...") fechaCobro to undefined', () => {
      const raw = { ...MOCK_COBRO, fechaCobro: '0001-01-01T00:00:00' };
      let result: Cobro[] | undefined;

      service.getCobros().subscribe(c => (result = c));
      httpMock.expectOne(API).flush([raw]);

      expect(result![0].fechaCobro).toBeUndefined();
    });

    it('should normalize a null fechaCobro to undefined', () => {
      const raw = { ...MOCK_COBRO, fechaCobro: null };
      let result: Cobro[] | undefined;

      service.getCobros().subscribe(c => (result = c));
      httpMock.expectOne(API).flush([raw]);

      expect(result![0].fechaCobro).toBeUndefined();
    });
  });

  // ── getCobrosProximos ────────────────────────────────────────────────────
  describe('getCobrosProximos', () => {
    it('should send GET to /cobros/proximos', () => {
      service.getCobrosProximos().subscribe();
      const req = httpMock.expectOne(`${API}/proximos`);
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });

  // ── getCobro ─────────────────────────────────────────────────────────────
  describe('getCobro', () => {
    it('should send GET to /cobros/:id and return the cobro', () => {
      let result: Cobro | undefined;

      service.getCobro(1).subscribe(c => (result = c));
      httpMock.expectOne(`${API}/1`).flush(MOCK_COBRO);

      expect(result!.id).toBe(1);
      expect(result!.numeroRecibo).toBe('REC-001');
    });
  });

  // ── createCobro ──────────────────────────────────────────────────────────
  describe('createCobro', () => {
    it('should send POST to /cobros and return the created cobro', () => {
      const request: CobroRequest = {
        polizaId: 1,
        fechaVencimiento: new Date('2025-12-31'),
        montoTotal: 2_500,
        moneda: 'CRC',
      };
      let result: Cobro | undefined;

      service.createCobro(request).subscribe(c => (result = c));
      const req = httpMock.expectOne(API);

      expect(req.request.method).toBe('POST');
      req.flush({ ...MOCK_COBRO, id: 10 });

      expect(result!.id).toBe(10);
    });
  });

  // ── registrarCobro ───────────────────────────────────────────────────────
  describe('registrarCobro', () => {
    it('should send PUT to /cobros/:id/registrar and return the updated cobro', () => {
      const request: RegistrarCobroRequest = {
        cobroId: 1,
        fechaCobro: new Date(),
        montoCobrado: 2_500,
        metodoPago: MetodoPago.Transferencia,
      };
      let result: Cobro | undefined;

      service.registrarCobro(request).subscribe(c => (result = c));
      const req = httpMock.expectOne(`${API}/1/registrar`);

      expect(req.request.method).toBe('PUT');
      req.flush({ ...MOCK_COBRO, estado: EstadoCobro.Cobrado });

      expect(result!.estado).toBe(EstadoCobro.Cobrado);
    });
  });

  // ── cancelarCobro ────────────────────────────────────────────────────────
  describe('cancelarCobro', () => {
    it('should send PUT to /cobros/:id/cancelar including the motivo in the body', () => {
      let result: Cobro | undefined;

      service.cancelarCobro(1, 'Duplicado').subscribe(c => (result = c));
      const req = httpMock.expectOne(`${API}/1/cancelar`);

      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({ motivo: 'Duplicado' });
      req.flush({ ...MOCK_COBRO, estado: EstadoCobro.Cancelado });

      expect(result!.estado).toBe(EstadoCobro.Cancelado);
    });

    it('should send PUT with motivo: undefined when no reason is given', () => {
      service.cancelarCobro(2).subscribe();
      const req = httpMock.expectOne(`${API}/2/cancelar`);

      expect(req.request.body).toEqual({ motivo: undefined });
      req.flush({ ...MOCK_COBRO, id: 2, estado: EstadoCobro.Cancelado });
    });
  });

  // ── getCobroStats ─────────────────────────────────────────────────────────
  describe('getCobroStats', () => {
    it('should send GET to /cobros/stats and return statistics', () => {
      let result: CobroStats | undefined;

      service.getCobroStats().subscribe(s => (result = s));
      httpMock.expectOne(`${API}/stats`).flush(MOCK_STATS);

      expect(result!.totalPendientes).toBe(10);
      expect(result!.totalCobrados).toBe(5);
      expect(result!.montoTotalCobrado).toBe(25_000);
    });
  });

  // ── getCobrosByEstado ─────────────────────────────────────────────────────
  describe('getCobrosByEstado', () => {
    it('should send GET to /cobros/estado/:estado', () => {
      let result: Cobro[] | undefined;

      service.getCobrosByEstado(EstadoCobro.Pendiente).subscribe(c => (result = c));
      httpMock.expectOne(`${API}/estado/${EstadoCobro.Pendiente}`).flush([MOCK_COBRO]);

      expect(result!.length).toBe(1);
      expect(result![0].estado).toBe(EstadoCobro.Pendiente);
    });
  });

  // ── getCobrosPendientes (alias) ───────────────────────────────────────────
  describe('getCobrosPendientes', () => {
    it('should be a convenience alias for getCobrosByEstado(Pendiente)', () => {
      service.getCobrosPendientes().subscribe();
      const req = httpMock.expectOne(`${API}/estado/${EstadoCobro.Pendiente}`);
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });

  // ── getCobrosByFechas ─────────────────────────────────────────────────────
  describe('getCobrosByFechas', () => {
    it('should send GET to /cobros/rango-fechas with ISO date params', () => {
      const inicio = new Date('2025-01-01');
      const fin    = new Date('2025-12-31');

      service.getCobrosByFechas(inicio, fin).subscribe();
      const req = httpMock.expectOne(r => r.url === `${API}/rango-fechas`);

      expect(req.request.params.get('fechaInicio')).toBe('2025-01-01');
      expect(req.request.params.get('fechaFin')).toBe('2025-12-31');
      req.flush([]);
    });
  });

  // ── generarCobrosAutomaticos ──────────────────────────────────────────────
  describe('generarCobrosAutomaticos', () => {
    it('should POST to /cobros/generar-automaticos with default mesesAdelante=3', () => {
      const mockResult: GenerarCobrosResult = {
        cobrosGenerados: 5,
        polizasProcesadas: 10,
        polizasSaltadas: 1,
        errores: [],
        cobrosCreados: [],
      };

      service.generarCobrosAutomaticos().subscribe();
      const req = httpMock.expectOne(`${API}/generar-automaticos?mesesAdelante=3`);

      expect(req.request.method).toBe('POST');
      req.flush(mockResult);
    });
  });
});
