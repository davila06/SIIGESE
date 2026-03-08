import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatListModule } from '@angular/material/list';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { 
  Cobro, 
  EstadoCobro, 
  MetodoPago,
  getEstadoCobroLabel,
  getMetodoPagoLabel
} from '../../interfaces/cobro.interface';
import { CobrosService } from '../../services/cobros.service';
import { CURRENCY_CONSTANTS, MONEDAS_SISTEMA, formatCurrencyByCode } from '../../../shared/constants/currency.constants';

interface MovimientoCobro {
  id: number;
  fecha: Date;
  descripcion: string;
  usuario: string;
  tipo: 'creacion' | 'cobro' | 'cancelacion' | 'modificacion';
  montoAnterior?: number;
  montoNuevo?: number;
  estadoAnterior?: EstadoCobro;
  estadoNuevo?: EstadoCobro;
}

interface DocumentoCobro {
  id: number;
  nombre: string;
  tipo: string;
  tamanio: number;
  fechaSubida: Date;
  usuario: string;
  url: string;
}

@Component({
  selector: 'app-cobro-detalle',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatListModule,
    MatTableModule,
    MatTooltipModule,
    MatDialogModule
  ],
  templateUrl: './cobro-detalle.component.html',
  styleUrls: ['./cobro-detalle.component.scss']
})
export class CobroDetalleComponent implements OnInit {
  cobro: Cobro | null = null;
  cobroId: number | null = null;
  loading = true;
  movimientos: MovimientoCobro[] = [];
  documentos: DocumentoCobro[] = [];

  // Columns para tablas
  movimientosColumns: string[] = ['fecha', 'descripcion', 'usuario', 'cambios'];
  documentosColumns: string[] = ['nombre', 'tipo', 'tamaño', 'fechaSubida', 'usuario', 'acciones'];

  // Enums para template
  EstadoCobro = EstadoCobro;
  MetodoPago = MetodoPago;

