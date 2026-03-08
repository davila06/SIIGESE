import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, interval, timer } from 'rxjs';
import { switchMap, filter, take } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

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
  
  // Renovar el token cuando falten 15 minutos para expirar
  private readonly REFRESH_THRESHOLD_MINUTES = 15;
  
  constructor(private http: HttpClient) {
    this.startTokenRefreshMonitor();
  }

  /**
   * Decodifica un token JWT sin validar la firma
   */
  decodeToken(token: string): TokenPayload | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) {
        console.error('❌ Token JWT inválido');
        return null;
      }

      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded) as TokenPayload;
    } catch (error) {
      console.error('❌ Error decodificando token:', error);
      return null;
    }
  }

  /**
   * Verifica si el token está expirado
   */
  isTokenExpired(token: string): boolean {
    const payload = this.decodeToken(token);
    if (!payload || !payload.exp) {
      return true;
    }

    const expirationDate = new Date(payload.exp * 1000);
    const now = new Date();
    
    return expirationDate <= now;
  }

  /**
   * Verifica si el token necesita renovarse (está cerca de expirar)
   */
  shouldRefreshToken(token: string): boolean {
    const payload = this.decodeToken(token);
    if (!payload || !payload.exp) {
      return false;
    }

    const expirationDate = new Date(payload.exp * 1000);
    const now = new Date();
    const thresholdDate = new Date(now.getTime() + this.REFRESH_THRESHOLD_MINUTES * 60 * 1000);
    
    // Si la fecha de expiración es menor que el threshold, necesita renovarse
    const shouldRefresh = expirationDate <= thresholdDate && expirationDate > now;
    
    if (shouldRefresh) {
      console.log('⏰ Token próximo a expirar:', {
        expira: expirationDate.toLocaleString(),
        ahora: now.toLocaleString(),
        minutosRestantes: Math.floor((expirationDate.getTime() - now.getTime()) / 60000)
      });
    }
    
    return shouldRefresh;
  }

  /**
   * Obtiene el tiempo restante hasta la expiración en milisegundos
   */
  getTimeUntilExpiration(token: string): number {
    const payload = this.decodeToken(token);
    if (!payload || !payload.exp) {
      return 0;
    }

    const expirationDate = new Date(payload.exp * 1000);
    const now = new Date();
    
    return Math.max(0, expirationDate.getTime() - now.getTime());
  }

  /**
   * Renueva el token usando el endpoint del backend
   */
  refreshToken(): Observable<any> {
    const currentToken = localStorage.getItem('authToken');
    
    if (!currentToken) {
      throw new Error('No hay token para renovar');
    }

    console.log('🔄 Renovando token...');
    this.tokenRefreshInProgress = true;

    return this.http.post(`${environment.apiUrl}/auth/refresh-token`, {}, {
      headers: {
        'Authorization': `Bearer ${currentToken}`
      }
    });
  }

  /**
   * Actualiza el token en localStorage y notifica
   */
  updateToken(response: any): void {
    if (response && response.token) {
      localStorage.setItem('authToken', response.token);
      
      if (response.user) {
        localStorage.setItem('currentUser', JSON.stringify(response.user));
      }
      
      console.log('✅ Token renovado exitosamente. Expira:', response.expiresAt);
      this.tokenRefreshInProgress = false;
      this.tokenRefreshedSubject.next(true);
    }
  }

  /**
   * Inicia el monitoreo automático del token para renovarlo
   */
  private startTokenRefreshMonitor(): void {
    // Verificar cada minuto si el token necesita renovarse
    interval(60000).subscribe(() => {
      const token = localStorage.getItem('authToken');
      
      if (!token) {
        return;
      }

      // Si ya está expirado, no intentar renovar (el interceptor manejará el logout)
      if (this.isTokenExpired(token)) {
        console.warn('⚠️ Token expirado detectado');
        return;
      }

      // Si necesita renovarse y no hay una renovación en progreso
      if (this.shouldRefreshToken(token) && !this.tokenRefreshInProgress) {
        console.log('🔄 Iniciando renovación automática de token...');
        
        this.refreshToken().subscribe({
          next: (response) => {
            this.updateToken(response);
          },
          error: (error) => {
            console.error('❌ Error renovando token automáticamente:', error);
            this.tokenRefreshInProgress = false;
            
            // Si el error es 401, el interceptor manejará el logout
            if (error.status !== 401) {
              // Reintentar en 1 minuto
              console.log('⏰ Reintentando renovación en 1 minuto...');
            }
          }
        });
      }
    });
  }

  /**
   * Obtiene información del token actual
   */
  getTokenInfo(): any {
    const token = localStorage.getItem('authToken');
    
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
