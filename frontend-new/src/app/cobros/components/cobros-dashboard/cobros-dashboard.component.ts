import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { 
  Cobro, 
  CobroStats, 
  CobroRequest,
  EstadoCobro, 
  MetodoPago,
  getEstadoCobroLabel,
  getMetodoPagoLabel
} from '../../interfaces/cobro.interface';
import { CobrosService } from '../../services/cobros.service';
import { CURRENCY_CONSTANTS, MONEDAS_SISTEMA, formatCurrencyByCode } from '../../../shared/constants/currency.constants';
import { ExportService, ExportColumn } from '../../../shared/services/export.service';
import { ExportDialogComponent, ExportDialogData, ExportDialogResult } from '../../../shared/components/export-dialog/export-dialog.component';

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
    MatTooltipModule
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
    'correoElectronico',
    'fechaVencimiento',
    'montoTotal',
    'estado',
    'fechaCobro',
    'metodoPago',
    'acciones'
  ];

  dataSource = new MatTableDataSource<Cobro>();
  cobros: Cobro[] = [];
  cobrosProximos: Cobro[] = [];
  stats: CobroStats | null = null;
  loading = true;
  filtroEstado: number | null = null;
  vistaProximos = true;

  // Enums para el template
  EstadoCobro = EstadoCobro;
  MetodoPago = MetodoPago;

  // Constantes de moneda para el template
  CURRENCY_CONSTANTS = CURRENCY_CONSTANTS;
  MONEDAS_SISTEMA = MONEDAS_SISTEMA;

  constructor(
    private readonly cobrosService: CobrosService,
    private readonly snackBar: MatSnackBar,
    private readonly router: Router,
    private readonly dialog: MatDialog,
    private readonly exportService: ExportService
  ) { 
    console.log('🔧 CobrosDashboardComponent inicializado con CobrosService');
  }

  ngOnInit(): void {
    console.log('🚀 CobrosDashboardComponent.ngOnInit()');
    this.loadCobrosProximos();
    this.loadStats();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadCobros(): void {
    console.log('📥 Cargando todos los cobros...');
    this.loading = true;
    this.vistaProximos = false;
    
    this.cobrosService.getCobros().subscribe({
      next: (cobros) => {
        console.log('✅ Cobros cargados:', cobros.length);
        this.cobros = cobros;
        this.filtroEstado = null;
        this.dataSource.data = cobros;
        this.loading = false;
      },
      error: (error) => {
        console.error('❌ Error al cargar cobros:', error);
        this.showMessage('Error al cargar los cobros');
        this.loading = false;
      }
    });
  }

  loadCobrosProximos(): void {
    console.log('📥 Cargando cobros próximos por periodicidad...');
    this.loading = true;
    this.vistaProximos = true;

    this.cobrosService.getCobrosProximos().subscribe({
      next: (cobros) => {
        console.log('✅ Cobros próximos cargados:', cobros.length);
        this.cobrosProximos = cobros;
        this.filtroEstado = null;
        this.dataSource.data = cobros;
        this.loading = false;
      },
      error: (error) => {
        console.error('❌ Error al cargar cobros próximos:', error);
        this.showMessage('Error al cargar los cobros próximos');
        this.loading = false;
      }
    });
  }

  loadStats(): void {
    console.log('📊 Cargando estadísticas...');
    this.cobrosService.getCobroStats().subscribe({
      next: (stats) => {
        console.log('✅ Estadísticas cargadas:', stats);
        this.stats = stats;
      },
      error: (error) => {
        console.error('❌ Error al cargar estadísticas:', error);
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
    const listaBase = this.vistaProximos ? this.cobrosProximos : this.cobros;
    if (estado === null) {
      this.dataSource.data = listaBase;
    } else {
      this.dataSource.data = listaBase.filter(cobro => cobro.estado === estado);
    }
    this.filtroEstado = estado;
  }

  agregarCobro(): void {
    // Importar el componente del diálogo dinámicamente
    import('../agregar-cobro-dialog/agregar-cobro-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.AgregarCobroDialogComponent, {
        width: '600px',
        disableClose: true,
        panelClass: 'agregar-cobro-dialog'
      });

      dialogRef.afterClosed().subscribe((result: CobroRequest | undefined) => {
        if (result) {
          console.log('✅ Guardando nuevo cobro:', result);
          this.cobrosService.createCobro(result).subscribe({
            next: (cobro) => {
              console.log('✅ Cobro creado exitosamente:', cobro);
              this.showMessage('Cobro creado exitosamente');
              if (this.vistaProximos) {
                this.loadCobrosProximos();
              } else {
                this.loadCobros();
              }
              this.loadStats(); // Recargar estadísticas
            },
            error: (error) => {
              console.error('❌ Error al crear cobro:', error);
              this.showMessage('Error al crear el cobro: ' + (error.error?.message || error.message));
            }
          });
        }
      });
    });
  }

  registrarCobro(cobro: Cobro): void {
    // Aquí se abriría un diálogo para registrar el cobro
    console.log('Registrar cobro:', cobro);
    this.showMessage('Funcionalidad de registro de cobro en desarrollo');
  }

  verDetalle(cobro: Cobro): void {
    this.router.navigate(['/cobros/detalle', cobro.id]);
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
    const dataToExport = this.dataSource.filteredData.length > 0 
      ? this.dataSource.filteredData 
      : this.cobros;

    const dialogData: ExportDialogData = {
      title: 'Exportar Cobros',
      totalRecords: dataToExport.length,
      defaultFilename: `cobros_${this.getCurrentDateString()}`
    };

    const dialogRef = this.dialog.open(ExportDialogComponent, {
      width: '600px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((result: ExportDialogResult) => {
      if (result) {
        this.performExport(dataToExport, result);
      }
    });
  }

  private performExport(data: Cobro[], options: ExportDialogResult): void {
    const columns: ExportColumn[] = [
      { key: 'numeroRecibo', header: 'No. Recibo', type: 'text' },
      { key: 'numeroPoliza', header: 'No. Póliza', type: 'text' },
      { key: 'clienteNombreCompleto', header: 'Cliente', type: 'text' },
      { key: 'correoElectronico', header: 'Correo', type: 'text' },
      { key: 'fechaVencimiento', header: 'Fecha Vencimiento', type: 'date', dateFormat: options.dateFormat },
      { key: 'montoTotal', header: 'Monto Total', type: 'currency', currencyCode: 'CRC' },
      { key: 'estado', header: 'Estado', type: 'text' },
      { key: 'fechaCobro', header: 'Fecha Cobro', type: 'date', dateFormat: options.dateFormat },
      { key: 'metodoPago', header: 'Método Pago', type: 'text' },
      { key: 'montoCobrado', header: 'Monto Cobrado', type: 'currency', currencyCode: 'CRC' },
      { key: 'usuarioCobroNombre', header: 'Usuario Cobro', type: 'text' },
      { key: 'observaciones', header: 'Observaciones', type: 'text' },
      { key: 'fechaCreacion', header: 'Fecha Creación', type: 'date', dateFormat: options.dateFormat }
    ];

    // Transformar datos para incluir labels legibles
    const transformedData = data.map(cobro => ({
      ...cobro,
      estado: getEstadoCobroLabel(cobro.estado),
      metodoPago: cobro.metodoPago ? getMetodoPagoLabel(cobro.metodoPago) : '',
      fechaVencimiento: new Date(cobro.fechaVencimiento),
      fechaCobro: cobro.fechaCobro ? new Date(cobro.fechaCobro) : null,
      fechaCreacion: new Date(cobro.fechaCreacion),
      fechaActualizacion: cobro.fechaActualizacion ? new Date(cobro.fechaActualizacion) : null
    }));

    const exportOptions = {
      filename: options.filename,
      format: options.format as 'csv' | 'excel' | 'pdf',
      includeHeaders: options.includeHeaders,
      dateFormat: options.dateFormat
    };

    try {
      switch (options.format) {
        case 'csv':
          this.exportService.exportToCSV(transformedData, columns, exportOptions);
          break;
        case 'excel':
          this.exportService.exportToExcel(transformedData, columns, exportOptions);
          break;
        case 'pdf':
          this.exportService.exportToPDF(transformedData, columns, exportOptions);
          break;
        default:
          this.exportService.exportToCSV(transformedData, columns, exportOptions);
      }

      this.showMessage(`Archivo exportado exitosamente como ${options.format.toUpperCase()}`);
    } catch (error) {
      console.error('Error al exportar:', error);
      this.showMessage('Error al exportar el archivo');
    }
  }

  private getCurrentDateString(): string {
    const now = new Date();
    const year = now.getFullYear();
    const month = (now.getMonth() + 1).toString().padStart(2, '0');
    const day = now.getDate().toString().padStart(2, '0');
    return `${year}${month}${day}`;
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