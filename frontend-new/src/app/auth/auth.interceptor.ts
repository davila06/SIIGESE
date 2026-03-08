import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { TokenService } from '../services/token.service';
import { LoginResponse } from '../interfaces/user.interface';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(
    private router: Router,
    private tokenService: TokenService
  ) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (req.url.includes('/auth/refresh-token')) {
      return next.handle(req);
    }

    const authToken = sessionStorage.getItem('authToken');

    if (authToken) {
      if (this.tokenService.shouldRefreshToken(authToken) && !this.isRefreshing) {
        return this.handleTokenRefresh(req, next);
      }

      return next.handle(this.addToken(req, authToken)).pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401 && !req.url.includes('/auth/login')) {
            if (!this.isRefreshing) {
              return this.handleTokenRefresh(req, next);
            }
            this.clearSessionAndRedirect();
          }
          return throwError(() => error);
        })
      );
    }

    return next.handle(req);
  }

  private handleTokenRefresh(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.tokenService.refreshToken().pipe(
        switchMap((response: LoginResponse) => {
          this.isRefreshing = false;
          this.tokenService.updateToken(response);
          this.refreshTokenSubject.next(response.token);
          return next.handle(this.addToken(request, response.token));
        }),
        catchError((error) => {
          this.isRefreshing = false;
          this.clearSessionAndRedirect();
          return throwError(() => error);
        })
      );
    }

    return this.refreshTokenSubject.pipe(
      filter((token): token is string => token !== null),
      take(1),
      switchMap(token => next.handle(this.addToken(request, token)))
    );
  }

  private addToken(request: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
    return request.clone({
      headers: request.headers.set('Authorization', `Bearer ${token}`)
    });
  }

  private clearSessionAndRedirect(): void {
    sessionStorage.removeItem('authToken');
    sessionStorage.removeItem('currentUser');
    this.router.navigate(['/login']);
  }
}

