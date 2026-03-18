export type ChatSessionStatus = 'Active' | 'Closed' | 'Archived';
export type ChatMessageType = 'User' | 'Bot' | 'System';
export type ChatMessageStatus = 'Sent' | 'Read' | 'Error';

export interface ChatSession {
  id: number;
  sessionId: string;
  title: string;
  status: ChatSessionStatus;
  lastMessage?: string;
  messageCount: number;
  createdAt: string;
  lastActivityAt?: string;
  unreadCount: number;
}

export interface ChatMessage {
  id: number;
  chatSessionId: number;
  content: string;
  messageType: ChatMessageType;
  status: ChatMessageStatus;
  richContent?: ChatRichContent | null;
  quickReplies?: string[] | null;
  reactionScore?: number | null;
  isRead: boolean;
  processingTimeMs?: number | null;
  createdAt: string;
  // UI-only state (not from server)
  isStreaming?: boolean;
}

// ── Rich Content Models ────────────────────────────────────────────────────────

export type RichContentType =
  | 'polizas_table'
  | 'cobros_summary'
  | 'reclamos_summary'
  | 'stats_dashboard';

export interface ChatRichContent {
  type: RichContentType;
  title: string;
  rows?: PolizaRow[];
  stats?: StatCard[];
  cards?: DashboardCard[];
  recentPendientes?: CobroRow[];
  montoTotal?: string;
}

export interface PolizaRow {
  numeroPoliza: string;
  nombreAsegurado: string;
  aseguradora: string;
  vigencia: string;
  frecuencia: string;
}

export interface StatCard {
  label: string;
  value: string;
  color: 'primary' | 'warn' | 'info' | 'error';
}

export interface DashboardCard {
  icon: string;
  label: string;
  value: string;
  color: 'purple' | 'blue' | 'orange' | 'red' | 'green';
}

export interface CobroRow {
  numeroPoliza: string;
  clienteNombreCompleto: string;
  montoTotal: number;
  moneda: string;
  fechaVencimiento: string;
}

// ── DTOs (matching backend) ────────────────────────────────────────────────────

export interface CreateChatSessionDto {
  title?: string;
}

export interface SendMessageDto {
  content: string;
}

export interface SendMessageResponseDto {
  sessionId: string;
  sessionTitle: string;
  userMessage: ChatMessage;
  botResponse: ChatMessage;
}

export interface ChatSessionDetailDto {
  session: ChatSession;
  messages: ChatMessage[];
}

export interface ReactToMessageDto {
  score: number; // 1 or -1
}

export interface ChatStatsDto {
  totalSessions: number;
  totalMessages: number;
  activeSessions: number;
  avgMessagesPerSession: number;
  avgResponseTimeMs: number;
  likedResponses: number;
  dislikedResponses: number;
}
