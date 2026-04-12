import { Component, OnInit, ElementRef, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { ReclamosService } from '../../services/reclamos.service';
import { 
  Reclamo, 
  TipoReclamo, 
  EstadoReclamo, 
  PrioridadReclamo,
  getTipoReclamoLabel,
  getEstadoReclamoLabel,
  getPrioridadReclamoLabel,
  ReclamoHistorialEntry,
  ReclamoDocumento
} from '../../interfaces/reclamo.interface';
import { LoggingService } from '../../../services/logging.service';

@Component({
  selector: 'app-reclamo-detalle',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTabsModule,
    MatListModule,
    MatTooltipModule,
    MatProgressBarModule
  ],
  templateUrl: './reclamo-detalle.component.html',
  styleUrls: ['./reclamo-detalle.component.scss']
})
export class ReclamoDetalleComponent implements OnInit {
  reclamo: Reclamo | null = null;
  loading = true;
  reclamoId: number = 0;

  // ── Historial ───────────────────────────────────────────────────────────
  historialEntries: ReclamoHistorialEntry[] = [];
  loadingHistorial = false;
  historialLoaded  = false;

  // ── Documentos ──────────────────────────────────────────────────────────
  documentos: ReclamoDocumento[] = [];
  loadingDocumentos    = false;
  documentosLoaded     = false;
  uploadingDocumento   = false;

  @ViewChild('fileInputRef') fileInputRef!: ElementRef<HTMLInputElement>;

  // Enums para el template
  TipoReclamo = TipoReclamo;
  EstadoReclamo = EstadoReclamo;
  PrioridadReclamo = PrioridadReclamo;

