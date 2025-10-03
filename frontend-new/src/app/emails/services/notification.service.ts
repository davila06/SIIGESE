import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface NotificationResult {
  success: boolean;
  message: string;
  overduePaymentsSent: number;
  overduePaymentsFailed: number;
  expiringPoliciesSent: number;
  expiringPoliciesFailed: number;
}

export interface CobroVencido {
  cobroId: number;
  numeroPoliza: string;
  clienteEmail: string;
  clienteNombre: string;
  montoVencido: number;
  fechaVencimiento: Date;
  diasMora: number;
  concepto: string;
}

export interface PolizaVencimiento {
  polizaId: number;
  numeroPoliza: string;
  clienteEmail: string;
  clienteNombre: string;
  fechaVencimiento: Date;
  diasHastaVencimiento: number;
  tipoPoliza: string;
  montoAsegurado: number;
  prima: number;
}

export interface NotificationStatistics {
  overduePaymentsCount: number;
  expiringPoliciesCount: number;
  totalOverdueAmount: number;
  totalInsuredAmount: number;
  daysBeforeExpiration: number;
  lastUpdated: Date;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/notifications`;

  constructor(private http: HttpClient) { }

  /**
   * Procesa y envía notificaciones de cobros vencidos
   */
  processOverduePayments(): Observable<NotificationResult> {
    return this.http.post<NotificationResult>(`${this.apiUrl}/process-overdue-payments`, {});
  }

  /**
   * Procesa y envía notificaciones de pólizas por vencer
   */
  processExpiringPolicies(daysBeforeExpiration: number = 30): Observable<NotificationResult> {
    const params = new HttpParams().set('daysBeforeExpiration', daysBeforeExpiration.toString());
    return this.http.post<NotificationResult>(`${this.apiUrl}/process-expiring-policies`, {}, { params });
  }

  /**
   * Procesa todas las notificaciones automáticas
   */
  processAllNotifications(daysBeforeExpiration: number = 30): Observable<NotificationResult> {
    const params = new HttpParams().set('daysBeforeExpiration', daysBeforeExpiration.toString());
    return this.http.post<NotificationResult>(`${this.apiUrl}/process-all`, {}, { params });
  }

  /**
   * Obtiene la lista de cobros vencidos
   */
  getOverduePayments(): Observable<CobroVencido[]> {
    return this.http.get<CobroVencido[]>(`${this.apiUrl}/overdue-payments`);
  }

  /**
   * Obtiene la lista de pólizas por vencer
   */
  getExpiringPolicies(daysBeforeExpiration: number = 30): Observable<PolizaVencimiento[]> {
    const params = new HttpParams().set('daysBeforeExpiration', daysBeforeExpiration.toString());
    return this.http.get<PolizaVencimiento[]>(`${this.apiUrl}/expiring-policies`, { params });
  }

  /**
   * Obtiene estadísticas de notificaciones pendientes
   */
  getNotificationStatistics(daysBeforeExpiration: number = 30): Observable<NotificationStatistics> {
    const params = new HttpParams().set('daysBeforeExpiration', daysBeforeExpiration.toString());
    return this.http.get<NotificationStatistics>(`${this.apiUrl}/statistics`, { params });
  }
}