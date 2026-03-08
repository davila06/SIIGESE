import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { EmailConfigService } from '../../services/email-config.service';
import { EmailConfig, ApiResponse } from '../../models/email-config.model';

@Component({
  selector: 'app-email-config-list',
  standalone: false,
  templateUrl: './email-config-list.html',
  styleUrl: './email-config-list.scss'
})
export class EmailConfigList implements OnInit {
  emailConfigs: EmailConfig[] = [];
  displayedColumns: string[] = ['configName', 'smtpServer', 'fromEmail', 'isDefault', 'isActive', 'actions'];
  loading = false;

  constructor(
    private emailConfigService: EmailConfigService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadEmailConfigs();
  }

  loadEmailConfigs(): void {
    this.loading = true;
    this.emailConfigService.getAll().subscribe({
      next: (response: ApiResponse<EmailConfig[]>) => {
        const data = response.data || [];
        this.emailConfigs = Array.isArray(data) ? data : [];
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading email configs:', error);
        this.showMessage('Error al cargar las configuraciones', 'error');
        this.loading = false;
      }
    });
  }

  onNew(): void {
    this.router.navigate(['/configuracion/email/new']);
  }

  onEdit(config: EmailConfig): void {
    this.router.navigate(['/configuracion/email/edit', config.id]);
  }

  onDelete(config: EmailConfig): void {
    if (confirm(`¿Está seguro de eliminar la configuración "${config.configName}"?`)) {
      this.emailConfigService.delete(config.id).subscribe({
        next: () => {
          this.showMessage('Configuración eliminada exitosamente', 'success');
          this.loadEmailConfigs();
        },
        error: (error) => {
          console.error('Error deleting config:', error);
          this.showMessage('Error al eliminar la configuración', 'error');
        }
      });
    }
  }

  onSetDefault(config: EmailConfig): void {
    this.emailConfigService.setAsDefault(config.id).subscribe({
      next: () => {
        this.showMessage('Configuración establecida como predeterminada', 'success');
        this.loadEmailConfigs();
      },
      error: (error) => {
        console.error('Error setting default:', error);
        this.showMessage('Error al establecer como predeterminada', 'error');
      }
    });
  }

  onToggleActive(config: EmailConfig): void {
    this.emailConfigService.toggleActiveStatus(config.id).subscribe({
      next: () => {
        const status = config.isActive ? 'desactivada' : 'activada';
        this.showMessage(`Configuración ${status} exitosamente`, 'success');
        this.loadEmailConfigs();
      },
      error: (error) => {
        console.error('Error toggling status:', error);
        this.showMessage('Error al cambiar el estado', 'error');
      }
    });
  }

  private showMessage(message: string, type: 'success' | 'error' | 'info' = 'info'): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: type === 'error' ? ['error-snackbar'] : 
                  type === 'success' ? ['success-snackbar'] : ['info-snackbar']
    });
  }
}
