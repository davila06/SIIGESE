import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { CommonModule, DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { EmailService, EmailHistoryResponse } from '../../services/email.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LoggingService } from '../../services/logging.service';
import { EmailPreviewDialogComponent } from '../../shared/components/email-preview-dialog/email-preview-dialog.component';

@Component({
  selector: 'app-email-history',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    DatePipe
  ],
  templateUrl: './email-history.component.html',
  styleUrls: ['./email-history.component.scss']
})
export class EmailHistoryComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = ['sentAt', 'toEmail', 'toName', 'subject', 'emailType', 'isSuccess', 'senderName', 'actions'];
  dataSource = new MatTableDataSource<EmailHistoryResponse>();
  loading = false;
  totalEmails = 0;
  pageSize = 10;
  currentPage = 1;

  emailTypeColors: { [key: string]: string } = {
    'Manual': 'primary',
    'CobroVencido': 'warn',
    'ReclamoRecibido': 'accent',
    'Bienvenida': 'primary',
    'PolizaPorVencer': 'warn'
  };

  private readonly logger = inject(LoggingService);

  constructor(
    private emailService: EmailService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
  ) { }

  ngOnInit(): void {
    this.loadEmailHistory();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadEmailHistory(): void {
    this.loading = true;
    this.emailService.getEmailHistory(this.currentPage, this.pageSize).subscribe({
      next: (emails) => {
        this.dataSource.data = emails;
        this.totalEmails = emails.length;
        this.loading = false;
      },
      error: (error) => {
        this.snackBar.open('Error cargando historial de emails', 'Cerrar', { duration: 3000 });
        this.logger.error('Error:', error);
        this.loading = false;
      }
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  getStatusIcon(isSuccess: boolean): string {
    return isSuccess ? 'check_circle' : 'error';
  }

  getStatusColor(isSuccess: boolean): string {
    return isSuccess ? 'primary' : 'warn';
  }

  getEmailTypeLabel(emailType: string): string {
    const labels: { [key: string]: string } = {
      'Manual': 'Manual',
      'CobroVencido': 'Cobro Vencido',
      'ReclamoRecibido': 'Reclamo Recibido',
      'Bienvenida': 'Bienvenida',
      'PolizaPorVencer': 'Póliza por Vencer'
    };
    return labels[emailType] || emailType;
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadEmailHistory();
  }

  refreshHistory(): void {
    this.loadEmailHistory();
  }

  viewEmailDetails(email: EmailHistoryResponse): void {
    this.dialog.open(EmailPreviewDialogComponent, {
      data: {
        mode: 'details',
        subject: email.subject,
        toEmail: email.toEmail,
        toName: email.toName,
        sentAt: email.sentAt,
        emailType: email.emailType,
        isSuccess: email.isSuccess,
        errorMessage: email.errorMessage,
      },
      width: '560px',
      maxWidth: '95vw',
      panelClass: 'epd-dialog-panel',
      autoFocus: false,
    });
  }

  resendEmail(email: EmailHistoryResponse): void {
    this.emailService.resendEmail(email.id).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.snackBar.open(`Email reenviado a ${email.toEmail}`, 'Cerrar', { duration: 3000 });
        } else {
          this.snackBar.open(`Error: ${response.message}`, 'Cerrar', { duration: 5000 });
        }
      },
      error: () => {
        this.snackBar.open('Error al reenviar el email. Intente nuevamente.', 'Cerrar', { duration: 4000 });
      },
    });
  }
}
