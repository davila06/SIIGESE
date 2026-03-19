import { Component, OnInit, ViewChild, ElementRef, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ApiService } from '../services/api.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { LoggingService } from '../services/logging.service';
import { UploadProgressDialogComponent } from './upload-progress-dialog.component';

@Component({
  selector: 'app-upload-polizas',
  templateUrl: './upload-polizas.component.html',
  styleUrls: ['./upload-polizas.component.scss'],
  standalone: false
})
export class UploadPolizasComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef;

  selectedFile: File | null = null;
  isDragOver = false;
  isDownloadingTemplate = false;

  private readonly logger = inject(LoggingService);

  constructor(
    private readonly apiService: ApiService,
    private readonly authService: AuthService,
    private readonly snackBar: MatSnackBar,
    private readonly router: Router,
    private readonly dialog: MatDialog
  ) {}

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.snackBar.open('Debes iniciar sesi�n para acceder a esta p�gina', 'Cerrar', {
        duration: 5000, panelClass: ['error-snackbar']
      });
      this.router.navigate(['/login']);
      return;
    }
    if (!this.canUploadExcel()) {
      this.snackBar.open(
        'Acceso denegado. Solo los administradores y cargadores de datos pueden subir archivos Excel.',
        'Cerrar',
        { duration: 5000, horizontalPosition: 'center', verticalPosition: 'top', panelClass: ['error-snackbar'] }
      );
      this.router.navigate(['/polizas']);
    }
  }

  canUploadExcel(): boolean {
    return this.authService.isAuthenticated() && this.authService.hasAnyRole(['Admin', 'DataLoader']);
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) this.validateAndSetFile(file);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
    const files = event.dataTransfer?.files;
    if (files?.length) this.validateAndSetFile(files[0]);
  }

  validateAndSetFile(file: File): void {
    const fileExtension = file.name.toLowerCase().split('.').pop();
    if (!['xlsx', 'xls'].includes(fileExtension ?? '')) {
      this.snackBar.open('Por favor selecciona un archivo Excel v�lido (.xlsx o .xls)', 'Cerrar', {
        duration: 5000, panelClass: ['error-snackbar']
      });
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      this.snackBar.open('El archivo es demasiado grande. M�ximo 10MB permitido.', 'Cerrar', {
        duration: 5000, panelClass: ['error-snackbar']
      });
      return;
    }
    this.selectedFile = file;
    this.snackBar.open(`Archivo "${file.name}" seleccionado correctamente`, 'Cerrar', {
      duration: 3000, panelClass: ['success-snackbar']
    });
  }

  uploadExcel(): void {
    if (!this.selectedFile) {
      this.snackBar.open('Por favor selecciona un archivo Excel', 'Cerrar', {
        duration: 3000, panelClass: ['error-snackbar']
      });
      return;
    }

    const user = this.authService.getCurrentUser();
    const perfilId = user?.id ?? 1;

    const dialogRef = this.dialog.open(UploadProgressDialogComponent, {
      width: '540px',
      maxWidth: '95vw',
      disableClose: true,
      panelClass: 'upload-progress-dialog-panel',
      data: { file: this.selectedFile, perfilId }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === 'continue') {
        this.clearFile();
      }
    });
  }

  clearFile(): void {
    this.selectedFile = null;
    if (this.fileInput) this.fileInput.nativeElement.value = '';
  }

  downloadDemo(): void {
    const link = document.createElement('a');
    link.href = '/assets/demo/DEMO_polizas.xlsx';
    link.download = 'DEMO_polizas.xlsx';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    this.snackBar.open('? Archivo DEMO descargado con 10 registros de ejemplo', 'Cerrar', {
      duration: 4000, panelClass: ['success-snackbar']
    });
  }

  downloadTemplate(): void {
    this.isDownloadingTemplate = true;
    this.apiService.downloadPolizasTemplate().subscribe({
      next: (blob: Blob) => {
        const url  = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href  = url;
        link.download = `template_polizas_${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        this.snackBar.open('Template descargado exitosamente', 'Cerrar', {
          duration: 3000, panelClass: ['success-snackbar']
        });
      },
      error: (error: any) => {
        this.logger.error('Error descargando template:', error);
        this.snackBar.open('Error descargando template. Intente nuevamente.', 'Cerrar', {
          duration: 3000, panelClass: ['error-snackbar']
        });
      },
      complete: () => { this.isDownloadingTemplate = false; }
    });
  }

  navigateToPolizas(): void {
    this.router.navigate(['/polizas']);
  }

  getFileIcon(): string {
    if (!this.selectedFile) return 'description';
    const ext = this.selectedFile.name.toLowerCase().split('.').pop();
    return ext === 'xlsx' || ext === 'xls' ? 'table_chart' : 'description';
  }

  getFileSizeFormatted(): string {
    if (!this.selectedFile) return '';
    const bytes = this.selectedFile.size;
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
