import {
  Component,
  OnInit,
  OnDestroy,
  ViewChild,
  ElementRef,
  AfterViewChecked,
  ChangeDetectorRef,
  ChangeDetectionStrategy,
  HostListener,
} from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { ChatService } from '../services/chat.service';
import {
  ChatSession,
  ChatMessage,
  ChatRichContent,
} from '../interfaces/chat.interface';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Default,
})
export class ChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('messageInput') messageInput!: ElementRef<HTMLTextAreaElement>;

  // ── State ──────────────────────────────────────────────────────────────────
  sessions: ChatSession[] = [];
  activeSession: ChatSession | null = null;
  messages: ChatMessage[] = [];

  isLoadingSessions = false;
  isLoadingMessages = false;
  isSendingMessage = false;
  isSignalRConnected = false;
  isSidebarOpen = true;
  isTyping = false;

  searchSessionTerm = '';
  newSessionTitle = '';

  readonly messageControl = new FormControl('', [
    Validators.required,
    Validators.minLength(1),
    Validators.maxLength(1000),
  ]);

  private readonly destroy$ = new Subject<void>();
  private shouldScrollToBottom = false;

  // ── Typing animation ───────────────────────────────────────────────────────
  readonly typingDots = [0, 1, 2];

  constructor(
    private readonly chatService: ChatService,
    private readonly snackBar: MatSnackBar,
    private readonly cdr: ChangeDetectorRef
  ) {}

  // ── Lifecycle ──────────────────────────────────────────────────────────────

  ngOnInit(): void {
    this.adjustLayoutForViewport();

    this.chatService.sessions
      .pipe(takeUntil(this.destroy$))
      .subscribe(s => {
        this.sessions = s;
        this.cdr.markForCheck();
      });

    this.chatService.typingIndicator
      .pipe(takeUntil(this.destroy$))
      .subscribe(typing => {
        this.isTyping = typing;
        if (typing) this.shouldScrollToBottom = true;
        this.cdr.markForCheck();
      });

    this.chatService.isConnected
      .pipe(takeUntil(this.destroy$))
      .subscribe(connected => {
        this.isSignalRConnected = connected;
        this.cdr.markForCheck();
      });

    this.chatService.newMessage
      .pipe(takeUntil(this.destroy$))
      .subscribe(msg => {
        if (this.activeSession && msg.chatSessionId) {
          // Only add if not already in list (avoid duplicates with HTTP response)
          const exists = this.messages.some(m => m.id === msg.id);
          if (!exists) {
            this.messages = [...this.messages, msg];
            this.shouldScrollToBottom = true;
            this.cdr.markForCheck();
          }
        }
      });

    this.loadSessions();
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.chatService.disconnect();
  }

  // ── Sessions ───────────────────────────────────────────────────────────────

  loadSessions(): void {
    this.isLoadingSessions = true;
    this.chatService.getSessions()
      .pipe(finalize(() => { this.isLoadingSessions = false; }))
      .subscribe({
        next: sessions => {
          // Auto-open first session
          if (sessions.length > 0 && !this.activeSession) {
            this.openSession(sessions[0]);
          }
        },
        error: () => this.snackBar.open('Error cargando sesiones', 'Cerrar', { duration: 3000 })
      });
  }

  createNewSession(): void {
    this.chatService.createSession({ title: 'Nueva conversación' }).subscribe({
      next: session => {
        this.openSession(session);
        this.snackBar.open('Nueva sesión creada', '', { duration: 2000 });
      },
      error: () => this.snackBar.open('Error al crear sesión', 'Cerrar', { duration: 3000 })
    });
  }

  openSession(session: ChatSession): void {
    if (this.activeSession?.sessionId === session.sessionId) return;

    this.activeSession = session;
    this.messages = [];
    this.isLoadingMessages = true;
    this.isTyping = false;

    this.chatService.getSession(session.sessionId)
      .pipe(finalize(() => { this.isLoadingMessages = false; }))
      .subscribe({
        next: detail => {
          this.activeSession = detail.session;
          this.messages = detail.messages;
          this.shouldScrollToBottom = true;
          this.chatService.connectToSession(session.sessionId);
          if (window.innerWidth <= 1024) {
            this.isSidebarOpen = false;
          }
          if (session.unreadCount > 0) {
            this.chatService.markAsRead(session.sessionId).subscribe();
          }
        },
        error: () => {
          this.snackBar.open('Error cargando conversación', 'Cerrar', { duration: 3000 });
          this.isLoadingMessages = false;
        }
      });
  }

  deleteSession(session: ChatSession, event: Event): void {
    event.stopPropagation();
    if (!confirm(`¿Eliminar la conversación "${session.title}"?`)) return;

    this.chatService.deleteSession(session.sessionId).subscribe({
      next: () => {
        if (this.activeSession?.sessionId === session.sessionId) {
          this.activeSession = null;
          this.messages = [];
          const remaining = this.sessions.filter(s => s.sessionId !== session.sessionId);
          if (remaining.length > 0) this.openSession(remaining[0]);
        }
        this.snackBar.open('Conversación eliminada', '', { duration: 2000 });
      },
      error: () => this.snackBar.open('Error eliminando conversación', 'Cerrar', { duration: 3000 })
    });
  }

  // ── Messages ───────────────────────────────────────────────────────────────

  sendMessage(): void {
    const content = (this.messageControl.value ?? '').trim();
    if (!content || this.isSendingMessage || !this.activeSession) return;

    this.messageControl.disable();
    this.isSendingMessage = true;
    this.isTyping = true;

    // Optimistic UI: add user message immediately
    const optimisticMsg: ChatMessage = {
      id: -Date.now(),
      chatSessionId: this.activeSession.id,
      content,
      messageType: 'User',
      status: 'Sent',
      isRead: true,
      createdAt: new Date().toISOString(),
      isStreaming: false,
    };
    this.messages = [...this.messages, optimisticMsg];
    this.messageControl.reset();
    this.shouldScrollToBottom = true;

    this.chatService
      .sendMessage(this.activeSession.sessionId, { content })
      .pipe(finalize(() => {
        this.isSendingMessage = false;
        this.isTyping = false;
        this.messageControl.enable();
        setTimeout(() => this.messageInput?.nativeElement?.focus(), 50);
      }))
      .subscribe({
        next: response => {
          // Replace optimistic message with server response
          this.messages = this.messages
            .filter(m => m.id !== optimisticMsg.id)
            .concat([response.userMessage, response.botResponse]);

          // Update session title if it changed
          if (this.activeSession) {
            this.activeSession = { ...this.activeSession, title: response.sessionTitle };
          }
          this.shouldScrollToBottom = true;
        },
        error: () => {
          // Remove failed optimistic message
          this.messages = this.messages.filter(m => m.id !== optimisticMsg.id);
          this.snackBar.open('Error al enviar el mensaje', 'Cerrar', { duration: 4000 });
        }
      });
  }

  sendQuickReply(reply: string): void {
    if (!this.activeSession || this.isSendingMessage) return;
    this.messageControl.setValue(reply);
    this.sendMessage();
  }

  reactToMessage(message: ChatMessage, score: number): void {
    if (message.messageType !== 'Bot') return;

    const newScore = message.reactionScore === score ? null : score;
    // Optimistic update
    message.reactionScore = newScore as number | null;

    this.chatService.reactToMessage(message.id, { score }).subscribe({
      error: () => {
        message.reactionScore = null;
        this.snackBar.open('Error al registrar reacción', 'Cerrar', { duration: 2000 });
      }
    });
  }

  onEnterKey(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  // ── UI Helpers ─────────────────────────────────────────────────────────────

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    this.adjustLayoutForViewport();
  }

  get filteredSessions(): ChatSession[] {
    const term = this.searchSessionTerm.toLowerCase().trim();
    if (!term) return this.sessions;
    return this.sessions.filter(s =>
      s.title.toLowerCase().includes(term) ||
      (s.lastMessage ?? '').toLowerCase().includes(term)
    );
  }

  isActiveSession(session: ChatSession): boolean {
    return this.activeSession?.sessionId === session.sessionId;
  }

  getSessionInitial(session: ChatSession): string {
    return session.title.charAt(0).toUpperCase();
  }

  getMessageDate(message: ChatMessage): string {
    return new Date(message.createdAt).toLocaleTimeString('es-CR', {
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  getSessionRelativeTime(session: ChatSession): string {
    const date = new Date(session.lastActivityAt ?? session.createdAt);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Ahora';
    if (diffMins < 60) return `Hace ${diffMins}m`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `Hace ${diffHours}h`;
    const diffDays = Math.floor(diffHours / 24);
    if (diffDays === 1) return 'Ayer';
    return `Hace ${diffDays}d`;
  }

  showDateSeparator(index: number): boolean {
    if (index === 0) return true;
    const current = new Date(this.messages[index].createdAt).toDateString();
    const previous = new Date(this.messages[index - 1].createdAt).toDateString();
    return current !== previous;
  }

  getDateSeparatorText(message: ChatMessage): string {
    const date = new Date(message.createdAt);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(today.getDate() - 1);

    if (date.toDateString() === today.toDateString()) return 'Hoy';
    if (date.toDateString() === yesterday.toDateString()) return 'Ayer';
    return date.toLocaleDateString('es-CR', { day: 'numeric', month: 'long', year: 'numeric' });
  }

  richContentIsTable(richContent: ChatRichContent): boolean {
    return richContent.type === 'polizas_table';
  }

  richContentIsStats(richContent: ChatRichContent): boolean {
    return richContent.type === 'cobros_summary' || richContent.type === 'reclamos_summary';
  }

  richContentIsDashboard(richContent: ChatRichContent): boolean {
    return richContent.type === 'stats_dashboard';
  }

  getStatColorClass(color: string): string {
    const map: Record<string, string> = {
      primary: 'stat-primary',
      warn: 'stat-warn',
      info: 'stat-info',
      error: 'stat-error',
    };
    return map[color] ?? 'stat-primary';
  }

  getDashboardCardColorClass(color: string): string {
    const map: Record<string, string> = {
      purple: 'card-purple',
      blue: 'card-blue',
      orange: 'card-orange',
      red: 'card-red',
      green: 'card-green',
    };
    return map[color] ?? 'card-blue';
  }

  trackByMessageId(_: number, msg: ChatMessage): number {
    return msg.id;
  }

  trackBySessionId(_: number, s: ChatSession): string {
    return s.sessionId;
  }

  private scrollToBottom(): void {
    try {
      const el = this.messagesContainer?.nativeElement;
      if (el) {
        el.scrollTop = el.scrollHeight;
      }
    } catch {
      // ignore
    }
  }

  private adjustLayoutForViewport(): void {
    this.isSidebarOpen = window.innerWidth > 1024;
  }
}
