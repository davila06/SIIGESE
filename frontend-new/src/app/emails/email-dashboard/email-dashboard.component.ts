import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { EmailService, EmailResponse } from '../../services/email.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
    selector: 'app-email-dashboard',
    templateUrl: './email-dashboard.component.html',
    styleUrls: ['./email-dashboard.component.scss'],
    standalone: false
})
export class EmailDashboardComponent implements OnInit {
  stats: any = {};
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
