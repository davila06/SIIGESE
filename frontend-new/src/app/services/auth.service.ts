import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { ApiService } from './api.service';
import { TokenService } from './token.service';
import { User, LoginResponse, Role } from '../interfaces/user.interface';
import { environment } from '../../environments/environment';

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

  constructor(
    private apiService: ApiService,
    private tokenService: TokenService
  ) {
    const savedUser = sessionStorage.getItem('currentUser');
    const authToken = sessionStorage.getItem('authToken');

    if (savedUser && authToken) {
      try {
        if (this.tokenService.isTokenExpired(authToken)) {
          this.logout();
        } else {
          const user = JSON.parse(savedUser) as User;
          this.currentUserSubject.next(user);
          if (!environment.production) {
            console.log('AuthService: session restored for', user.email);
          }
        }
      } catch {
        sessionStorage.removeItem('currentUser');
        sessionStorage.removeItem('authToken');
      }
    }
  }

  login(email: string, password: string): Observable<LoginResponse> {
    const userName = email.split('@')[0];
    return this.apiService.login({ userName, email, password }).pipe(
      tap(response => {
        this.currentUserSubject.next(response.user);
        sessionStorage.setItem('currentUser', JSON.stringify(response.user));
        sessionStorage.setItem('authToken', response.token);
      }),
      catchError(error => {
        throw error;
      })
    );
  }

  logout(): void {
    this.currentUserSubject.next(null);
    sessionStorage.removeItem('currentUser');
    sessionStorage.removeItem('authToken');
  }

  resetPassword(email: string): Observable<ResetPasswordResponse> {
    return this.apiService.forgotPassword(email).pipe(
      map(response => ({ message: response.message, success: true })),
      catchError(error => {
        throw error;
      })
    );
  }

  changePassword(currentPassword: string, newPassword: string, confirmPassword: string): Observable<void> {
    return this.apiService.changePassword({ currentPassword, newPassword, confirmPassword });
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
