/**
 * @file reclamos.service.spec.ts
 * @description Unit tests for ReclamosService.
 *
 * Strategy: HttpClientTestingModule intercepts all HTTP calls so no real
 * network requests are made.  Each test flushes a mock response and asserts
 * on the returned data, HTTP method, and URL.
 */
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { environment } from '../../../environments/environment';

import { ReclamosService } from './reclamos.service';
import {
  CreateReclamoDto,
  EstadoReclamo,
  FiltroReclamos,
  PrioridadReclamo,
  Reclamo,
  ReclamosStats,
  TipoReclamo,
  UpdateReclamoDto,
} from '../interfaces/reclamo.interface';
import { PagedResult } from '../../interfaces/user.interface';

const API = `${environment.apiUrl}/reclamos`;

// ─── Fixtures ───────────────────────────────────────────────────────────────
const MOCK_RECLAMO: Reclamo = {
  id: 1,
  numeroReclamo: 'REC-2025-001',
  polizaId: 1,
  numeroPoliza: 'POL-001',
  clienteNombreCompleto: 'María González',
  fechaReclamo: '2025-03-01T00:00:00',
  tipoReclamo: TipoReclamo.Siniestro,
  estado: EstadoReclamo.Abierto,
  descripcion: 'Accidente vehículo',
  montoReclamado: 150_000,
  moneda: 'CRC',
  prioridad: PrioridadReclamo.Alta,
  createdAt: '2025-03-01T00:00:00',
  createdBy: 'admin',
  isDeleted: false,
};

const MOCK_STATS: ReclamosStats = {
  totalReclamos: 20,
  reclamosAbiertos: 8,
  reclamosEnProceso: 5,
  reclamosResueltos: 5,
  reclamosCerrados: 1,
  reclamosRechazados: 1,
  totalMontoReclamado: 3_000_000,
  totalMontoAprobado: 1_500_000,
  monedaPrincipal: 'CRC',
  reclamosPrioridadAlta: 4,
  reclamosPrioridadCritica: 2,
  reclamosVencidos: 3,
};

