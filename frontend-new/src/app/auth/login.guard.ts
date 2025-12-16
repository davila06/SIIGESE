import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class LoginGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): boolean {
    const isAuthenticated = this.authService.isAuthenticated();
    
    if (isAuthenticated) {
      console.log('✅ Usuario ya autenticado, redirigiendo a dashboard');
      this.router.navigate(['/dashboard']);
      return false;
    }
    
    console.log('👤 Usuario no autenticado, permitiendo acceso a login');
    return true;
  }
}
