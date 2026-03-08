import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class DataLoaderGuard implements CanActivate {
  
  constructor(
    private readonly router: Router,
    private readonly authService: AuthService
  ) {}

  canActivate(): boolean {
    const canLoadData = this.authService.hasAnyRole(['Admin', 'DataLoader']);
    
    if (!canLoadData) {
      this.router.navigate(['/polizas']);
      return false;
    }
    
    return true;
  }
}
