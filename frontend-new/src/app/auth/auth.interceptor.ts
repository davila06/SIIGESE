import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { TokenService } from '../services/token.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);
  
  constructor(
    private router: Router,
    private tokenService: TokenService
  ) {}
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // No agregar token a la petición de refresh-token
    if (req.url.includes('/auth/refresh-token')) {
      return next.handle(req);
    }

    // Obtener el token de autenticación desde localStorage
    const authToken = localStorage.getItem('authToken');
    
    // Si hay token, agregarlo al header Authorization
    if (authToken) {
      // Verificar si el token necesita renovarse antes de hacer la petición
      if (this.tokenService.shouldRefreshToken(authToken) && !this.isRefreshing) {
        console.log('🔄 Token próximo a expirar, renovando antes de la petición...');
        return this.handleTokenRefresh(req, next);
      }

      const authReq = req.clone({
        headers: req.headers.set('Authorization', `Bearer ${authToken}`)
      });
      
      console.log('🔐 AuthInterceptor: Agregando token a la request:', {
        url: req.url,
        method: req.method,
        token: authToken.substring(0, 20) + '...'
      });
      
      return next.handle(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401) {
            // Si es 401 y no estamos renovando, intentar renovar el token
            if (!this.isRefreshing && !req.url.includes('/auth/login')) {
              console.log('⚠️ Recibido 401, intentando renovar token...');
              return this.handleTokenRefresh(req, next);
            }
            
            // Si ya estamos renovando o es la petición de login, hacer logout
            console.error('❌ Token inválido o expirado. Limpiando sesión...');
            localStorage.removeItem('authToken');
            localStorage.removeItem('currentUser');
            this.router.navigate(['/login']);
          }
          return throwError(() => error);
        })
      );
    }
    
    console.log('⚠️ AuthInterceptor: No hay token disponible para:', req.url);
    return next.handle(req);
  }

  private handleTokenRefresh(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.tokenService.refreshToken().pipe(
        switchMap((response: any) => {
          this.isRefreshing = false;
          this.tokenService.updateToken(response);
          this.refreshTokenSubject.next(response.token);
          
          // Reintentar la petición original con el nuevo token
          const clonedRequest = this.addToken(request, response.token);
          return next.handle(clonedRequest);
        }),
        catchError((error) => {
          this.isRefreshing = false;
          
          // Si falla la renovación, hacer logout
          console.error('❌ Error renovando token. Cerrando sesión...');
          localStorage.removeItem('authToken');
          localStorage.removeItem('currentUser');
          this.router.navigate(['/login']);
          
          return throwError(() => error);
        })
      );
    } else {
      // Si ya hay una renovación en progreso, esperar a que termine
      return this.refreshTokenSubject.pipe(
        filter(token => token != null),
        take(1),
        switchMap(token => {
          const clonedRequest = this.addToken(request, token);
          return next.handle(clonedRequest);
        })
      );
    }
  }

  private addToken(request: HttpRequest<any>, token: string): HttpRequest<any> {
    return request.clone({
      headers: request.headers.set('Authorization', `Bearer ${token}`)
    });
  }
}
