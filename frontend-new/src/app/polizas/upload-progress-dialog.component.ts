import { Component, OnInit, OnDestroy, Inject, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ApiService } from '../services/api.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DataUploadResult, FailedRecord } from '../interfaces/user.interface';
import { LoggingService } from '../services/logging.service';

export interface UploadDialogData {
  file: File;
  perfilId: number;
}

type DialogState = 'loading' | 'success' | 'error';

@Component({
  selector: 'app-upload-progress-dialog',
  templateUrl: './upload-progress-dialog.component.html',
  styleUrls: ['./upload-progress-dialog.component.scss'],
  standalone: false
})
export class UploadProgressDialogComponent implements OnInit, OnDestroy {

  readonly loadingSteps = [
    { icon: 'folder_open',            label: 'Leyendo archivo Excel...' },
    { icon: 'fact_check',             label: 'Validando registros...' },
    { icon: 'published_with_changes', label: 'Procesando pólizas...' },
    { icon: 'cloud_upload',           label: 'Guardando en base de datos...' },
    { icon: 'verified',               label: 'Finalizando...' },
  ];

  state: DialogState = 'loading';
  activeStep = 0;
  private stepInterval: any;

  result: DataUploadResult | null = null;
  animatedProcessed = 0;
  animatedErrors = 0;
  countdownSeconds = 5;
  private countdownTimer: any;
  private countUpTimer: any;

  uploadStats = {
    totalRecords: 0,
    processedRecords: 0,
    errorRecords: 0,
    errors: [] as string[],
    failedRecords: [] as FailedRecord[]
  };

  errorMessage = '';
  isDownloading = false;

  private readonly logger = inject(LoggingService);

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: UploadDialogData,
    private readonly dialogRef: MatDialogRef<UploadProgressDialogComponent>,
    private readonly apiService: ApiService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.startLoadingSteps();
    this.runUpload();
  }

  ngOnDestroy(): void {
    clearInterval(this.stepInterval);
    clearInterval(this.countdownTimer);
    clearInterval(this.countUpTimer);
  }

  private startLoadingSteps(): void {
    this.activeStep = 0;
    clearInterval(this.stepInterval);
    this.stepInterval = setInterval(() => {
      if (this.activeStep < this.loadingSteps.length - 1) {
        this.activeStep++;
      }
    }, 1400);
  }

  private runUpload(): void {
    this.apiService.uploadExcelPolizas(this.data.perfilId, this.data.file).subscribe({
      next: (result: DataUploadResult) => {
        clearInterval(this.stepInterval);
        this.activeStep = this.loadingSteps.length - 1;
        this.result = result;
        this.uploadStats = {
          totalRecords:      result.totalRecords,
          processedRecords:  result.processedRecords,
          errorRecords:      result.errorRecords,
          errors:            result.errors ?? [],
          failedRecords:     result.failedRecords ?? []
        };

        if (result.success) {
          this.showSuccess();
        } else {
          this.state = 'error';
          // Auto-download errors file
          if (result.failedRecords?.length) {
            setTimeout(() => this.downloadErrorsFile(), 800);
          }
        }
      },
      error: (err: any) => {
        clearInterval(this.stepInterval);
        this.state = 'error';
        this.logger.error('Upload error:', err);
        if (err.status === 400)      this.errorMessage = err.error?.message || 'Formato de archivo inválido';
        else if (err.status === 413) this.errorMessage = 'El archivo es demasiado grande (máximo 10 MB)';
        else                         this.errorMessage = 'Error interno del servidor. Intente nuevamente.';
      }
    });
  }

  private showSuccess(): void {
    this.state = 'success';
    this.animatedProcessed = 0;
    this.animatedErrors = 0;
    this.countdownSeconds = 5;

    const target    = this.uploadStats.processedRecords;
    const errTarget = this.uploadStats.errorRecords;
    const steps = 40;
    let tick = 0;
    clearInterval(this.countUpTimer);
    this.countUpTimer = setInterval(() => {
      tick++;
      this.animatedProcessed = Math.round(target    * Math.min(tick / steps, 1));
      this.animatedErrors    = Math.round(errTarget * Math.min(tick / steps, 1));
      if (tick >= steps) clearInterval(this.countUpTimer);
    }, 30);

    clearInterval(this.countdownTimer);
    this.countdownTimer = setInterval(() => {
      this.countdownSeconds--;
      if (this.countdownSeconds <= 0) {
        clearInterval(this.countdownTimer);
        this.goToPolizas();
      }
    }, 1000);
  }

  get countdownOffset(): number {
    const elapsed = 5 - this.countdownSeconds;
    return Math.round(113 - (113 * elapsed / 5));
  }

  goToPolizas(): void {
    clearInterval(this.countdownTimer);
    this.dialogRef.close('navigate');
    this.router.navigate(['/polizas']);
  }

  closeAndContinue(): void {
    clearInterval(this.countdownTimer);
    this.dialogRef.close('continue');
  }

  downloadErrorsFile(): void {
    if (!this.result?.failedRecords?.length) return;

    const payload = {
      fileHeaders:      this.result.fileHeaders ?? [],
      failedRecords:    this.result.failedRecords,
      originalFileName: this.data.file.name,
    };

    this.isDownloading = true;
    this.apiService.downloadPolizasErrorsExcel(payload).subscribe({
      next: (blob: Blob) => {
        this.isDownloading = false;
        const errorCount = this.result!.failedRecords.length;
        const timestamp  = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
        const baseName   = this.data.file.name.replace(/\.[^.]+$/, '');
        const fileName   = `ERRORES_${baseName}_${timestamp}_${errorCount}reg.xlsx`;

        const url  = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href  = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);

        this.snackBar.open(
          `📥 ${errorCount} registro(s) con errores descargados para corregir`,
          'Cerrar',
          { duration: 5000, panelClass: ['error-snackbar'] }
        );
      },
      error: (err: any) => {
        this.isDownloading = false;
        this.logger.error('Error descargando errores:', err);
      }
    });
  }
}
