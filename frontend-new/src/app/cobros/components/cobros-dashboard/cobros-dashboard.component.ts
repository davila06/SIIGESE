import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { 
  Cobro, 
  CobroStats, 
  EstadoCobro, 
  MetodoPago,
  getEstadoCobroLabel,
  getMetodoPagoLabel
} from '../../interfaces/cobro.interface';
import { CobrosService } from '../../services/cobros.service';
import { CURRENCY_CONSTANTS, MONEDAS_SISTEMA, formatCurrencyByCode } from '../../../shared/constants/currency.constants';
import { CobroDetalleDialogComponent } from '../cobro-detalle-dialog/cobro-detalle-dialog.component';

@Component({
  selector: 'app-cobros-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatSnackBarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDialogModule
  ],
  templateUrl: './cobros-dashboard.component.html',
  styleUrls: ['./cobros-dashboard.component.scss']
})
export class CobrosDashboardComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = [
    'numeroRecibo',
    'numeroPoliza',
    'cliente',
    'fechaVencimiento',
    'montoTotal',
    'estado',
    'fechaCobro',
    'metodoPago',
    'acciones'
  ];

  dataSource = new MatTableDataSource<Cobro>();
  cobros: Cobro[] = [];
  stats: CobroStats | null = null;
  loading = true;
  filtroEstado: number | null = null;

  // Enums para el template
  EstadoCobro = EstadoCobro;
  MetodoPago = MetodoPago;

  // Constantes de moneda para el template
  CURRENCY_CONSTANTS = CURRENCY_CONSTANTS;
  MONEDAS_SISTEMA = MONEDAS_SISTEMA;

  constructor(
    private cobrosService: CobrosService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.loadCobros();
    this.loadStats();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadCobros(): void {
    this.loading = true;
    
    this.cobrosService.getCobros().subscribe({
      next: (cobros) => {
        console.log('=== DEBUG COBROS ===');
        console.log('Total cobros recibidos:', cobros.length);
        cobros.forEach((cobro, index) => {
          console.log(`Cobro ${index + 1}:`, {
            numeroRecibo: cobro.numeroRecibo,
            estado: cobro.estado,
            tipoEstado: typeof cobro.estado,
            clienteNombre: cobro.clienteNombre
          });
        });
        console.log('=== FIN DEBUG ===');
        
        this.cobros = cobros;
        this.dataSource.data = cobros;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar cobros:', error);
        this.showMessage('Error al cargar los cobros');
        this.loading = false;
      }
    });
  }

  loadStats(): void {
    this.cobrosService.getCobroStats().subscribe({
      next: (stats) => {
        this.stats = stats;
      },
      error: (error) => {
        console.error('Error al cargar estadísticas:', error);
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

  filtrarPorEstado(estado: number | null): void {
    if (estado === null) {
      this.dataSource.data = this.cobros;
    } else {
      this.dataSource.data = this.cobros.filter(cobro => cobro.estado === estado);
    }
    this.filtroEstado = estado;
  }

  registrarCobro(cobro: Cobro): void {
    // Aquí se abriría un diálogo para registrar el cobro
    console.log('Registrar cobro:', cobro);
    this.showMessage('Funcionalidad de registro de cobro en desarrollo');
  }

  verDetalle(cobro: Cobro): void {
    const dialogRef = this.dialog.open(CobroDetalleDialogComponent, {
      width: '800px',
      maxWidth: '90vw',
      data: cobro
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Si se realizó alguna acción, recargar los datos
        this.loadCobros();
      }
    });
  }

  cancelarCobro(cobro: Cobro): void {
    // Aquí se confirmaría y cancelaría el cobro
    console.log('Cancelar cobro:', cobro);
    this.showMessage('Funcionalidad de cancelación en desarrollo');
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

  exportarCobros(): void {
    this.showMessage('Funcionalidad de exportación en desarrollo');
  }

  formatCurrency(amount: number, currencyCode: string = CURRENCY_CONSTANTS.DEFAULT_CURRENCY): string {
    return formatCurrencyByCode(amount, currencyCode);
  }

  getCurrencySymbol(currencyCode: string = CURRENCY_CONSTANTS.DEFAULT_CURRENCY): string {
    const currency = MONEDAS_SISTEMA.find(m => m.value === currencyCode);
    return currency?.symbol || '₡';
  }

  // Funciones helper para obtener labels de enums
  getEstadoLabel(estado: EstadoCobro): string {
    return getEstadoCobroLabel(estado);
  }

  getEstadoCobroLabel(estado: any): string {
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