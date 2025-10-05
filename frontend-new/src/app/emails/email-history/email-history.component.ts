import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { EmailService, EmailHistoryResponse } from '../../services/email.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-email-history',
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

  constructor(
    private emailService: EmailService,
    private snackBar: MatSnackBar
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
        console.error('Error:', error);
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
    // Implementar modal para ver detalles del email
    console.log('Ver detalles del email:', email);
  }

  resendEmail(email: EmailHistoryResponse): void {
    // Implementar reenvío de email
    console.log('Reenviar email:', email);
    this.snackBar.open('Funcionalidad de reenvío en desarrollo', 'Cerrar', { duration: 3000 });
  }
}
