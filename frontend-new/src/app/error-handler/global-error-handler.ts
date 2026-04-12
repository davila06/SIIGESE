import { ErrorHandler, Injectable, NgZone, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { SeverityLevel } from '@microsoft/applicationinsights-web';
import { AppInsightsService } from '../services/app-insights.service';
import { environment } from '../../environments/environment';

/**
 * Enterprise GlobalErrorHandler
 *
 * Replaces Angular's default ErrorHandler to:
 *   1. Forward unhandled exceptions to Application Insights in production.
 *   2. Classify errors by type (HTTP vs chunk-load vs generic) for richer telemetry.
 *   3. In development, preserve the normal console output so debugging is unaffected.
 *   4. Never throw during error handling — a secondary exception in the handler
 *      would mask the original error and destabilise the application.
 *
 * Registration: provide in AppModule via { provide: ErrorHandler, useClass: GlobalErrorHandler }.
 */
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {

  private readonly appInsights = inject(AppInsightsService);
  private readonly zone = inject(NgZone);
  private readonly isProduction = environment.production;

  handleError(error: unknown): void {
    // Run outside Angular zone to avoid unnecessary change-detection cycles
    // triggered by the monitoring SDK's own async callbacks.
    this.zone.runOutsideAngular(() => {
      try {
        if (error instanceof HttpErrorResponse) {
          this.handleHttpError(error);
        } else if (this.isChunkLoadError(error)) {
          this.handleChunkLoadError(error as Error);
        } else {
          this.handleGenericError(error);
        }
      } catch {
        // Safety net: never let the error handler itself crash the app.
        // Intentionally swallowed — we already attempted to report the original error.
      }
    });
  }

  // ──────────────────────────────────────────────────────────────────────────
  // Private helpers
  // ──────────────────────────────────────────────────────────────────────────

  private handleHttpError(error: HttpErrorResponse): void {
    const severity = error.status >= 500
      ? SeverityLevel.Error
      : SeverityLevel.Warning;

    const properties: Record<string, string> = {
      statusCode: String(error.status),
      statusText: error.statusText ?? '',
      url: error.url ?? '',
      errorType: 'HttpError',
    };

    this.appInsights.trackException(
      new Error(`HTTP ${error.status} — ${error.url ?? 'unknown'}`),
      severity,
      properties,
    );

    if (!this.isProduction) {
      console.error('[GlobalErrorHandler] HTTP error', error);
    }
  }

  private handleChunkLoadError(error: Error): void {
    // Lazy-loaded chunk not found — usually caused by a new deployment while
    // the user has the old bundle cached. Track and attempt a soft reload.
    this.appInsights.trackException(error, SeverityLevel.Warning, {
      errorType: 'ChunkLoadError',
      message: error.message,
    });

    if (!this.isProduction) {
      console.warn('[GlobalErrorHandler] Chunk load error — consider reloading', error);
    }

    // Reload once to pick up the new bundle; guard against reload loops.
    const reloadFlag = sessionStorage.getItem('_chunkReload');
    if (!reloadFlag) {
      sessionStorage.setItem('_chunkReload', '1');
      window.location.reload();
    }
  }

  private handleGenericError(error: unknown): void {
    const err = error instanceof Error
      ? error
      : new Error(typeof error === 'string' ? error : JSON.stringify(error));

    this.appInsights.trackException(err, SeverityLevel.Error, {
      errorType: 'UnhandledException',
    });

    if (!this.isProduction) {
      console.error('[GlobalErrorHandler] Unhandled error', error);
    }
  }

  /** Detects Webpack/Rollup lazy-chunk loading failures. */
  private isChunkLoadError(error: unknown): boolean {
    if (!(error instanceof Error)) return false;
    return (
      error.name === 'ChunkLoadError' ||
      /loading chunk \d+ failed/i.test(error.message) ||
      /failed to fetch dynamically imported module/i.test(error.message)
    );
  }
}