  // Constantes de moneda para el template
  CURRENCY_CONSTANTS = CURRENCY_CONSTANTS;
  MONEDAS_SISTEMA = MONEDAS_SISTEMA;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cobrosService: CobrosService,
    private readonly snackBar: MatSnackBar,
    private readonly dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.cobroId = +id;
        this.loadCobroDetalle();
        this.loadDocumentos();
      } else {
        this.showMessage('ID de cobro no válido');
        this.router.navigate(['/cobros']);
      }
    });
  }

  loadCobroDetalle(): void {
    if (!this.cobroId) return;

    this.loading = true;
    this.cobrosService.getCobro(this.cobroId).subscribe({
      next: (cobro: Cobro) => {
        this.cobro = cobro;
        this.loading = false;
        this.loadMovimientos();
      },
      error: (error: any) => {
        console.error('Error al cargar detalle del cobro:', error);
        this.showMessage('Error al cargar el detalle del cobro');
        this.loading = false;
        this.router.navigate(['/cobros']);
      }
    });
  }

  loadMovimientos(): void {
    if (!this.cobro) return;

    const movimientos: MovimientoCobro[] = [
      {
        id: 1,
        fecha: new Date(this.cobro.fechaCreacion),
        descripcion: 'Cobro creado',
        usuario: 'Sistema',
        tipo: 'creacion'
      }
    ];

    if (this.cobro.estado === EstadoCobro.Cobrado && this.cobro.fechaCobro) {
      movimientos.push({
        id: 2,
        fecha: new Date(this.cobro.fechaCobro),
        descripcion: 'Cobro registrado',
        usuario: this.cobro.usuarioCobroNombre || 'Sistema',
        tipo: 'cobro',
        estadoAnterior: EstadoCobro.Pendiente,
        estadoNuevo: EstadoCobro.Cobrado
      });
    }

    if (this.cobro.estado === EstadoCobro.Cancelado && this.cobro.fechaActualizacion) {
      movimientos.push({
        id: 3,
        fecha: new Date(this.cobro.fechaActualizacion),
        descripcion: 'Cobro cancelado',
        usuario: 'Sistema',
        tipo: 'cancelacion',
        estadoAnterior: EstadoCobro.Pendiente,
        estadoNuevo: EstadoCobro.Cancelado
      });
    }

    this.movimientos = [...movimientos].sort((a, b) => b.fecha.getTime() - a.fecha.getTime());
  }

  loadDocumentos(): void {
    // Document management is not yet implemented in the backend.
    this.documentos = [];
  }

  volver(): void {
    this.router.navigate(['/cobros']);
  }

  registrarCobro(): void {
    if (!this.cobro) return;

    import('../registrar-cobro-dialog/registrar-cobro-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.RegistrarCobroDialogComponent, {
        width: '500px',
        disableClose: true,
        data: { cobro: this.cobro }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.cobrosService.registrarCobro(result).subscribe({
            next: (updated) => {
              this.cobro = updated;
              this.showMessage('Cobro registrado exitosamente');
              this.loadMovimientos();
            },
            error: (err) => {
              this.showMessage('Error al registrar cobro: ' + (err?.error?.message || 'Error desconocido'));
            }
          });
        }
      });
    });
  }

  cancelarCobro(): void {
    if (!this.cobro) return;

    import('../cancelar-cobro-dialog/cancelar-cobro-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.CancelarCobroDialogComponent, {
        width: '450px',
        disableClose: true,
        data: { cobro: this.cobro }
      });

      dialogRef.afterClosed().subscribe((motivo: string | undefined) => {
        if (motivo !== undefined) {
          this.cobrosService.cancelarCobro(this.cobro!.id, motivo).subscribe({
            next: (updated) => {
              this.cobro = updated;
              this.showMessage('Cobro cancelado exitosamente');
              this.loadMovimientos();
            },
            error: (err) => {
              this.showMessage('Error al cancelar cobro: ' + (err?.error?.message || 'Error desconocido'));
            }
          });
        }
      });
    });
  }

  editarCobro(): void {
    if (!this.cobro) return;
    this.showMessage('La edición de cobros aún no está disponible');
  }

  imprimirRecibo(): void {
    globalThis.print();
  }

  descargarDocumento(documento: DocumentoCobro): void {
    if (documento.url && documento.url !== '#') {
      const link = document.createElement('a');
      link.href = documento.url;
      link.download = documento.nombre;
      link.click();
    } else {
      this.showMessage('Documento no disponible para descarga');
    }
  }

  eliminarDocumento(_documento: DocumentoCobro): void {
    this.showMessage('La gestión de documentos aún no está disponible');
  }

  getEstadoColor(estado: EstadoCobro): string {
    switch (estado) {
      case EstadoCobro.Pendiente:
        return 'primary';
      case EstadoCobro.Cobrado:
        return 'accent';
      case EstadoCobro.Vencido:
        return 'warn';
      case EstadoCobro.Cancelado:
        return '';
      default:
        return '';
    }
  }

  getEstadoIcon(estado: EstadoCobro): string {
    switch (estado) {
      case EstadoCobro.Pendiente:
        return 'schedule';
      case EstadoCobro.Cobrado:
        return 'check_circle';
      case EstadoCobro.Vencido:
        return 'warning';
      case EstadoCobro.Cancelado:
        return 'cancel';
      default:
        return 'help';
    }
  }

  getTipoMovimientoIcon(tipo: string): string {
    switch (tipo) {
      case 'creacion':
        return 'add_circle';
      case 'cobro':
        return 'payment';
      case 'cancelacion':
        return 'cancel';
      case 'modificacion':
        return 'edit';
      default:
        return 'info';
    }
  }

  getTipoMovimientoColor(tipo: string): string {
    switch (tipo) {
      case 'creacion':
        return 'primary';
      case 'cobro':
        return 'accent';
      case 'cancelacion':
        return 'warn';
      case 'modificacion':
        return '';
      default:
        return '';
    }
  }

  formatCurrency(amount: number, currencyCode: string = CURRENCY_CONSTANTS.DEFAULT_CURRENCY): string {
    return formatCurrencyByCode(amount, currencyCode);
  }

  getCurrencySymbol(currencyCode: string = CURRENCY_CONSTANTS.DEFAULT_CURRENCY): string {
    const currency = MONEDAS_SISTEMA.find(m => m.value === currencyCode);
    return currency?.symbol || '₡';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Number.parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  // Funciones helper para obtener labels de enums
  getEstadoLabel(estado: EstadoCobro): string {
    return getEstadoCobroLabel(estado);
  }

  getMetodoPagoLabel(metodo: MetodoPago): string {
    return getMetodoPagoLabel(metodo);
  }

  private showMessage(message: string): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }
}