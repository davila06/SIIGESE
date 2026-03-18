import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const dataLoaderGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.hasAnyRole(['Admin', 'DataLoader'])) {
    router.navigate(['/polizas']);
    return false;
  }

  return true;
};
