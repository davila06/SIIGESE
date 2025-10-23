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
    MatTooltipModule
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
    private route: ActivatedRoute,
    private router: Router,
    private cobrosService: CobrosService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.cobroId = +id;
        this.loadCobroDetalle();
        this.loadMovimientos();
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
    if (!this.cobroId) return;

    // Datos mock para movimientos
    this.movimientos = [
      {
        id: 1,
        fecha: new Date('2024-01-15T09:00:00'),
        descripcion: 'Cobro creado automáticamente',
        usuario: 'Sistema',
        tipo: 'creacion'
      },
      {
        id: 2,
        fecha: new Date('2024-01-20T14:30:00'),
        descripcion: 'Fecha de vencimiento extendida',
        usuario: 'admin@sinseg.com',
        tipo: 'modificacion'
      }
    ];

    // Si el cobro está cobrado, agregar movimiento de cobro
    if (this.cobro && this.cobro.estado === EstadoCobro.Cobrado) {
      this.movimientos.push({
        id: 3,
        fecha: this.cobro.fechaCobro || new Date(),
        descripcion: 'Cobro registrado exitosamente',
        usuario: 'admin@sinseg.com',
        tipo: 'cobro',
        estadoAnterior: EstadoCobro.Pendiente,
        estadoNuevo: EstadoCobro.Cobrado
      });
    }
  }

  loadDocumentos(): void {
    if (!this.cobroId) return;

    // Datos mock para documentos
    this.documentos = [
      {
        id: 1,
        nombre: `recibo_${this.cobroId}.pdf`,
        tipo: 'PDF',
        tamanio: 245760, // 240 KB
        fechaSubida: new Date('2024-01-15T09:00:00'),
        usuario: 'Sistema',
        url: '#'
      },
      {
        id: 2,
        nombre: `comprobante_pago_${this.cobroId}.jpg`,
        tipo: 'Imagen',
        tamanio: 512000, // 500 KB
        fechaSubida: new Date('2024-01-18T16:45:00'),
        usuario: 'admin@sinseg.com',
        url: '#'
      }
    ];
  }

  volver(): void {
    this.router.navigate(['/cobros']);
  }

  registrarCobro(): void {
    if (!this.cobro) return;
    
    // Implementar lógica de registro de cobro
    this.showMessage('Funcionalidad de registro de cobro en desarrollo');
  }

  cancelarCobro(): void {
    if (!this.cobro) return;
    
    // Implementar lógica de cancelación
    this.showMessage('Funcionalidad de cancelación en desarrollo');
  }

  editarCobro(): void {
    if (!this.cobro) return;
    
    // Navegar a formulario de edición
    this.showMessage('Funcionalidad de edición en desarrollo');
  }

  imprimirRecibo(): void {
    if (!this.cobro) return;
    
    // Implementar impresión de recibo
    this.showMessage('Funcionalidad de impresión en desarrollo');
  }

  descargarDocumento(documento: DocumentoCobro): void {
    // Implementar descarga de documento
    this.showMessage(`Descargando ${documento.nombre}...`);
  }

  eliminarDocumento(documento: DocumentoCobro): void {
    // Implementar eliminación de documento
    this.showMessage('Funcionalidad de eliminación de documentos en desarrollo');
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
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
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