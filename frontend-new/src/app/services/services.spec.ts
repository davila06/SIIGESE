/**
 * @file services.spec.ts
 * @description Unit tests for TokenService and AuthService.
 *
 * Strategy:
 *  - TokenService: pure-logic tests (JWT decode / expiry checks) — no HTTP mocking required.
 *  - AuthService: ApiService is replaced by a Jasmine spy; TokenService is
 *    injected for real so the session-restore path and all token helpers are
 *    exercised end-to-end.
 */
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';

import { TokenService } from './token.service';
import { AuthService } from './auth.service';
import { ApiService } from './api.service';
import { LoginResponse, User } from '../interfaces/user.interface';

// ─── JWT helper ────────────────────────────────────────────────────────────
/**
 * Creates a minimal, correctly-structured JWT token for unit tests.
 * The signature segment is intentionally a dummy value because AuthService /
 * TokenService only inspect the payload, not the cryptographic signature.
 */
function makeJwt(exp: number, role: string | string[] = 'Admin'): string {
  const encode = (obj: object): string =>
    btoa(JSON.stringify(obj))
      .replace(/=/g, '')
      .replace(/\+/g, '-')
      .replace(/\//g, '_');

  const header  = encode({ alg: 'HS256', typ: 'JWT' });
  const payload = encode({
    sub: '1', nameid: '1',
    email: 'test@sinseg.cr', name: 'Test User',
    role, nbf: 1_700_000_000,
    exp,   iat: 1_700_000_000,
    iss: 'sinseg', aud: 'sinseg-spa',
  });
  return `${header}.${payload}.dummy-signature`;
}

const FUTURE_EXP = 9_999_999_999; // year 2286 — never expires in test runs
const PAST_EXP   = 1_000_000_000; // year 2001 — already expired

// ─── Shared fixtures ────────────────────────────────────────────────────────
const MOCK_USER: User = {
  id: 1,
  email: 'test@sinseg.cr',
  firstName: 'Test',
  lastName: 'User',
  roles: [{
    id: 1, name: 'Admin', description: 'Administrador',
    permissions: ['read', 'write'], isActive: true,
  }],
  isActive: true,
  createdAt: '2024-01-01T00:00:00',
  updatedAt: '2024-01-01T00:00:00',
};

const MOCK_LOGIN_RESPONSE: LoginResponse = {
  token: makeJwt(FUTURE_EXP),
  refreshToken: 'refresh-token',
  user: MOCK_USER,
  expiresAt: '2286-01-01T00:00:00Z',
};

// ════════════════════════════════════════════════════════════════════════════
//  TokenService
// ════════════════════════════════════════════════════════════════════════════
describe('TokenService', () => {
  let service: TokenService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(TokenService);
  });

  afterEach(() => sessionStorage.clear());

  // ── decodeToken ─────────────────────────────────────────────────────────
  describe('decodeToken', () => {
    it('should decode a valid JWT and return its payload', () => {
      const token   = makeJwt(FUTURE_EXP);
      const payload = service.decodeToken(token);

      expect(payload).not.toBeNull();
      expect(payload!.email).toBe('test@sinseg.cr');
      expect(payload!.name).toBe('Test User');
      expect(payload!.exp).toBe(FUTURE_EXP);
      expect(payload!.role).toBe('Admin');
    });

    it('should return null for a token with fewer than 3 parts', () => {
      expect(service.decodeToken('invalid')).toBeNull();
      expect(service.decodeToken('header.payload')).toBeNull();
    });

    it('should return null when the payload segment is not valid base64 JSON', () => {
      expect(service.decodeToken('header.!!!not-base64.signature')).toBeNull();
    });
  });

  // ── isTokenExpired ───────────────────────────────────────────────────────
  describe('isTokenExpired', () => {
    it('should return false for a token with a future expiration', () => {
      expect(service.isTokenExpired(makeJwt(FUTURE_EXP))).toBeFalse();
    });

    it('should return true for a token with a past expiration', () => {
      expect(service.isTokenExpired(makeJwt(PAST_EXP))).toBeTrue();
    });

    it('should return true for a malformed token (no parseable exp)', () => {
      expect(service.isTokenExpired('not.a.token')).toBeTrue();
    });
  });

  // ── shouldRefreshToken ───────────────────────────────────────────────────
  describe('shouldRefreshToken', () => {
    it('should return false when expiration is far in the future (>15 min)', () => {
      expect(service.shouldRefreshToken(makeJwt(FUTURE_EXP))).toBeFalse();
    });

    it('should return false for an already-expired token', () => {
      expect(service.shouldRefreshToken(makeJwt(PAST_EXP))).toBeFalse();
    });

    it('should return false for a malformed token', () => {
      expect(service.shouldRefreshToken('bad.token.here')).toBeFalse();
    });
  });

  // ── getTimeUntilExpiration ───────────────────────────────────────────────
  describe('getTimeUntilExpiration', () => {
    it('should return a positive millisecond value for a future token', () => {
      const remaining = service.getTimeUntilExpiration(makeJwt(FUTURE_EXP));
      expect(remaining).toBeGreaterThan(0);
    });

    it('should return 0 for an already-expired token', () => {
      expect(service.getTimeUntilExpiration(makeJwt(PAST_EXP))).toBe(0);
    });
  });

  // ── getTokenInfo ─────────────────────────────────────────────────────────
  describe('getTokenInfo', () => {
    it('should return null when no token is stored in sessionStorage', () => {
      sessionStorage.removeItem('authToken');
      expect(service.getTokenInfo()).toBeNull();
    });

    it('should return a populated TokenInfo for a valid stored token', () => {
      sessionStorage.setItem('authToken', makeJwt(FUTURE_EXP));

      const info = service.getTokenInfo();

      expect(info).not.toBeNull();
      expect(info!.usuario).toBe('test@sinseg.cr');
      expect(info!.nombre).toBe('Test User');
      expect(info!.expirado).toBeFalse();
      expect(info!.roles).toContain('Admin');
      expect(info!.minutosRestantes).toBeGreaterThan(0);
    });

    it('should handle an array role value in the token payload', () => {
      sessionStorage.setItem('authToken', makeJwt(FUTURE_EXP, ['Admin', 'Gestor']));

      const info = service.getTokenInfo();

      expect(info!.roles.length).toBe(2);
      expect(info!.roles).toContain('Admin');
      expect(info!.roles).toContain('Gestor');
    });

    it('should mark an expired token as expirado: true', () => {
      sessionStorage.setItem('authToken', makeJwt(PAST_EXP));

      const info = service.getTokenInfo();

      expect(info!.expirado).toBeTrue();
      expect(info!.minutosRestantes).toBe(0);
    });
  });
});

