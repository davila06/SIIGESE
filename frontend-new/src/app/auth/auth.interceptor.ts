import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // For now, just pass through the request without modification
    // In a real implementation, you would add authentication headers here
    
    // Example of adding an auth header:
    // const authToken = localStorage.getItem('authToken');
    // if (authToken) {
    //   const authReq = req.clone({
    //     headers: req.headers.set('Authorization', `Bearer ${authToken}`)
    //   });
    //   return next.handle(authReq);
    // }
    
    return next.handle(req);
  }
}
