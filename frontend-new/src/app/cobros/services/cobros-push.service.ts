import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CobroPushNotification {
  type: string;
  requestId: number;
  cobroId: number;
  numeroRecibo: string;
  numeroPoliza?: string;
  estadoActual?: string;
  estadoSolicitado?: string;
  estadoSolicitud?: string;
  requestedBy?: string;
  resolvedBy?: string;
  createdAt?: string;
  resolvedAt?: string;
  reason?: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class CobrosPushService {
  private hubConnection: signalR.HubConnection | null = null;
  private readonly connected$ = new BehaviorSubject<boolean>(false);
  private readonly notifications$ = new BehaviorSubject<CobroPushNotification | null>(null);
  private hasLoggedConnectionFailure = false;

  readonly isConnected$ = this.connected$.asObservable();
  readonly notification$ = this.notifications$.asObservable();

  async start(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const token = sessionStorage.getItem('authToken');
    if (!token) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.resolveHubUrl(), {
        accessTokenFactory: () => token,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      // Limit reconnect attempts to avoid noisy infinite loops when API proxy is down.
      .withAutomaticReconnect([0, 2000, 5000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hubConnection.on('CobroEstadoChangeRequestNotification', (payload: CobroPushNotification) => {
      this.notifications$.next(payload);
    });

    this.hubConnection.onreconnected(() => {
      this.connected$.next(true);
    });

    this.hubConnection.onclose(() => {
      this.connected$.next(false);
    });

    try {
      await this.hubConnection.start();
      this.connected$.next(true);
      this.hasLoggedConnectionFailure = false;
    } catch {
      this.connected$.next(false);
      if (!this.hasLoggedConnectionFailure) {
        console.warn('[CobrosPushService] No se pudo establecer conexión SignalR.');
        this.hasLoggedConnectionFailure = true;
      }
    }
  }

  async stop(): Promise<void> {
    if (!this.hubConnection) {
      return;
    }

    try {
      await this.hubConnection.stop();
    } finally {
      this.hubConnection = null;
      this.connected$.next(false);
    }
  }

  private resolveHubUrl(): string {
    const apiUrl = environment.apiUrl || '';
    if (apiUrl.endsWith('/api')) {
      return apiUrl.slice(0, -4) + '/hubs/chat';
    }

    return '/hubs/chat';
  }
}
