import { Injectable, inject } from '@angular/core';
import { SeverityLevel } from '@microsoft/applicationinsights-web';
import { environment } from '../../environments/environment';
import { AppInsightsService } from './app-insights.service';

/**
 * Centralised logging service.
 *
 * Routing matrix:
 *  ┌────────────┬─────────────────────────┬────────────────────────────────┐
 *  │ Method     │ Development             │ Production                     │
 *  ├────────────┼─────────────────────────┼────────────────────────────────┤
 *  │ log()      │ console.log             │ trackTrace (Information)        │
 *  │ warn()     │ console.warn            │ trackTrace (Warning)            │
 *  │ error()    │ console.error           │ trackException + trackTrace     │
 *  └────────────┴─────────────────────────┴────────────────────────────────┘
 *
 * Pass an `Error` object as the first extra argument to `error()` to preserve
 * the original stack trace in Application Insights:
 *
 *   this.logger.error('Upload failed', uploadError, { fileSize: '10MB' });
 *
 * The service delegates to AppInsightsService which is a safe no-op when
 * the connection string is absent (i.e. dev / CI environments).
 */
@Injectable({
  providedIn: 'root',
})
export class LoggingService {

  private readonly appInsights = inject(AppInsightsService);
  private readonly isProduction = environment.production;

  // ──────────────────────────────────────────────────────────────────────────
  // Public API
  // ──────────────────────────────────────────────────────────────────────────

  /**
   * Logs an informational message.
   * In production the message is forwarded to Application Insights as a Trace.
   */
  log(message: string, ...args: unknown[]): void {
    if (!this.isProduction) {
      console.log(message, ...args);
      return;
    }

    this.appInsights.trackTrace(
      message,
      SeverityLevel.Information,
      this.extractStringProperties(args),
    );
  }

  /**
   * Logs a warning.
   * In production the warning is forwarded to Application Insights as a Warning Trace.
   */
  warn(message: string, ...args: unknown[]): void {
    if (!this.isProduction) {
      console.warn(message, ...args);
      return;
    }

    this.appInsights.trackTrace(
      message,
      SeverityLevel.Warning,
      this.extractStringProperties(args),
    );
  }

  /**
   * Logs an error.
   * In production: tracks the exception with Application Insights and also
   * emits a Warning Trace so the message appears in the Traces table.
   *
   * @param message  Human-readable description of what went wrong.
   * @param args     Optional extra context. If the first element is an `Error`
   *                 its stack is forwarded to Application Insights; remaining
   *                 elements are serialised as custom properties.
   */
  error(message: string, ...args: unknown[]): void {
    if (!this.isProduction) {
      console.error(message, ...args);
      return;
    }

    // Extract an Error instance if provided so the stack trace is preserved.
    const [firstArg, ...rest] = args;
    const errorObj: Error = firstArg instanceof Error
      ? firstArg
      : new Error(message);

    const properties: Record<string, string> = {
      ...this.extractStringProperties(firstArg instanceof Error ? rest : args),
      logMessage: message,
    };

    this.appInsights.trackException(errorObj, SeverityLevel.Error, properties);
  }

  // ──────────────────────────────────────────────────────────────────────────
  // Private helpers
  // ──────────────────────────────────────────────────────────────────────────

  /**
   * Converts an array of unknown log arguments into a flat string property bag.
   * Objects are serialised via JSON; primitives are coerced to string.
   * Non-serialisable values (circular refs) are represented as '[Unserializable]'.
   */
  private extractStringProperties(
    args: unknown[],
  ): Record<string, string> | undefined {
    if (!args.length) return undefined;

    const properties: Record<string, string> = {};
    args.forEach((arg, index) => {
      const key = `arg${index}`;
      if (arg === null || arg === undefined) {
        properties[key] = String(arg);
      } else if (typeof arg === 'string' || typeof arg === 'number' || typeof arg === 'boolean') {
        properties[key] = String(arg);
      } else {
        try {
          properties[key] = JSON.stringify(arg);
        } catch {
          properties[key] = '[Unserializable]';
        }
      }
    });

    return Object.keys(properties).length ? properties : undefined;
  }
}
