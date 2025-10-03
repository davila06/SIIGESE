import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface EmailRequest {
  toEmail: string;
  toName?: string;
  subject: string;
  body: string;
  emailType?: string;
  relatedEntityId?: number;
  relatedEntityType?: string;
}

export interface BulkEmailRequest {
  emails: EmailRequest[];
}

export interface CobroVencidoEmailData {
  clienteNombre: string;
  clienteEmail: string;
  numeroPoliza: string;
  montoVencido: number;
  fechaVencimiento: Date;
  diasVencido: number;
}

export interface ReclamoRecibidoEmailData {
  clienteNombre: string;
  clienteEmail: string;
  numeroReclamo: string;
  numeroPoliza: string;
  fechaReclamo: Date;
  descripcion: string;
}

export interface BienvenidaEmailData {
  nombreUsuario: string;
  email: string;
  userName: string;
  temporalPassword: string;
  roles: string[];
}

export interface EmailResponse {
  isSuccess: boolean;
  message: string;
  emailLogId?: number;
}

export interface EmailHistoryResponse {
  id: number;
  toEmail: string;
  toName?: string;
  subject: string;
  emailType: string;
  sentAt: Date;
  isSuccess: boolean;
  errorMessage?: string;
  senderName: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmailService {
  private apiUrl = `${environment.apiUrl}/api/email`;

  constructor(private http: HttpClient) { }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  // Envío manual de email
  sendEmail(request: EmailRequest): Observable<EmailResponse> {
    return this.http.post<EmailResponse>(`${this.apiUrl}/send`, request, {
      headers: this.getHeaders()
    });
  }

  // Envío masivo de emails
  sendBulkEmails(request: BulkEmailRequest): Observable<EmailResponse[]> {
    return this.http.post<EmailResponse[]>(`${this.apiUrl}/send-bulk`, request, {
      headers: this.getHeaders()
    });
  }

  // Envío de notificación de cobro vencido
  sendCobroVencidoEmail(data: CobroVencidoEmailData): Observable<EmailResponse> {
    return this.http.post<EmailResponse>(`${this.apiUrl}/send-cobro-vencido`, data, {
      headers: this.getHeaders()
    });
  }

  // Envío de notificación de reclamo recibido
  sendReclamoRecibidoEmail(data: ReclamoRecibidoEmailData): Observable<EmailResponse> {
    return this.http.post<EmailResponse>(`${this.apiUrl}/send-reclamo-recibido`, data, {
      headers: this.getHeaders()
    });
  }

  // Envío de email de bienvenida
  sendBienvenidaEmail(data: BienvenidaEmailData): Observable<EmailResponse> {
    return this.http.post<EmailResponse>(`${this.apiUrl}/send-bienvenida`, data, {
      headers: this.getHeaders()
    });
  }

  // Obtener historial de emails
  getEmailHistory(pageNumber: number = 1, pageSize: number = 10): Observable<EmailHistoryResponse[]> {
    return this.http.get<EmailHistoryResponse[]>(`${this.apiUrl}/history`, {
      headers: this.getHeaders(),
      params: {
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString()
      }
    });
  }

  // Obtener estadísticas de emails
  getEmailStats(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/stats`, {
      headers: this.getHeaders()
    });
  }

  // Envío automático de notificaciones de cobros vencidos
  sendAutomaticCobroVencidoNotifications(): Observable<EmailResponse[]> {
    return this.http.post<EmailResponse[]>(`${this.apiUrl}/automatic/cobros-vencidos`, {}, {
      headers: this.getHeaders()
    });
  }

  // Envío automático de notificaciones de pólizas por vencer
  sendAutomaticPolizasPorVencerNotifications(): Observable<EmailResponse[]> {
    return this.http.post<EmailResponse[]>(`${this.apiUrl}/automatic/polizas-por-vencer`, {}, {
      headers: this.getHeaders()
    });
  }
}