import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, interval } from 'rxjs';
import { switchMap, filter, take } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { LoginResponse, TokenInfo } from '../interfaces/user.interface';

interface TokenPayload {
  sub: string;
  nameid: string;
  email: string;
  name: string;
  role: string | string[];
  nbf: number;
  exp: number;
  iat: number;
  iss: string;
  aud: string;
}

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private tokenRefreshInProgress = false;
  private tokenRefreshedSubject = new BehaviorSubject<boolean>(false);

  private readonly REFRESH_THRESHOLD_MINUTES = 15;

  constructor(private http: HttpClient) {
    this.startTokenRefreshMonitor();
  }

  decodeToken(token: string): TokenPayload | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) {
        return null;
      }
      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded) as TokenPayload;
    } catch {
      return null;
    }
  }

  isTokenExpired(token: string): boolean {
    const payload = this.decodeToken(token);
    if (!payload?.exp) {
      return true;
    }
    return new Date(payload.exp * 1000) <= new Date();
  }

  shouldRefreshToken(token: string): boolean {
    const payload = this.decodeToken(token);
    if (!payload?.exp) {
      return false;
    }
    const expirationDate = new Date(payload.exp * 1000);
    const now = new Date();
    const thresholdDate = new Date(now.getTime() + this.REFRESH_THRESHOLD_MINUTES * 60 * 1000);
    return expirationDate <= thresholdDate && expirationDate > now;
  }

  getTimeUntilExpiration(token: string): number {
    const payload = this.decodeToken(token);
    if (!payload?.exp) {
      return 0;
    }
    return Math.max(0, new Date(payload.exp * 1000).getTime() - Date.now());
  }

  refreshToken(): Observable<LoginResponse> {
    const currentToken = sessionStorage.getItem('authToken');
    if (!currentToken) {
      throw new Error('No token available for refresh');
    }
    this.tokenRefreshInProgress = true;
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/refresh-token`, {}, {
      headers: { 'Authorization': `Bearer ${currentToken}` }
    });
  }

  updateToken(response: LoginResponse): void {
    if (response?.token) {
      sessionStorage.setItem('authToken', response.token);
      if (response.user) {
        sessionStorage.setItem('currentUser', JSON.stringify(response.user));
      }
      if (!environment.production) {
        console.log('TokenService: token refreshed, expires', response.expiresAt);
      }
      this.tokenRefreshInProgress = false;
      this.tokenRefreshedSubject.next(true);
    }
  }

  private startTokenRefreshMonitor(): void {
    interval(60000).subscribe(() => {
      const token = sessionStorage.getItem('authToken');
      if (!token || this.isTokenExpired(token)) {
        return;
      }
      if (this.shouldRefreshToken(token) && !this.tokenRefreshInProgress) {
        this.refreshToken().subscribe({
          next: (response) => this.updateToken(response),
          error: () => { this.tokenRefreshInProgress = false; }
        });
      }
    });
  }

  getTokenInfo(): TokenInfo | null {
    const token = sessionStorage.getItem('authToken');
    if (!token) {
      return null;
    }
    const payload = this.decodeToken(token);
    if (!payload) {
      return null;
    }
    return {
      usuario: payload.email,
      nombre: payload.name,
      roles: Array.isArray(payload.role) ? payload.role : [payload.role],
      expira: new Date(payload.exp * 1000),
      expirado: this.isTokenExpired(token),
      necesitaRenovacion: this.shouldRefreshToken(token),
      minutosRestantes: Math.floor(this.getTimeUntilExpiration(token) / 60000)
    };
  }
}

