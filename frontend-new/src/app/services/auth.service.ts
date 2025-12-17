import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { ApiService } from './api.service';
import { User, LoginResponse, Role } from '../interfaces/user.interface';

export interface ResetPasswordResponse {
  message: string;
  success: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private apiService: ApiService) {
    // Verificar si hay usuario guardado en localStorage
    const savedUser = localStorage.getItem('currentUser');
    const authToken = localStorage.getItem('authToken');
    
    if (savedUser && authToken) {
      try {
        const user = JSON.parse(savedUser);
        this.currentUserSubject.next(user);
        console.log('✅ Usuario cargado desde localStorage:', user);
      } catch (error) {
        // Si hay error al parsear, limpiar localStorage
        console.error('❌ Error parseando usuario desde localStorage:', error);
        localStorage.removeItem('currentUser');
        localStorage.removeItem('authToken');
      }
    }
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.apiService.login({ email, password }).pipe(
      tap(response => {
        console.log('✅ Login response from API:', response);
        this.currentUserSubject.next(response.user);
        localStorage.setItem('currentUser', JSON.stringify(response.user));
        localStorage.setItem('authToken', response.token);
      }),
      catchError(error => {
        console.error('❌ Login error:', error);
        throw error;
      })
    );
  }

  logout(): void {
    this.currentUserSubject.next(null);
    localStorage.removeItem('currentUser');
    localStorage.removeItem('authToken');
  }

  resetPassword(email: string): Observable<ResetPasswordResponse> {
    // TODO: Implementar reset password con backend real
    return new Observable(observer => {
      setTimeout(() => {
        observer.next({
          message: 'Se ha enviado un enlace de reseteo a tu email',
          success: true
        });
        observer.complete();
      }, 1000);
    });
  }

  isAuthenticated(): boolean {
    return this.currentUserSubject.value !== null;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  hasRole(roleName: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles.some(role => role.name === roleName) || false;
  }

  hasAnyRole(roleNames: string[]): boolean {
    const user = this.getCurrentUser();
    return roleNames.some(roleName => 
      user?.roles.some(role => role.name === roleName)
    ) || false;
  }

  hasPermission(permission: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles.some(role => 
      role.permissions.includes(permission)
    ) || false;
  }

  isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  canUploadExcel(): boolean {
    return this.hasPermission('write') || this.isAdmin();
  }

  getCurrentUserId(): number | null {
    const user = this.getCurrentUser();
    return user?.id || null;
  }
}
