import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    const isAuth = this.authService.isAuthenticated();
    console.log('🛡️ AuthGuard - isAuthenticated:', isAuth);
    console.log('🛡️ AuthGuard - Current token:', this.authService.getToken());
    
    if (isAuth) {
      console.log('🛡️ AuthGuard - Access granted');
      return true;
    } else {
      console.log('🛡️ AuthGuard - Access denied, redirecting to login');
      // Redirigir al login si no está autenticado
      this.router.navigate(['/login']);
      return false;
    }
  }
}