import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  
  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  canActivate(): boolean {
    const currentUser = this.authService.getCurrentUser();
    const isAuthenticated = !!currentUser;
    
    console.log('🔒 AuthGuard check:', { 
      isAuthenticated, 
      currentUser: currentUser?.email || 'none' 
    });
    
    if (!isAuthenticated) {
      console.log('❌ AuthGuard: Usuario no autenticado, redirigiendo a login');
      this.router.navigate(['/login']);
      return false;
    }
    
    console.log('✅ AuthGuard: Usuario autenticado, permitiendo acceso');
    return true;
  }
}
