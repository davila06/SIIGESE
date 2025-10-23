import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class DataLoaderGuard implements CanActivate {

  constructor(
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  canActivate(): boolean {
    const isAuth = this.authService.isAuthenticated();
    const hasRole = this.authService.hasAnyRole(['Admin', 'DataLoader']);
    const user = this.authService.getCurrentUser();
    
    console.log('🛡️ DataLoaderGuard - canActivate check:', {
      isAuthenticated: isAuth,
      hasRole: hasRole,
      user: user,
      userRoles: user?.roles,
      lookingFor: ['Admin', 'DataLoader']
    });

    if (isAuth && hasRole) {
      console.log('✅ DataLoaderGuard - Access granted');
      return true;
    }

    console.log('❌ DataLoaderGuard - Access denied');
    
    // Mostrar mensaje de acceso denegado
    this.snackBar.open(
      'Acceso denegado. Solo los administradores y cargadores de datos pueden acceder a esta sección.',
      'Cerrar',
      {
        duration: 5000,
        horizontalPosition: 'center',
        verticalPosition: 'top',
        panelClass: ['error-snackbar']
      }
    );

    // Redirigir a la página de pólizas
    this.router.navigate(['/polizas']);
    return false;
  }
}