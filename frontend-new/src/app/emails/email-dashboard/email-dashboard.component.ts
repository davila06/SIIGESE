import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { EmailService, EmailResponse, EmailStats } from '../../services/email.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-email-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './email-dashboard.component.html',
  styleUrls: ['./email-dashboard.component.scss']
})
export class EmailDashboardComponent implements OnInit {
  stats: EmailStats = { totalSent: 0, totalFailed: 0, pendingCobros: 0, polizasPorVencer: 0 };
  loading = false;

  constructor(
    private emailService: EmailService,
    private router: Router,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.loading = true;
    this.emailService.getEmailStats().subscribe({
      next: (stats) => {
        this.stats = stats;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading email stats:', error);
        this.loading = false;
      }
    });
  }

  sendAutomaticCobroVencidoNotifications(): void {
    this.loading = true;
    this.emailService.sendAutomaticCobroVencidoNotifications().subscribe({
      next: (responses: EmailResponse[]) => {
        const successCount = responses.filter(r => r.isSuccess).length;
        const totalCount = responses.length;
        this.snackBar.open(
          `Notificaciones enviadas: ${successCount}/${totalCount}`, 
          'Cerrar', 
          { duration: 5000 }
        );
        this.loading = false;
        this.loadStats();
      },
      error: (error) => {
        this.snackBar.open('Error enviando notificaciones', 'Cerrar', { duration: 3000 });
        console.error('Error:', error);
        this.loading = false;
      }
    });
  }

  sendAutomaticPolizasPorVencerNotifications(): void {
    this.loading = true;
    this.emailService.sendAutomaticPolizasPorVencerNotifications().subscribe({
      next: (responses: EmailResponse[]) => {
        const successCount = responses.filter(r => r.isSuccess).length;
        const totalCount = responses.length;
        this.snackBar.open(
          `Notificaciones enviadas: ${successCount}/${totalCount}`, 
          'Cerrar', 
          { duration: 5000 }
        );
        this.loading = false;
        this.loadStats();
      },
      error: (error) => {
        this.snackBar.open('Error enviando notificaciones', 'Cerrar', { duration: 3000 });
        console.error('Error:', error);
        this.loading = false;
      }
    });
  }

  navigateToSendEmail(): void {
    this.router.navigate(['/emails/send']);
  }

  navigateToEmailHistory() {
    this.router.navigate(['/emails/history']);
  }

  navigateToTemplates() {
    this.router.navigate(['/emails/templates']);
  }

  navigateToNotifications(): void {
    this.router.navigate(['/emails/notifications']);
  }
}
