import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class DataLoaderGuard implements CanActivate {
  
  constructor(private router: Router) {}

  canActivate(): boolean {
    // Mock data loader permission check
    // In a real app, you would check if user has data loading permissions
    const canLoadData = true; // Mock: always allow access for now
    
    if (!canLoadData) {
      this.router.navigate(['/polizas']); // Redirect to polizas list
      return false;
    }
    
    return true;
  }
}
