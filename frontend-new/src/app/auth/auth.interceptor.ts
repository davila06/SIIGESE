import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Obtener el token de autenticación desde localStorage
    const authToken = localStorage.getItem('authToken');
    
    // Si hay token, agregarlo al header Authorization
    if (authToken) {
      const authReq = req.clone({
        headers: req.headers.set('Authorization', `Bearer ${authToken}`)
      });
      console.log('🔐 AuthInterceptor: Agregando token a la request:', {
        url: req.url,
        method: req.method,
        token: authToken.substring(0, 20) + '...'
      });
      return next.handle(authReq);
    }
    
    console.log('⚠️ AuthInterceptor: No hay token disponible para:', req.url);
    return next.handle(req);
  }
}
