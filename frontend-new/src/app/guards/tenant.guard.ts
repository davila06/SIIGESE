// tenant.guard.ts - Guard para proteger rutas que requieren tenant

import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError, take } from 'rxjs/operators';
import { TenantService, TenantInfo } from '../services/tenant.service';

@Injectable({
  providedIn: 'root'
})
export class TenantGuard implements CanActivate, CanActivateChild {
  
  constructor(
    private tenantService: TenantService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkTenantAccess(route, state);
  }

  canActivateChild(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkTenantAccess(route, state);
  }

  private checkTenantAccess(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> {
    return this.tenantService.getTenantInfo().pipe(
      take(1),
      map((tenant: TenantInfo | null) => {
        // Verificar si la ruta requiere tenant
        const requiresTenant = route.data?.['requiresTenant'] !== false;
        
        if (requiresTenant && (!tenant || !tenant.isActive)) {
          console.warn('Access denied: No valid tenant or tenant inactive');
          
          // Intentar detectar tenant desde URL
          const detectedTenantId = this.detectTenantFromRoute(route, state);
          if (detectedTenantId) {
            // Redirigir para cargar el tenant
            this.router.navigate(['/loading'], { 
              queryParams: { 
                tenant: detectedTenantId,
                redirect: state.url 
              }
            });
            return false;
          }
          
          // No se pudo determinar tenant - ir a selector
          this.router.navigate(['/select-tenant'], {
            queryParams: { redirect: state.url }
          });
          return false;
        }

        // Verificar permisos específicos de la ruta
        const requiredRole = route.data?.['requiredRole'];
        if (requiredRole && tenant) {
          // Aquí verificarías el rol del usuario en el tenant
          // const userRole = this.getUserRoleInTenant(tenant.tenantId);
          // return this.hasRequiredRole(userRole, requiredRole);
        }

        return true;
      }),
      catchError((error: any) => {
        console.error('Error checking tenant access:', error);
        this.router.navigate(['/error'], {
          queryParams: { 
            error: 'tenant_check_failed',
            message: 'Error verificando acceso a la empresa'
          }
        });
        return of(false);
      })
    );
  }

  private detectTenantFromRoute(
    route: ActivatedRouteSnapshot, 
    state: RouterStateSnapshot
  ): string | null {
    // Detectar desde parámetros de ruta
    if (route.params['tenantId']) {
      return route.params['tenantId'];
    }

    // Detectar desde query parameters
    if (route.queryParams['tenant']) {
      return route.queryParams['tenant'];
    }

    // Detectar desde URL path
    const pathMatch = state.url.match(/^\/tenant\/([^\/]+)/);
    if (pathMatch) {
      return pathMatch[1];
    }

    return null;
  }
}