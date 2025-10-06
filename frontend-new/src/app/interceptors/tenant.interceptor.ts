// tenant.interceptor.ts - Interceptor HTTP para Multi-Tenancy

import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap, take } from 'rxjs/operators';
import { TenantService } from '../services/tenant.service';
import { Router } from '@angular/router';

@Injectable()
export class TenantInterceptor implements HttpInterceptor {
  
  constructor(
    private tenantService: TenantService,
    private router: Router
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Rutas que no requieren tenant
    const publicRoutes = [
      '/api/auth/login',
      '/api/auth/register',
      '/api/auth/forgot-password',
      '/api/auth/reset-password',
      '/api/tenants/create',
      '/api/tenants/check-availability',
      '/api/tenants/current/branding',
      '/api/health',
      '/health'
    ];

    const isPublicRoute = publicRoutes.some(route => req.url.includes(route));
    
    // Para rutas públicas, continuar sin modificaciones
    if (isPublicRoute) {
      return next.handle(req).pipe(
        catchError(error => this.handleError(error, req))
      );
    }

    // Para rutas protegidas, agregar tenant header
    return this.tenantService.getTenantInfo().pipe(
      take(1),
      switchMap(tenant => {
        let modifiedReq = req;

        // Agregar header X-Tenant-ID si hay tenant
        if (tenant?.tenantId) {
          modifiedReq = req.clone({
            setHeaders: {
              'X-Tenant-ID': tenant.tenantId
            }
          });
        }

        // Agregar headers adicionales para autenticación
        const token = this.getAuthToken();
        if (token) {
          modifiedReq = modifiedReq.clone({
            setHeaders: {
              ...modifiedReq.headers,
              'Authorization': `Bearer ${token}`
            }
          });
        }

        return next.handle(modifiedReq).pipe(
          catchError(error => this.handleError(error, modifiedReq))
        );
      })
    );
  }

  private getAuthToken(): string | null {
    try {
      return localStorage.getItem('auth_token');
    } catch {
      return null;
    }
  }

  private handleError(error: HttpErrorResponse, req: HttpRequest<any>): Observable<never> {
    console.error('HTTP Error:', error);

    // Errores específicos de tenant
    if (error.status === 403 && error.error?.includes?.('Tenant')) {
      console.error('Tenant access denied:', error.error);
      this.router.navigate(['/tenant-error'], { 
        queryParams: { 
          error: 'access_denied',
          message: 'No tienes acceso a esta empresa'
        }
      });
    }

    // Tenant no encontrado o inactivo
    if (error.status === 404 && error.url?.includes('/api/tenants/')) {
      console.error('Tenant not found:', error.error);
      this.router.navigate(['/tenant-error'], { 
        queryParams: { 
          error: 'tenant_not_found',
          message: 'La empresa especificada no existe o está inactiva'
        }
      });
    }

    // Error de autenticación
    if (error.status === 401) {
      console.error('Authentication error:', error.error);
      
      // Limpiar token inválido
      localStorage.removeItem('auth_token');
      
      // Redirigir a login preservando tenant
      const currentTenant = this.tenantService.getCurrentTenantId();
      const loginRoute = currentTenant ? ['/login'] : ['/login'];
      const queryParams = currentTenant ? { tenant: currentTenant } : {};
      
      this.router.navigate(loginRoute, { queryParams });
    }

    // Error del servidor
    if (error.status >= 500) {
      console.error('Server error:', error.error);
      // Aquí podrías mostrar un toast de error global
    }

    // Error de red
    if (error.status === 0) {
      console.error('Network error:', error.error);
      // Aquí podrías mostrar un mensaje de "Sin conexión"
    }

    return throwError(() => error);
  }
}