// ════════════════════════════════════════════════════════════════════════════
describe('ReclamosService', () => {
  let service: ReclamosService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service  = TestBed.inject(ReclamosService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  // ── getReclamos ──────────────────────────────────────────────────────────
  describe('getReclamos', () => {
    it('should send GET to /reclamos and return an array', () => {
      let result: Reclamo[] | undefined;

      service.getReclamos().subscribe(r => (result = r));
      httpMock.expectOne(API).flush([MOCK_RECLAMO]);

      expect(result!.length).toBe(1);
      expect(result![0].numeroReclamo).toBe('REC-2025-001');
      expect(result![0].estado).toBe(EstadoReclamo.Abierto);
    });

    it('should return an empty array when the backend returns none', () => {
      let result: Reclamo[] | undefined;

      service.getReclamos().subscribe(r => (result = r));
      httpMock.expectOne(API).flush([]);

      expect(result!.length).toBe(0);
    });
  });

  // ── getReclamo ───────────────────────────────────────────────────────────
  describe('getReclamo', () => {
    it('should send GET to /reclamos/:id and return the reclamo', () => {
      let result: Reclamo | undefined;

      service.getReclamo(1).subscribe(r => (result = r));
      httpMock.expectOne(`${API}/1`).flush(MOCK_RECLAMO);

      expect(result!.id).toBe(1);
      expect(result!.clienteNombreCompleto).toBe('María González');
    });
  });

  // ── getReclamoById (alias) ───────────────────────────────────────────────
  describe('getReclamoById', () => {
    it('should behave identically to getReclamo', () => {
      let result: Reclamo | undefined;

      service.getReclamoById(1).subscribe(r => (result = r));
      httpMock.expectOne(`${API}/1`).flush(MOCK_RECLAMO);

      expect(result!.id).toBe(1);
    });
  });

  // ── createReclamo ────────────────────────────────────────────────────────
  describe('createReclamo', () => {
    it('should send POST to /reclamos and return the created reclamo', () => {
      const dto: CreateReclamoDto = {
        polizaId: 1,
        numeroPoliza: 'POL-001',
        clienteNombreCompleto: 'María González',
        tipoReclamo: TipoReclamo.Siniestro,
        descripcion: 'Accidente',
        montoReclamado: 150_000,
        moneda: 'CRC',
        prioridad: PrioridadReclamo.Alta,
      };
      let result: Reclamo | undefined;

      service.createReclamo(dto).subscribe(r => (result = r));
      const req = httpMock.expectOne(API);

      expect(req.request.method).toBe('POST');
      expect(req.request.body.numeroPoliza).toBe('POL-001');
      req.flush({ ...MOCK_RECLAMO, id: 5 });

      expect(result!.id).toBe(5);
    });
  });

  // ── updateReclamo ────────────────────────────────────────────────────────
  describe('updateReclamo', () => {
    it('should send PUT to /reclamos/:id with the update payload', () => {
      const update: UpdateReclamoDto = { descripcion: 'Descripción actualizada' };
      let result: Reclamo | undefined;

      service.updateReclamo(1, update).subscribe(r => (result = r));
      const req = httpMock.expectOne(`${API}/1`);

      expect(req.request.method).toBe('PUT');
      expect(req.request.body.descripcion).toBe('Descripción actualizada');
      req.flush({ ...MOCK_RECLAMO, descripcion: 'Descripción actualizada' });

      expect(result!.descripcion).toBe('Descripción actualizada');
    });
  });

  // ── deleteReclamo ────────────────────────────────────────────────────────
  describe('deleteReclamo', () => {
    it('should send DELETE to /reclamos/:id', () => {
      service.deleteReclamo(1).subscribe();
      const req = httpMock.expectOne(`${API}/1`);

      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  // ── filtrarReclamos ───────────────────────────────────────────────────────
  describe('filtrarReclamos', () => {
    it('should POST filters to /reclamos/filtrar and return a paged result', () => {
      const filtros: FiltroReclamos = {
        estado: EstadoReclamo.Abierto,
        prioridad: PrioridadReclamo.Alta,
      };
      const paged: PagedResult<Reclamo> = {
        items: [MOCK_RECLAMO],
        totalCount: 1,
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      };
      let result: PagedResult<Reclamo> | undefined;

      service.filtrarReclamos(filtros).subscribe(r => (result = r));
      const req = httpMock.expectOne(`${API}/filtrar`);

      expect(req.request.method).toBe('POST');
      expect(req.request.body.estado).toBe(EstadoReclamo.Abierto);
      req.flush(paged);

      expect(result!.totalCount).toBe(1);
      expect(result!.items[0].id).toBe(1);
    });
  });

  // ── getReclamosStats ──────────────────────────────────────────────────────
  describe('getReclamosStats', () => {
    it('should send GET to /reclamos/stats and return statistics', () => {
      let result: ReclamosStats | undefined;

      service.getReclamosStats().subscribe(s => (result = s));
      httpMock.expectOne(`${API}/stats`).flush(MOCK_STATS);

      expect(result!.totalReclamos).toBe(20);
      expect(result!.reclamosAbiertos).toBe(8);
      expect(result!.totalMontoReclamado).toBe(3_000_000);
    });
  });

  // ── asignarReclamo ────────────────────────────────────────────────────────
  describe('asignarReclamo', () => {
    it('should send PUT to /reclamos/:id/asignar with the usuarioId', () => {
      let result: Reclamo | undefined;

      service.asignarReclamo(1, 42).subscribe(r => (result = r));
      const req = httpMock.expectOne(`${API}/1/asignar`);

      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({ usuarioId: 42 });
      req.flush({ ...MOCK_RECLAMO, usuarioAsignadoId: 42 });

      expect(result!.usuarioAsignadoId).toBe(42);
    });
  });

  // ── cambiarEstado ─────────────────────────────────────────────────────────
  describe('cambiarEstado', () => {
    it('should send PUT to /reclamos/:id/estado with estado and optional observaciones', () => {
      let result: Reclamo | undefined;

      service.cambiarEstado(1, EstadoReclamo.EnProceso, 'Revisando').subscribe(r => (result = r));
      const req = httpMock.expectOne(`${API}/1/estado`);

      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({ estado: EstadoReclamo.EnProceso, observaciones: 'Revisando' });
      req.flush({ ...MOCK_RECLAMO, estado: EstadoReclamo.EnProceso });

      expect(result!.estado).toBe(EstadoReclamo.EnProceso);
    });
  });

  // ── resolverReclamo ───────────────────────────────────────────────────────
  describe('resolverReclamo', () => {
    it('should send PUT to /reclamos/:id/resolver with montoAprobado and observaciones', () => {
      let result: Reclamo | undefined;

      service.resolverReclamo(1, 120_000, 'Aprobado parcialmente').subscribe(r => (result = r));
      const req = httpMock.expectOne(`${API}/1/resolver`);

      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({ montoAprobado: 120_000, observaciones: 'Aprobado parcialmente' });
      req.flush({ ...MOCK_RECLAMO, estado: EstadoReclamo.Resuelto, montoAprobado: 120_000 });

      expect(result!.estado).toBe(EstadoReclamo.Resuelto);
      expect(result!.montoAprobado).toBe(120_000);
    });
  });
});
