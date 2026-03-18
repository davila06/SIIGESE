import { Injectable, OnDestroy, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, Subject, from, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import {
  ChatSession,
  ChatMessage,
  ChatSessionDetailDto,
  CreateChatSessionDto,
  SendMessageDto,
  SendMessageResponseDto,
  ReactToMessageDto,
  ChatStatsDto,
} from '../interfaces/chat.interface';
import { LoggingService } from '../../services/logging.service';

@Injectable({
  providedIn: 'root'
})
export class ChatService implements OnDestroy {
  private readonly apiUrl = `${environment.apiUrl}/chat`;
  private readonly hubUrl = this.resolveHubUrl();

  // ── State streams ─────────────────────────────────────────────────────────
  private readonly sessions$ = new BehaviorSubject<ChatSession[]>([]);
  private readonly isConnected$ = new BehaviorSubject<boolean>(false);
  private readonly typingIndicator$ = new BehaviorSubject<boolean>(false);
  private readonly newMessage$ = new Subject<ChatMessage>();

  readonly sessions = this.sessions$.asObservable();
  readonly isConnected = this.isConnected$.asObservable();
  readonly typingIndicator = this.typingIndicator$.asObservable();
  readonly newMessage = this.newMessage$.asObservable();

  private hubConnection: signalR.HubConnection | null = null;
  private currentSessionId: string | null = null;
  private typingTimeout: ReturnType<typeof setTimeout> | null = null;

  private readonly logger = inject(LoggingService);

  constructor(private readonly http: HttpClient) {}

  // ── HTTP API ──────────────────────────────────────────────────────────────

  getSessions(): Observable<ChatSession[]> {
    return this.http.get<ChatSession[]>(`${this.apiUrl}/sessions`).pipe(
      tap(sessions => this.sessions$.next(sessions)),
      catchError(err => {
        this.logger.error('[ChatService] getSessions error', err);
        return of([]);
      })
    );
  }

  createSession(dto: CreateChatSessionDto = {}): Observable<ChatSession> {
    return this.http.post<ChatSession>(`${this.apiUrl}/sessions`, dto).pipe(
      tap(session => {
        const current = this.sessions$.value;
        this.sessions$.next([session, ...current]);
      })
    );
  }

  getSession(sessionId: string): Observable<ChatSessionDetailDto> {
    return this.http.get<ChatSessionDetailDto>(`${this.apiUrl}/sessions/${sessionId}`);
  }

  deleteSession(sessionId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/sessions/${sessionId}`).pipe(
      tap(() => {
        const filtered = this.sessions$.value.filter(s => s.sessionId !== sessionId);
        this.sessions$.next(filtered);
      })
    );
  }

  markAsRead(sessionId: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/sessions/${sessionId}/read`, {});
  }

  sendMessage(sessionId: string, dto: SendMessageDto): Observable<SendMessageResponseDto> {
    return this.http.post<SendMessageResponseDto>(
      `${this.apiUrl}/sessions/${sessionId}/messages`, dto
    ).pipe(
      tap(response => this.updateSessionInList(response.sessionId, response.sessionTitle))
    );
  }

  reactToMessage(messageId: number, dto: ReactToMessageDto): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/messages/${messageId}/reaction`, dto);
  }

  getStats(): Observable<ChatStatsDto> {
    return this.http.get<ChatStatsDto>(`${this.apiUrl}/stats`);
  }

  // ── SignalR ───────────────────────────────────────────────────────────────

  async connectToSession(sessionId: string): Promise<void> {
    if (this.currentSessionId === sessionId && this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    await this.disconnect();
    this.currentSessionId = sessionId;

    const token = localStorage.getItem('token') ?? sessionStorage.getItem('token') ?? '';

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token,
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hubConnection.on('ReceiveMessage', (message: ChatMessage) => {
      this.newMessage$.next(message);
    });

    this.hubConnection.on('TypingIndicator', (data: { isTyping: boolean }) => {
      this.typingIndicator$.next(data.isTyping);
      if (data.isTyping) {
        if (this.typingTimeout) clearTimeout(this.typingTimeout);
        this.typingTimeout = setTimeout(() => this.typingIndicator$.next(false), 5000);
      }
    });

    this.hubConnection.onreconnected(() => {
      this.isConnected$.next(true);
      this.joinSession(sessionId);
    });

    this.hubConnection.onclose(() => {
      this.isConnected$.next(false);
    });

    try {
      await this.hubConnection.start();
      this.isConnected$.next(true);
      await this.joinSession(sessionId);
    } catch (err) {
      this.logger.warn('[ChatService] SignalR connection failed (falling back to HTTP polling):', err);
      this.isConnected$.next(false);
    }
  }

  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
      } catch {
        // ignore disconnection errors
      }
      this.hubConnection = null;
    }
    this.isConnected$.next(false);
    this.typingIndicator$.next(false);
    this.currentSessionId = null;
  }

  private async joinSession(sessionId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('JoinSession', sessionId);
      } catch (err) {
        this.logger.warn('[ChatService] JoinSession failed:', err);
      }
    }
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private updateSessionInList(sessionId: string, title: string): void {
    const sessions = this.sessions$.value.map(s =>
      s.sessionId === sessionId ? { ...s, title, lastActivityAt: new Date().toISOString() } : s
    );
    // Move updated session to top
    const idx = sessions.findIndex(s => s.sessionId === sessionId);
    if (idx > 0) {
      const [updated] = sessions.splice(idx, 1);
      sessions.unshift(updated);
    }
    this.sessions$.next(sessions);
  }

  private resolveHubUrl(): string {
    const apiUrl = environment.apiUrl;
    if (apiUrl.startsWith('http://') || apiUrl.startsWith('https://')) {
      // Local dev: strip /api to get base
      return apiUrl.replace(/\/api$/, '') + '/hubs/chat';
    }
    // Proxy-based: relative URL
    return '/hubs/chat';
  }

  ngOnDestroy(): void {
    this.disconnect();
    if (this.typingTimeout) clearTimeout(this.typingTimeout);
  }
}
