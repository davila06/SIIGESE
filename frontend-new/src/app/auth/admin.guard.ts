import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  
  constructor(private router: Router) {}

  canActivate(): boolean {
    // Mock admin check
    // In a real app, you would check if user has admin privileges
    const isAdmin = true; // Mock: always allow access for now
    
    if (!isAdmin) {
      this.router.navigate(['/reclamos']); // Redirect to main dashboard
      return false;
    }
    
    return true;
  }
}
