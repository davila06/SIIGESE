import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {

  constructor(
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  canActivate(): boolean {
    if (this.authService.isAuthenticated() && this.authService.hasAnyRole(['Admin'])) {
      return true;
    }

    // Mostrar mensaje de acceso denegado
    this.snackBar.open(
      'Acceso denegado. Solo los administradores pueden acceder a esta sección.',
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