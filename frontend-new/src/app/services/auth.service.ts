import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, delay } from 'rxjs/operators';

export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  roles: Role[];
  lastLoginAt: Date;
}

export interface Role {
  id: number;
  name: string;
  permissions: string[];
}

export interface LoginResponse {
  user: User;
  token: string;
  message: string;
}

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

  // Mock user data
  private mockUser: User = {
    id: 1,
    email: 'admin@sinseg.com',
    firstName: 'Administrador',
    lastName: 'Sistema',
    roles: [
      {
        id: 1,
        name: 'Admin',
        permissions: ['read', 'write', 'delete', 'admin']
      }
    ],
    lastLoginAt: new Date()
  };

  constructor() {
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
    // Removido el auto-login - el usuario debe hacer login manualmente
  }

  login(email: string, password: string): Observable<LoginResponse> {
    // Mock login - siempre exitoso para desarrollo
    return of({
      user: this.mockUser,
      token: 'mock-jwt-token',
      message: 'Login exitoso'
    }).pipe(
      delay(1000), // Simular delay de red
      map(response => {
        this.currentUserSubject.next(response.user);
        localStorage.setItem('currentUser', JSON.stringify(response.user));
        localStorage.setItem('authToken', response.token);
        return response;
      })
    );
  }

  logout(): void {
    this.currentUserSubject.next(null);
    localStorage.removeItem('currentUser');
    localStorage.removeItem('authToken');
  }

  resetPassword(email: string): Observable<ResetPasswordResponse> {
    // Mock reset password
    return of({
      message: 'Se ha enviado un enlace de reseteo a tu email',
      success: true
    }).pipe(delay(1000));
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