// ════════════════════════════════════════════════════════════════════════════
//  AuthService
// ════════════════════════════════════════════════════════════════════════════
describe('AuthService', () => {
  let service: AuthService;
  let apiSpy: jasmine.SpyObj<ApiService>;

  beforeEach(() => {
    sessionStorage.clear();

    const spy = jasmine.createSpyObj<ApiService>('ApiService', [
      'login', 'forgotPassword', 'changePassword',
    ]);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService,
        { provide: ApiService, useValue: spy },
      ],
    });

    service = TestBed.inject(AuthService);
    apiSpy  = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
  });

  afterEach(() => sessionStorage.clear());

  // ── isAuthenticated (initial state) ─────────────────────────────────────
  describe('isAuthenticated (initial state)', () => {
    it('should return false when no user is set after a clean init', () => {
      expect(service.isAuthenticated()).toBeFalse();
    });

    it('should return null from getCurrentUser when not logged in', () => {
      expect(service.getCurrentUser()).toBeNull();
    });
  });

  // ── login ────────────────────────────────────────────────────────────────
  describe('login', () => {
    it('should set current user and persist token to sessionStorage on success', done => {
      apiSpy.login.and.returnValue(of(MOCK_LOGIN_RESPONSE));

      service.login('test@sinseg.cr', 'password123').subscribe(() => {
        expect(service.isAuthenticated()).toBeTrue();
        expect(service.getCurrentUser()!.email).toBe('test@sinseg.cr');
        expect(sessionStorage.getItem('authToken')).toBe(MOCK_LOGIN_RESPONSE.token);
        expect(sessionStorage.getItem('currentUser')).not.toBeNull();
        done();
      });
    });

    it('should pass email-derived userName to ApiService.login', done => {
      apiSpy.login.and.returnValue(of(MOCK_LOGIN_RESPONSE));

      service.login('agent@sinseg.cr', 'pass').subscribe(() => {
        const loginArg = apiSpy.login.calls.mostRecent().args[0];
        expect(loginArg.email).toBe('agent@sinseg.cr');
        expect(loginArg.userName).toBe('agent');
        done();
      });
    });

    it('should propagate errors from ApiService without changing auth state', done => {
      const error = new Error('Credenciales inválidas');
      apiSpy.login.and.returnValue(throwError(() => error));

      service.login('test@sinseg.cr', 'wrong').subscribe({
        error: err => {
          expect(err.message).toBe('Credenciales inválidas');
          expect(service.isAuthenticated()).toBeFalse();
          done();
        },
      });
    });
  });

  // ── logout ───────────────────────────────────────────────────────────────
  describe('logout', () => {
    beforeEach(done => {
      apiSpy.login.and.returnValue(of(MOCK_LOGIN_RESPONSE));
      service.login('test@sinseg.cr', 'pass').subscribe({ next: () => done() });
    });

    it('should clear user state and remove sessionStorage entries', () => {
      service.logout();

      expect(service.isAuthenticated()).toBeFalse();
      expect(service.getCurrentUser()).toBeNull();
      expect(sessionStorage.getItem('authToken')).toBeNull();
      expect(sessionStorage.getItem('currentUser')).toBeNull();
    });
  });

  // ── hasRole ──────────────────────────────────────────────────────────────
  describe('hasRole', () => {
    beforeEach(done => {
      apiSpy.login.and.returnValue(of(MOCK_LOGIN_RESPONSE));
      service.login('test@sinseg.cr', 'pass').subscribe({ next: () => done() });
    });

    it('should return true for a role the logged-in user has', () => {
      expect(service.hasRole('Admin')).toBeTrue();
    });

    it('should return false for a role the user does not have', () => {
      expect(service.hasRole('Gestor')).toBeFalse();
    });

    it('should return false when called without an authenticated user', () => {
      service.logout();
      expect(service.hasRole('Admin')).toBeFalse();
    });
  });

  // ── hasAnyRole ───────────────────────────────────────────────────────────
  describe('hasAnyRole', () => {
    beforeEach(done => {
      apiSpy.login.and.returnValue(of(MOCK_LOGIN_RESPONSE));
      service.login('test@sinseg.cr', 'pass').subscribe({ next: () => done() });
    });

    it('should return true when the user has at least one of the given roles', () => {
      expect(service.hasAnyRole(['Gestor', 'Admin'])).toBeTrue();
    });

    it('should return false when the user has none of the given roles', () => {
      expect(service.hasAnyRole(['Gestor', 'Auditor'])).toBeFalse();
    });
  });

  // ── hasPermission ────────────────────────────────────────────────────────
  describe('hasPermission', () => {
    beforeEach(done => {
      apiSpy.login.and.returnValue(of(MOCK_LOGIN_RESPONSE));
      service.login('test@sinseg.cr', 'pass').subscribe({ next: () => done() });
    });

    it('should return true for a permission included in the Admin role', () => {
      expect(service.hasPermission('write')).toBeTrue();
      expect(service.hasPermission('read')).toBeTrue();
    });

    it('should return false for a permission not assigned to any role', () => {
      expect(service.hasPermission('delete_all')).toBeFalse();
    });
  });

  // ── isAdmin ──────────────────────────────────────────────────────────────
  describe('isAdmin', () => {
    it('should return false when no user is authenticated', () => {
      expect(service.isAdmin()).toBeFalse();
    });

    it('should return true for a user with the Admin role', done => {
      apiSpy.login.and.returnValue(of(MOCK_LOGIN_RESPONSE));
      service.login('test@sinseg.cr', 'pass').subscribe(() => {
        expect(service.isAdmin()).toBeTrue();
        done();
      });
    });
  });

  // ── canUploadExcel ───────────────────────────────────────────────────────
  describe('canUploadExcel', () => {
    it('should return true for a user with the "write" permission', done => {
      apiSpy.login.and.returnValue(of(MOCK_LOGIN_RESPONSE));
      service.login('test@sinseg.cr', 'pass').subscribe(() => {
        expect(service.canUploadExcel()).toBeTrue();
        done();
      });
    });

    it('should return false when no user is authenticated', () => {
      expect(service.canUploadExcel()).toBeFalse();
    });
  });

  // ── getCurrentUserId ─────────────────────────────────────────────────────
  describe('getCurrentUserId', () => {
    it('should return null when not authenticated', () => {
      expect(service.getCurrentUserId()).toBeNull();
    });

    it('should return the numeric user ID after login', done => {
      apiSpy.login.and.returnValue(of(MOCK_LOGIN_RESPONSE));
      service.login('test@sinseg.cr', 'pass').subscribe(() => {
        expect(service.getCurrentUserId()).toBe(1);
        done();
      });
    });
  });

  // ── session restore ──────────────────────────────────────────────────────
  describe('session restore on constructor init', () => {
    it('should restore the authenticated user from valid sessionStorage entries', () => {
      sessionStorage.setItem('authToken', makeJwt(FUTURE_EXP));
      sessionStorage.setItem('currentUser', JSON.stringify(MOCK_USER));

      // Directly instantiate to exercise the constructor restore path
      const restored = new AuthService(
        TestBed.inject(ApiService),
        TestBed.inject(TokenService),
      );

      expect(restored.isAuthenticated()).toBeTrue();
      expect(restored.getCurrentUser()!.email).toBe('test@sinseg.cr');
    });

    it('should clear sessionStorage and remain unauthenticated if stored token is expired', () => {
      sessionStorage.setItem('authToken', makeJwt(PAST_EXP));
      sessionStorage.setItem('currentUser', JSON.stringify(MOCK_USER));

      const restored = new AuthService(
        TestBed.inject(ApiService),
        TestBed.inject(TokenService),
      );

      expect(restored.isAuthenticated()).toBeFalse();
      expect(sessionStorage.getItem('authToken')).toBeNull();
      expect(sessionStorage.getItem('currentUser')).toBeNull();
    });

    it('should remain unauthenticated when sessionStorage is empty', () => {
      const fresh = new AuthService(
        TestBed.inject(ApiService),
        TestBed.inject(TokenService),
      );

      expect(fresh.isAuthenticated()).toBeFalse();
    });
  });
});
