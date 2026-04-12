import { Injectable, OnDestroy } from '@angular/core';
import {
  ApplicationInsights,
  SeverityLevel,
  IExceptionTelemetry,
  ITraceTelemetry,
  IEventTelemetry,
  IPageViewTelemetry,
} from '@microsoft/applicationinsights-web';
import { environment } from '../../environments/environment';

/**
 * Enterprise Application Insights service.
 *
 * Wraps the @microsoft/applicationinsights-web SDK and exposes a typed,
 * testable API for telemetry collection. The instance is initialised only
 * when a non-empty connection string is present in the environment, so the
 * service is a safe no-op in development and CI builds.
 *
 * Usage:
 *   1. Inject AppInsightsService wherever you need telemetry.
 *   2. All public methods guard against an uninitialised SDK — no try/catch needed at call sites.
 *   3. Call setAuthenticatedUserContext() after login and
 *      clearAuthenticatedUserContext() after logout.
 */
@Injectable({
  providedIn: 'root',
})
export class AppInsightsService implements OnDestroy {

  private appInsights: ApplicationInsights | null = null;
  private readonly isEnabled: boolean;

  constructor() {
    const connectionString = environment.appInsightsConnectionString?.trim();
    this.isEnabled = !!connectionString;

    if (this.isEnabled) {
      this.appInsights = new ApplicationInsights({
        config: {
          connectionString,
          /* ── Behaviour ──────────────────────────────────────────────── */
          enableAutoRouteTracking: true,          // Track SPA page views automatically
          enableRequestHeaderTracking: true,      // Include request headers in dep telemetry
          enableResponseHeaderTracking: false,    // Reduce PII exposure
          enableCorsCorrelation: true,            // Propagate correlation headers
          correlationHeaderExcludedDomains: [],   // Leave empty; restrict if needed
          /* ── Sampling (keep all events in prod unless volume spikes) ── */
          samplingPercentage: 100,
          /* ── Self-diagnostics (disable in prod to reduce noise) ─────── */
          enableDebug: false,
          loggingLevelConsole: 0,                 // 0 = silent SDK self-logs
          loggingLevelTelemetry: 1,               // 1 = only critical internal errors
          /* ── Session ────────────────────────────────────────────────── */
          sessionExpirationMs: 30 * 60 * 1000,   // 30 minutes
          /* ── Exception auto-collection ──────────────────────────────── */
          disableExceptionTracking: false,
          /* ── Performance auto-collection ────────────────────────────── */
          disableAjaxTracking: false,
          disableFetchTracking: false,
        },
      });

      this.appInsights.loadAppInsights();
      this.appInsights.addTelemetryInitializer((envelope) => {
        // Tag every envelope with the app version for easier filtering
        envelope.tags ??= {};
        envelope.tags['ai.application.ver'] = environment.version ?? '1.0.0';
      });
    }
  }

  // ──────────────────────────────────────────────────────────────────────────
  // Exception tracking
  // ──────────────────────────────────────────────────────────────────────────

  /**
   * Tracks an exception. Pass a real `Error` object whenever possible.
   *
   * @param error      The Error or string that triggered the event.
   * @param severityLevel  Defaults to SeverityLevel.Error.
   * @param properties Optional bag of extra dimensions for filtering in the portal.
   */
  trackException(
    error: Error | string,
    severityLevel: SeverityLevel = SeverityLevel.Error,
    properties?: Record<string, string>,
  ): void {
    if (!this.appInsights) return;

    const exception: IExceptionTelemetry = {
      exception: error instanceof Error ? error : new Error(String(error)),
      severityLevel,
      properties,
    };
    this.appInsights.trackException(exception);
  }

  // ──────────────────────────────────────────────────────────────────────────
  // Trace (structured log forwarding)
  // ──────────────────────────────────────────────────────────────────────────

  /**
   * Forwards a trace message to Application Insights.
   * Use for informational runtime events that are not errors.
   */
  trackTrace(
    message: string,
    severityLevel: SeverityLevel = SeverityLevel.Information,
    properties?: Record<string, string>,
  ): void {
    if (!this.appInsights) return;

    const trace: ITraceTelemetry = { message, severityLevel, properties };
    this.appInsights.trackTrace(trace);
  }

  // ──────────────────────────────────────────────────────────────────────────
  // Custom events
  // ──────────────────────────────────────────────────────────────────────────

  /**
   * Tracks a named business event (e.g. "PolizaCreated", "ReclamoSubmitted").
   * Useful for funnel and conversion analytics.
   */
  trackEvent(
    name: string,
    properties?: Record<string, string>,
    measurements?: Record<string, number>,
  ): void {
    if (!this.appInsights) return;

    const event: IEventTelemetry = { name, properties, measurements };
    this.appInsights.trackEvent(event);
  }

  // ──────────────────────────────────────────────────────────────────────────
  // Page views
  // ──────────────────────────────────────────────────────────────────────────

  /**
   * Manually tracks a page view. Prefer enabling `enableAutoRouteTracking`
   * in the config above; call this only for virtual sub-views.
   */
  trackPageView(
    name: string,
    uri?: string,
    properties?: Record<string, string>,
  ): void {
    if (!this.appInsights) return;

    const pv: IPageViewTelemetry = { name, uri, properties };
    this.appInsights.trackPageView(pv);
  }

  // ──────────────────────────────────────────────────────────────────────────
  // Authentication context
  // ──────────────────────────────────────────────────────────────────────────

  /**
   * Associates subsequent telemetry with an authenticated user.
   * Call after a successful login.
   *
   * @param userId      Opaque user identifier (e.g. DB primary key as string).
   *                    MUST NOT contain PII such as email or full name.
   * @param accountId   Optional tenant/broker account identifier.
   */
  setAuthenticatedUserContext(userId: string, accountId?: string): void {
    if (!this.appInsights) return;
    this.appInsights.setAuthenticatedUserContext(userId, accountId, true);
  }

  /**
   * Clears the authenticated user context.
   * Call on logout.
   */
  clearAuthenticatedUserContext(): void {
    if (!this.appInsights) return;
    this.appInsights.clearAuthenticatedUserContext();
  }

  // ──────────────────────────────────────────────────────────────────────────
  // Lifecycle
  // ──────────────────────────────────────────────────────────────────────────

  /**
   * Flushes buffered telemetry on service destruction (e.g. app teardown).
   */
  ngOnDestroy(): void {
    if (this.appInsights) {
      this.appInsights.flush();
    }
  }
}
