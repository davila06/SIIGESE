/**
 * @file polizas.service.spec.ts
 * @description Unit tests for PolizasService.
 *
 * Strategy: HttpClientTestingModule intercepts all HTTP calls so no real
 * network requests are made.  Each test flushes a mock response and asserts
 * on the returned data, HTTP method, and URL.
 */
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { environment } from '../../../environments/environment';

import { Poliza, PolizasService } from './polizas.service';

const API = `${environment.apiUrl}/polizas`;

// ─── Fixtures ───────────────────────────────────────────────────────────────
const MOCK_POLIZA: Poliza = {
  id: 1,
  numeroPoliza: 'POL-2025-001',
  nombreAsegurado: 'Carlos Rodríguez',
  numeroCedula: '1-0234-5678',
  prima: 45_000,
  moneda: 'CRC',
  fechaVigencia: new Date('2026-01-01T00:00:00'),
  frecuencia: 'MENSUAL',
  aseguradora: 'INS',
  esActivo: true,
};

// ════════════════════════════════════════════════════════════════════════════
describe('PolizasService', () => {
  let service: PolizasService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service  = TestBed.inject(PolizasService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  // ── getAll ───────────────────────────────────────────────────────────────
  describe('getAll', () => {
    it('should send GET to /polizas and return all polizas', () => {
      let result: Poliza[] | undefined;

      service.getAll().subscribe(p => (result = p));
      httpMock.expectOne(API).flush([MOCK_POLIZA]);

      expect(result!.length).toBe(1);
      expect(result![0].numeroPoliza).toBe('POL-2025-001');
      expect(result![0].nombreAsegurado).toBe('Carlos Rodríguez');
    });

    it('should return an empty array when no polizas exist', () => {
      let result: Poliza[] | undefined;

      service.getAll().subscribe(p => (result = p));
      httpMock.expectOne(API).flush([]);

      expect(result!.length).toBe(0);
    });
  });

  // ── getById ──────────────────────────────────────────────────────────────
  describe('getById', () => {
    it('should send GET to /polizas/:id and return the poliza', () => {
      let result: Poliza | undefined;

      service.getById(1).subscribe(p => (result = p));
      httpMock.expectOne(`${API}/1`).flush(MOCK_POLIZA);

      expect(result!.id).toBe(1);
      expect(result!.prima).toBe(45_000);
    });

    it('should use the correct id in the request URL', () => {
      service.getById(99).subscribe();
      const req = httpMock.expectOne(`${API}/99`);

      expect(req.request.method).toBe('GET');
      req.flush({ ...MOCK_POLIZA, id: 99 });
    });
  });

  // ── getByNumeroPoliza ─────────────────────────────────────────────────────
  describe('getByNumeroPoliza', () => {
    it('should send GET to /polizas/numero/:numeroPoliza and return the poliza', () => {
      let result: Poliza | undefined;

      service.getByNumeroPoliza('POL-2025-001').subscribe(p => (result = p));
      httpMock.expectOne(`${API}/numero/POL-2025-001`).flush(MOCK_POLIZA);

      expect(result!.numeroPoliza).toBe('POL-2025-001');
    });
  });

  // ── buscarPolizas ─────────────────────────────────────────────────────────
  describe('buscarPolizas', () => {
    it('should send GET to /polizas/buscar?termino=... with the search term URL-encoded', () => {
      let result: Poliza[] | undefined;

      service.buscarPolizas('Carlos').subscribe(p => (result = p));
      const req = httpMock.expectOne(`${API}/buscar?termino=Carlos`);

      expect(req.request.method).toBe('GET');
      req.flush([MOCK_POLIZA]);

      expect(result!.length).toBe(1);
    });

    it('should URL-encode special characters in the search term', () => {
      const encoded = encodeURIComponent('Rodríguez');

      service.buscarPolizas('Rodríguez').subscribe();
      const req = httpMock.expectOne(`${API}/buscar?termino=${encoded}`);

      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });
});
