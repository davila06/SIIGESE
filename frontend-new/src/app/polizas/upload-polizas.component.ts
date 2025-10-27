import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../services/api.service';
import { AuthService } from '../services/auth.service';
import { DataUploadResult, FailedRecord } from '../interfaces/user.interface';
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
    errors: [] as string[],
    failedRecords: [] as FailedRecord[]
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
          // Descargar automáticamente archivo de errores si hay registros fallidos
          if (result.failedRecords && result.failedRecords.length > 0) {
            setTimeout(() => {
              this.downloadErrorsFile();
            }, 1000); // Pequeña pausa para que el usuario vea el mensaje primero
          }

          this.snackBar.open(
            `Error en el procesamiento. ${result.errorRecords} errores encontrados. Descargando archivo de errores...`,
            'Ver Detalles',
            {
              duration: 8000,
              panelClass: ['error-snackbar']
            }
          );
        }
      },
      error: (error: any) => {
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
      errors: result.errors || [],
      failedRecords: result.failedRecords || []
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
    this.isLoading = true;

    this.apiService.downloadPolizasTemplate().subscribe({
      next: (blob: Blob) => {
        // Crear un link para descargar el archivo
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `template_polizas_${new Date().toISOString().split('T')[0]}.xlsx`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        this.snackBar.open('Template descargado exitosamente', 'Cerrar', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
      },
      error: (error: any) => {
        console.error('Error descargando template:', error);
        this.snackBar.open('Error descargando template. Intente nuevamente.', 'Cerrar', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  downloadErrorsFile(): void {
    if (this.uploadStats.failedRecords.length === 0) {
      this.snackBar.open('No hay errores para descargar', 'Cerrar', {
        duration: 3000
      });
      return;
    }

    // Crear datos para el Excel de errores mejorado
    const headers = [
      'Fila Original',
      'Error Detectado',
      'POLIZA',
      'NOMBRE',
      'NUMEROCEDULA',
      'PRIMA',
      'MONEDA',
      'FECHA',
      'FRECUENCIA',
      'ASEGURADORA',
      'MARCA',
      'MODELO',
      'PLACA',
      'AÑO',
      'CORREO',
      'NUMEROTELEFONO',
      'Instrucciones de Corrección'
    ];

    const errorData = this.uploadStats.failedRecords.map(record => [
      record.rowNumber.toString(),
      record.error,
      record.originalData['POLIZA'] || record.originalData['Número Póliza'] || '',
      record.originalData['NOMBRE'] || record.originalData['Nombre Asegurado'] || '',
      record.originalData['NUMEROCEDULA'] || record.originalData['Número Cédula'] || '',
      record.originalData['PRIMA'] || record.originalData['Prima'] || '',
      record.originalData['MONEDA'] || record.originalData['Moneda'] || '',
      record.originalData['FECHA'] || record.originalData['Fecha Vigencia'] || '',
      record.originalData['FRECUENCIA'] || record.originalData['Frecuencia'] || '',
      record.originalData['ASEGURADORA'] || record.originalData['Aseguradora'] || '',
      record.originalData['MARCA'] || record.originalData['Marca'] || '',
      record.originalData['MODELO'] || record.originalData['Modelo'] || '',
      record.originalData['PLACA'] || record.originalData['Placa'] || '',
      record.originalData['AÑO'] || record.originalData['Año'] || '',
      record.originalData['CORREO'] || record.originalData['Correo'] || '',
      record.originalData['NUMEROTELEFONO'] || record.originalData['Número Teléfono'] || '',
      'Corrige el error indicado y vuelve a subir el archivo'
    ]);

    // Crear contenido CSV mejorado con BOM para caracteres especiales
    let csvContent = '\ufeff'; // BOM para UTF-8
    csvContent += headers.map(header => `"${header}"`).join(',') + '\n';
    
    errorData.forEach(row => {
      csvContent += row.map(value => `"${value.toString().replace(/"/g, '""')}"`).join(',') + '\n';
    });

    // Crear y descargar archivo con nombre más descriptivo
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);

    const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
    const fileName = `ERRORES_Polizas_${timestamp}_${this.uploadStats.failedRecords.length}_registros.csv`;
    link.setAttribute('download', fileName);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);

    this.snackBar.open(
      `📄 Archivo de errores descargado: ${this.uploadStats.failedRecords.length} registros para revisar`,
      'Cerrar',
      {
        duration: 5000,
        panelClass: ['success-snackbar']
      }
    );
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