  private readonly logger = inject(LoggingService);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly reclamosService: ReclamosService,
    private readonly snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.reclamoId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.reclamoId) {
      this.cargarReclamo();
    } else {
      this.showMessage('ID de reclamo inválido', 'error');
      this.router.navigate(['/reclamos']);
    }
  }

  cargarReclamo(): void {
    this.loading = true;
    this.reclamosService.getReclamoById(this.reclamoId).subscribe({
      next: (reclamo: Reclamo) => {
        this.reclamo = reclamo;
        this.loading = false;
      },
      error: (error) => {
        this.logger.error('Error cargando reclamo:', error);
        this.showMessage('Error al cargar el reclamo', 'error');
        this.loading = false;
        this.router.navigate(['/reclamos']);
      }
    });
  }

  onEdit(): void {
    if (this.reclamo) {
      this.router.navigate(['/reclamos/editar', this.reclamo.id]);
    }
  }

  onBack(): void {
    this.router.navigate(['/reclamos']);
  }

  // ── Tab change handler ───────────────────────────────────────────────────

  onTabChange(index: number): void {
    if (index === 1 && !this.historialLoaded)  this.cargarHistorial();
    if (index === 2 && !this.documentosLoaded) this.cargarDocumentos();
  }

  // ── Historial ─────────────────────────────────────────────────────────────

  cargarHistorial(): void {
    this.loadingHistorial = true;
    this.reclamosService.getHistorial(this.reclamoId).subscribe({
      next: (entries) => {
        this.historialEntries = entries;
        this.historialLoaded  = true;
        this.loadingHistorial = false;
      },
      error: (err) => {
        this.logger.error('Error cargando historial:', err);
        this.loadingHistorial = false;
        this.showMessage('Error al cargar el historial', 'error');
      }
    });
  }

  getHistorialIcon(tipoEvento: string): string {
    switch (tipoEvento) {
      case 'Creacion':          return 'add_circle';
      case 'CambioEstado':      return 'swap_horiz';
      case 'Asignacion':        return 'person';
      case 'Resolucion':        return 'check_circle';
      case 'Actualizacion':     return 'edit';
      case 'DocumentoAgregado': return 'upload_file';
      case 'DocumentoEliminado': return 'delete_outline';
      default:                  return 'info';
    }
  }

  getHistorialColor(tipoEvento: string): string {
    switch (tipoEvento) {
      case 'Creacion':           return '#4caf50';
      case 'CambioEstado':       return '#2196f3';
      case 'Asignacion':         return '#9c27b0';
      case 'Resolucion':         return '#4caf50';
      case 'Actualizacion':      return '#ff9800';
      case 'DocumentoAgregado':  return '#00bcd4';
      case 'DocumentoEliminado': return '#f44336';
      default:                   return '#9e9e9e';
    }
  }

  // ── Documentos ────────────────────────────────────────────────────────────

  cargarDocumentos(): void {
    this.loadingDocumentos = true;
    this.reclamosService.getDocumentos(this.reclamoId).subscribe({
      next: (docs) => {
        this.documentos        = docs;
        this.documentosLoaded  = true;
        this.loadingDocumentos = false;
      },
      error: (err) => {
        this.logger.error('Error cargando documentos:', err);
        this.loadingDocumentos = false;
        this.showMessage('Error al cargar los documentos', 'error');
      }
    });
  }

  triggerFileUpload(): void {
    this.fileInputRef?.nativeElement.click();
  }

  onFileSelected(event: Event): void {
    const input   = event.target as HTMLInputElement;
    const archivo = input?.files?.[0];
    if (!archivo) return;
    input.value = ''; // reset so the same file can be re-selected
    this.subirDocumento(archivo);
  }

  private subirDocumento(archivo: File): void {
    const MAX_MB = 20;
    if (archivo.size > MAX_MB * 1024 * 1024) {
      this.showMessage(`El archivo supera el límite de ${MAX_MB} MB.`, 'error');
      return;
    }
    this.uploadingDocumento = true;
    this.reclamosService.uploadDocumento(this.reclamoId, archivo).subscribe({
      next: (doc) => {
        this.documentos = [doc, ...this.documentos];
        this.uploadingDocumento = false;
        this.showMessage(`"${doc.nombre}" adjuntado correctamente.`, 'success');
      },
      error: (err) => {
        this.logger.error('Error subiendo documento:', err);
        this.uploadingDocumento = false;
        this.showMessage('Error al subir el archivo. Verifica el tamaño y formato.', 'error');
      }
    });
  }

  descargarDocumento(doc: ReclamoDocumento): void {
    const url = this.reclamosService.downloadDocumentoUrl(this.reclamoId, doc.id);
    const a   = document.createElement('a');
    a.href    = url;
    a.download = doc.nombre;
    a.target  = '_blank';
    a.rel     = 'noopener noreferrer';
    a.click();
  }

  eliminarDocumento(doc: ReclamoDocumento): void {
    if (!confirm(`¿Eliminar el archivo "${doc.nombre}"?`)) return;
    this.reclamosService.deleteDocumento(this.reclamoId, doc.id).subscribe({
      next: () => {
        this.documentos = this.documentos.filter(d => d.id !== doc.id);
        this.showMessage(`"${doc.nombre}" eliminado.`, 'info');
      },
      error: (err) => {
        this.logger.error('Error eliminando documento:', err);
        this.showMessage('Error al eliminar el documento.', 'error');
      }
    });
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024)       return `${bytes} B`;
    if (bytes < 1048576)    return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1048576).toFixed(1)} MB`;
  }

  getDocumentoIcon(tipoContenido: string): string {
    if (tipoContenido.startsWith('image/'))           return 'image';
    if (tipoContenido === 'application/pdf')           return 'picture_as_pdf';
    if (tipoContenido.includes('word'))                return 'description';
    if (tipoContenido.includes('excel') || tipoContenido.includes('spreadsheet'))
                                                       return 'table_chart';
    if (tipoContenido.startsWith('video/'))            return 'video_file';
    return 'insert_drive_file';
  }

  getEstadoColor(estado: EstadoReclamo): string {
    switch (estado) {
      case EstadoReclamo.Abierto:
        return 'primary';
      case EstadoReclamo.EnProceso:
        return 'accent';
      case EstadoReclamo.Resuelto:
        return 'primary';
      case EstadoReclamo.Cerrado:
        return '';
      case EstadoReclamo.Rechazado:
        return 'warn';
      case EstadoReclamo.Escalado:
        return 'warn';
      default:
        return 'primary';
    }
  }

  getPrioridadColor(prioridad: PrioridadReclamo): string {
    switch (prioridad) {
      case PrioridadReclamo.Baja:
        return 'primary';
      case PrioridadReclamo.Media:
        return 'accent';
      case PrioridadReclamo.Alta:
        return 'warn';
      case PrioridadReclamo.Critica:
        return 'warn';
      default:
        return 'primary';
    }
  }

  getTipoIcon(tipo: TipoReclamo): string {
    switch (tipo) {
      case TipoReclamo.Siniestro:
        return 'car_crash';
      case TipoReclamo.Servicio:
        return 'feedback';
      case TipoReclamo.Facturacion:
        return 'money';
      case TipoReclamo.Cobertura:
        return 'security';
      case TipoReclamo.Proceso:
        return 'edit';
      case TipoReclamo.Otro:
        return 'help_outline';
      default:
        return 'assignment';
    }
  }

  getEstadoIcon(estado: EstadoReclamo): string {
    switch (estado) {
      case EstadoReclamo.Abierto:
        return 'new_releases';
      case EstadoReclamo.EnProceso:
        return 'pending';
      case EstadoReclamo.Resuelto:
        return 'check_circle';
      case EstadoReclamo.Cerrado:
        return 'lock';
      case EstadoReclamo.Rechazado:
        return 'cancel';
      case EstadoReclamo.Escalado:
        return 'warning';
      default:
        return 'info';
    }
  }

  // Helper methods para labels
  getTipoReclamoLabel(tipo: TipoReclamo): string {
    return getTipoReclamoLabel(tipo);
  }

  getEstadoReclamoLabel(estado: EstadoReclamo): string {
    return getEstadoReclamoLabel(estado);
  }

  getPrioridadReclamoLabel(prioridad: PrioridadReclamo): string {
    return getPrioridadReclamoLabel(prioridad);
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