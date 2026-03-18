import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

/**
 * Centralised logging service.
 *
 * In production (`environment.production === true`) all output is suppressed
 * to avoid leaking internal state to the browser console.
 *
 * Future enhancement: route `error()` calls to Application Insights,
 * Sentry, or another remote monitoring back-end instead of just silencing them.
 */
@Injectable({
  providedIn: 'root'
})
export class LoggingService {

  private readonly isProduction = environment.production;

  log(message: string, ...args: unknown[]): void {
    if (!this.isProduction) {
      console.log(message, ...args);
    }
  }

  warn(message: string, ...args: unknown[]): void {
    if (!this.isProduction) {
      console.warn(message, ...args);
    }
  }

  /**
   * Logs an error.
   * In production the call is silenced; wire a remote monitoring provider
   * here (e.g. Application Insights `trackException`) when ready.
   */
  error(message: string, ...args: unknown[]): void {
    if (!this.isProduction) {
      console.error(message, ...args);
    }
    // TODO [OPS]: forward to Application Insights / Sentry in production
  }
}
