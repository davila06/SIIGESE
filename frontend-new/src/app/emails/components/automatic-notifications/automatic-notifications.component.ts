import { Component, OnInit, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { 
  NotificationService, 
  NotificationResult, 
  CobroVencido, 
  PolizaVencimiento, 
  NotificationStatistics 
} from '../../services/notification.service';
import { parseBackendDate } from '../../../shared/constants/currency.constants';
import { LoggingService } from '../../../services/logging.service';

@Component({
  selector: 'app-automatic-notifications',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatTabsModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule
  ],
  templateUrl: './automatic-notifications.component.html',
  styleUrls: ['./automatic-notifications.component.scss']
})
export class AutomaticNotificationsComponent implements OnInit {
  statistics: NotificationStatistics | null = null;
  overduePayments: CobroVencido[] = [];
  expiringPolicies: PolizaVencimiento[] = [];
  
  isLoading = false;
  daysBeforeExpiration = 30;
  
  displayedColumnsOverdue = ['numeroPoliza', 'clienteNombre', 'montoVencido', 'diasMora', 'actions'];
  displayedColumnsExpiring = ['numeroPoliza', 'clienteNombre', 'fechaVencimiento', 'diasHastaVencimiento', 'montoAsegurado'];

  private readonly logger = inject(LoggingService);

  constructor(
    private notificationService: NotificationService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    
    this.notificationService.getNotificationStatistics(this.daysBeforeExpiration).subscribe({
      next: (stats) => {
        this.statistics = stats;
      },
      error: (error) => {
        this.logger.error('Error cargando estadísticas:', error);
        this.showMessage('Error cargando estadísticas');
      }
    });

    this.notificationService.getOverduePayments().subscribe({
      next: (cobros) => {
        this.overduePayments = cobros;
      },
      error: (error) => {
        this.logger.error('Error cargando cobros vencidos:', error);
        this.showMessage('Error cargando cobros vencidos');
      }
    });

    this.notificationService.getExpiringPolicies(this.daysBeforeExpiration).subscribe({
      next: (polizas) => {
        this.expiringPolicies = polizas;
        this.isLoading = false;
      },
      error: (error) => {
        this.logger.error('Error cargando pólizas por vencer:', error);
        this.showMessage('Error cargando pólizas por vencer');
        this.isLoading = false;
      }
    });
  }

  processOverduePayments(): void {
    this.isLoading = true;
    this.notificationService.processOverduePayments().subscribe({
      next: (result: NotificationResult) => {
        this.handleNotificationResult(result, 'Cobros Vencidos');
        this.loadData();
      },
      error: (error: any) => {
        this.logger.error('Error procesando cobros vencidos:', error);
        this.showMessage('Error procesando cobros vencidos');
        this.isLoading = false;
      }
    });
  }

  processExpiringPolicies(): void {
    this.isLoading = true;
    this.notificationService.processExpiringPolicies(this.daysBeforeExpiration).subscribe({
      next: (result: NotificationResult) => {
        this.handleNotificationResult(result, 'Pólizas por Vencer');
        this.loadData();
      },
      error: (error: any) => {
        this.logger.error('Error procesando pólizas por vencer:', error);
        this.showMessage('Error procesando pólizas por vencer');
        this.isLoading = false;
      }
    });
  }

  processAllNotifications(): void {
    this.isLoading = true;
    this.notificationService.processAllNotifications(this.daysBeforeExpiration).subscribe({
      next: (result: NotificationResult) => {
        this.handleNotificationResult(result, 'Todas las Notificaciones');
        this.loadData();
      },
      error: (error: any) => {
        this.logger.error('Error procesando todas las notificaciones:', error);
        this.showMessage('Error procesando todas las notificaciones');
        this.isLoading = false;
      }
    });
  }

  onDaysChange(): void {
    this.loadData();
  }

  private handleNotificationResult(result: NotificationResult, type: string): void {
    this.isLoading = false;
    
    if (result.success) {
      let message = `${type} procesadas exitosamente.\n`;
      if (result.overduePaymentsSent > 0) {
        message += `Cobros vencidos: ${result.overduePaymentsSent} enviados`;
        if (result.overduePaymentsFailed > 0) {
          message += `, ${result.overduePaymentsFailed} fallidos`;
        }
        message += '\n';
      }
      if (result.expiringPoliciesSent > 0) {
        message += `Pólizas por vencer: ${result.expiringPoliciesSent} enviadas`;
        if (result.expiringPoliciesFailed > 0) {
          message += `, ${result.expiringPoliciesFailed} fallidas`;
        }
      }
      
      this.showMessage(message, 'success');
    } else {
      this.showMessage(`Error: ${result.message}`, 'error');
    }
  }

  private showMessage(message: string, type: 'success' | 'error' | 'info' = 'info'): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 5000,
      panelClass: type === 'success' ? 'success-snackbar' : type === 'error' ? 'error-snackbar' : ''
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS'
    }).format(amount);
  }

  formatDate(date: Date): string {
    const d = parseBackendDate(date as unknown);
    if (!d) return '-';
    return new Intl.DateTimeFormat('es-AR').format(d);
  }
}