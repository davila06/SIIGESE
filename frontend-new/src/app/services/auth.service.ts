import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ResetPasswordRequest {
  email: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface PasswordResetRequest {
  token: string;
  newPassword: string;
}

export interface ResetPasswordResponse {
  message: string;
  success: boolean;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: {
    id: number;
    email: string;
    firstName: string;
    lastName: string;
    roles: Role[];
  };
}

export interface Role {
  id: number;
  name: string;
  description: string;
}

export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  roles: Role[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = environment.apiUrl;
  private readonly TOKEN_KEY = 'sinseg_token';
  private readonly REFRESH_TOKEN_KEY = 'sinseg_refresh_token';
  private readonly USER_KEY = 'sinseg_user';

  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    // Inicializar correctamente verificando autenticación
    this.initializeAuth();
  }

  private initializeAuth(): void {
    const user = this.getCurrentUser();
    this.currentUserSubject.next(user);
  }

  login(email: string, password: string): Observable<LoginResponse> {
    const loginData: LoginRequest = { email, password };
    
    return this.http.post<LoginResponse>(`${this.API_URL}/auth/login`, loginData)
      .pipe(
        tap(response => {
          this.storeAuthData(response);
          this.currentUserSubject.next(response.user);
        })
      );
  }

  logout(): void {
    this.clearAuthData();
    this.currentUserSubject.next(null);
  }

  private clearAuthData(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    // Verificar si el token ha expirado
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000; // Convertir a milliseconds
      return Date.now() < expiry;
    } catch {
      return false;
    }
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getCurrentUser(): User | null {
    // Primero verificar si está autenticado
    if (!this.isAuthenticated()) {
      // Si no está autenticado, limpiar localStorage y retornar null
      this.clearAuthData();
      return null;
    }
    
    const userStr = localStorage.getItem(this.USER_KEY);
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    }
    return null;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles?.some(r => r.name === role) || false;
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUser();
    if (!user?.roles) return false;
    return roles.some(role => user.roles.some(r => r.name === role));
  }

  private storeAuthData(response: LoginResponse): void {
    // Limpiar el token del prefijo "Bearer " si existe
    const cleanToken = response.token.replace('Bearer ', '');
    localStorage.setItem(this.TOKEN_KEY, cleanToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(response.user));
  }

  // Método para refrescar el token (implementar según necesidades del backend)
  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    return this.http.post<LoginResponse>(`${this.API_URL}/auth/refresh`, {
      refreshToken
    }).pipe(
      tap(response => {
        this.storeAuthData(response);
        this.currentUserSubject.next(response.user);
      })
    );
  }

  // Método para resetear contraseña
  resetPassword(email: string): Observable<ResetPasswordResponse> {
    const resetData: ResetPasswordRequest = { email };
    
    return this.http.post<ResetPasswordResponse>(`${this.API_URL}/auth/reset-password`, resetData);
  }

  // Método para cambiar contraseña del usuario actual
  changePassword(currentPassword: string, newPassword: string): Observable<any> {
    const changeData: ChangePasswordRequest = { currentPassword, newPassword };
    
    return this.http.post(`${this.API_URL}/auth/change-password`, changeData);
  }

  // Método para solicitar reset de contraseña (olvido)
  forgotPassword(email: string): Observable<any> {
    const forgotData: ForgotPasswordRequest = { email };
    
    return this.http.post(`${this.API_URL}/auth/forgot-password`, forgotData);
  }

  // Método para completar reset de contraseña con token
  resetPasswordWithToken(token: string, newPassword: string): Observable<any> {
    const resetData: PasswordResetRequest = { token, newPassword };
    
    return this.http.post(`${this.API_URL}/auth/reset-password`, resetData);
  }
}