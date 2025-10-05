import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../services/api.service';
import { AuthService } from '../services/auth.service';
import { DataUploadResult } from '../interfaces/user.interface';
import { Router } from '@angular/router';
import { CURRENCY_CONSTANTS } from '../shared/constants/currency.constants';

@Component({
  selector: 'app-upload-polizas',
  templateUrl: './upload-polizas.component.html',
  styleUrls: ['./upload-polizas.component.scss'],
  standalone: false
})
export class UploadPolizasComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef;
  
  selectedFile: File | null = null;
  isLoading = false;
  isDragOver = false;
  uploadResult: DataUploadResult | null = null;
  
  // Estadísticas del último upload
  uploadStats = {
    totalRecords: 0,
    processedRecords: 0,
    errorRecords: 0,
    errors: [] as string[]
  };

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Verificar permisos de upload con doble validación
    if (!this.authService.isAuthenticated()) {
      this.snackBar.open('Debes iniciar sesión para acceder a esta página', 'Cerrar', {
        duration: 5000,
        panelClass: ['error-snackbar']
      });
      this.router.navigate(['/login']);
      return;
    }

    if (!this.canUploadExcel()) {
      this.snackBar.open(
        'Acceso denegado. Solo los administradores y cargadores de datos pueden subir archivos Excel.', 
        'Cerrar', 
        {
          duration: 5000,
          horizontalPosition: 'center',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        }
      );
      this.router.navigate(['/polizas']);
      return;
    }
  }

  canUploadExcel(): boolean {
    return this.authService.isAuthenticated() && this.authService.hasAnyRole(['Admin', 'DataLoader']);
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.validateAndSetFile(file);
    }
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
    if (files && files.length > 0) {
      this.validateAndSetFile(files[0]);
    }
  }

  validateAndSetFile(file: File): void {
    // Validar tipo de archivo
    const allowedTypes = [
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'application/vnd.ms-excel',
      '.xlsx',
      '.xls'
    ];
    
    const fileExtension = file.name.toLowerCase().split('.').pop();
    
    if (!allowedTypes.includes(file.type) && !['xlsx', 'xls'].includes(fileExtension || '')) {
      this.snackBar.open('Por favor selecciona un archivo Excel válido (.xlsx o .xls)', 'Cerrar', {
        duration: 5000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    // Validar tamaño del archivo (máximo 10MB)
    const maxSize = 10 * 1024 * 1024; // 10MB
    if (file.size > maxSize) {
      this.snackBar.open('El archivo es demasiado grande. Máximo 10MB permitido.', 'Cerrar', {
        duration: 5000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    this.selectedFile = file;
    this.uploadResult = null; // Limpiar resultado anterior
    
    this.snackBar.open(`Archivo "${file.name}" seleccionado correctamente`, 'Cerrar', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  uploadExcel(): void {
    if (!this.selectedFile) {
      this.snackBar.open('Por favor selecciona un archivo Excel', 'Cerrar', {
        duration: 3000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    this.isLoading = true;
    const user = this.authService.getCurrentUser();
    const perfilId = user?.id || 1;

    this.apiService.uploadExcelPolizas(perfilId, this.selectedFile).subscribe({
      next: (result: DataUploadResult) => {
        this.isLoading = false;
        this.uploadResult = result;
        this.updateUploadStats(result);
        
        if (result.success) {
          this.snackBar.open(
            `¡Éxito! ${result.processedRecords} pólizas procesadas de ${result.totalRecords} registros`,
            'Cerrar',
            {
              duration: 5000,
              panelClass: ['success-snackbar']
            }
          );
          
          // Limpiar archivo después del éxito
          this.clearFile();
        } else {
          this.snackBar.open(
            `Error en el procesamiento. ${result.errorRecords} errores encontrados`,
            'Ver Detalles',
            {
              duration: 8000,
              panelClass: ['error-snackbar']
            }
          );
        }
      },
      error: (error) => {
        this.isLoading = false;
        console.error('Error uploading file:', error);
        
        let errorMessage = 'Error al subir el archivo';
        if (error.status === 400) {
          errorMessage = error.error?.message || 'Formato de archivo inválido';
        } else if (error.status === 413) {
          errorMessage = 'El archivo es demasiado grande';
        } else if (error.status === 500) {
          errorMessage = 'Error interno del servidor';
        }
        
        this.snackBar.open(errorMessage, 'Cerrar', {
          duration: 8000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  updateUploadStats(result: DataUploadResult): void {
    this.uploadStats = {
      totalRecords: result.totalRecords,
      processedRecords: result.processedRecords,
      errorRecords: result.errorRecords,
      errors: result.errors || []
    };
  }

  clearFile(): void {
    this.selectedFile = null;
    this.uploadResult = null;
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }

  downloadTemplate(): void {
    // Crear un template Excel de ejemplo
    const templateData = [
      {
        'Número Póliza': 'POL-2024-001',
        'Nombre Asegurado': 'Juan Pérez García',
        'Prima': '150000',
        'Aseguradora': 'Seguros ABC',
        'Fecha Vigencia': '2024-12-31',
        'Marca': 'Toyota',
        'Modelo': 'Corolla',
        'Placa': 'ABC123',
        'Modalidad': 'Anual',
        'Frecuencia': 'Mensual',
        'Moneda': CURRENCY_CONSTANTS.DEFAULT_CURRENCY
      }
    ];

    // Aquí podrías implementar la descarga del template
    // Por ahora solo mostramos un mensaje
    this.snackBar.open('Función de descarga de template próximamente', 'Cerrar', {
      duration: 3000
    });
  }

  navigateToPolizas(): void {
    this.router.navigate(['/polizas']);
  }

  getFileIcon(): string {
    if (!this.selectedFile) return 'description';
    
    const extension = this.selectedFile.name.toLowerCase().split('.').pop();
    return extension === 'xlsx' || extension === 'xls' ? 'table_chart' : 'description';
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