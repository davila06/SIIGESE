import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  
  constructor(
    private readonly router: Router,
    private readonly authService: AuthService
  ) {}

  canActivate(): boolean {
    const isAdmin = this.authService.isAdmin();
    
    if (!isAdmin) {
      this.router.navigate(['/polizas']);
      return false;
    }
    
    return true;
  }
}
