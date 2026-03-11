import { Component, OnInit, ViewChild, AfterViewInit, QueryList, ViewChildren } from '@angular/core';
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
import { MatTabsModule, MatTabChangeEvent } from '@angular/material/tabs';
import { MatBadgeModule } from '@angular/material/badge';
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

export interface PeriodicidadTab {
  label: string;
  frecuencia: string | null;  // null = "Todos"
  icon: string;
  cobros: Cobro[];
  dataSource: MatTableDataSource<Cobro>;
  loading: boolean;
  loaded: boolean;
}

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
    MatTabsModule,
    MatBadgeModule
  ],
  templateUrl: './cobros-dashboard.component.html',
  styleUrls: ['./cobros-dashboard.component.scss']
})
export class CobrosDashboardComponent implements OnInit, AfterViewInit {
  @ViewChildren(MatPaginator) paginators!: QueryList<MatPaginator>;
  @ViewChildren(MatSort) sorts!: QueryList<MatSort>;

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

  // ─── Tabs de periodicidad ───────────────────────────────────────────────────
  periodicidadTabs: PeriodicidadTab[] = [
    { label: 'Próximos', frecuencia: 'PROXIMOS', icon: 'upcoming', cobros: [], dataSource: new MatTableDataSource<Cobro>(), loading: false, loaded: false },
    { label: 'Mensual',  frecuencia: 'MENSUAL',  icon: 'calendar_month', cobros: [], dataSource: new MatTableDataSource<Cobro>(), loading: false, loaded: false },
    { label: 'Bimestral', frecuencia: 'BIMESTRAL', icon: 'event_repeat', cobros: [], dataSource: new MatTableDataSource<Cobro>(), loading: false, loaded: false },
    { label: 'Trimestral', frecuencia: 'TRIMESTRAL', icon: 'date_range', cobros: [], dataSource: new MatTableDataSource<Cobro>(), loading: false, loaded: false },
    { label: 'Cuatrimestral', frecuencia: 'CUATRIMESTRAL', icon: 'event_note', cobros: [], dataSource: new MatTableDataSource<Cobro>(), loading: false, loaded: false },
    { label: 'Semestral', frecuencia: 'SEMESTRAL', icon: 'calendar_today', cobros: [], dataSource: new MatTableDataSource<Cobro>(), loading: false, loaded: false },
    { label: 'Anual',    frecuencia: 'ANUAL',    icon: 'calendar_view_year', cobros: [], dataSource: new MatTableDataSource<Cobro>(), loading: false, loaded: false },
    { label: 'Todos',    frecuencia: null,       icon: 'list_alt', cobros: [], dataSource: new MatTableDataSource<Cobro>(), loading: false, loaded: false },
  ];

  selectedTabIndex = 0;
  stats: CobroStats | null = null;
  filtroEstadoMap: Map<number, number | null> = new Map(); // tabIndex → estado filter
  searchValueMap: Map<number, string> = new Map();         // tabIndex → search string
  emailSendingMap: Map<number, boolean> = new Map();       // cobro.id → sending state

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
    this.loadStats();
    this.loadTab(0); // Cargar el primer tab (Próximos) al inicio
  }

  ngAfterViewInit(): void {
    // Paginators/sorts se asignan dinámicamente en loadTab
  }

  // ─── Carga de tabs ──────────────────────────────────────────────────────────
  onTabChange(event: MatTabChangeEvent): void {
    this.selectedTabIndex = event.index;
    const tab = this.periodicidadTabs[event.index];
    if (!tab.loaded) {
      this.loadTab(event.index);
    } else {
      // Re-assign paginator/sort; lazy tabs destroy/recreate DOM on switch
      this.assignPaginatorSort(event.index);
    }
  }

  loadTab(index: number): void {
    const tab = this.periodicidadTabs[index];
    if (tab.loading) return;

    tab.loading = true;

    const obs = tab.frecuencia === null
      ? this.cobrosService.getCobros()
      : tab.frecuencia === 'PROXIMOS'
        ? this.cobrosService.getCobrosProximos()
        : this.cobrosService.getCobrosByFrecuencia(tab.frecuencia);

    obs.subscribe({
      next: (cobros) => {
        console.log(`📋 [DEBUG] Tab "${tab.label}": ${cobros.length} cobros recibidos`);
        cobros.forEach((c, i) => {
          if (c.montoTotal === null || c.montoTotal === undefined || (typeof c.montoTotal !== 'number' && isNaN(Number(c.montoTotal)))) {
            console.warn(`⚠️ [DEBUG] cobro[${i}] id=${c.id} montoTotal INVÁLIDO:`, c.montoTotal, '| tipo:', typeof c.montoTotal);
          }
          if (c.fechaVencimiento) {
            const dv = new Date(c.fechaVencimiento);
            if (isNaN(dv.getTime())) console.warn(`⚠️ [DEBUG] cobro[${i}] id=${c.id} fechaVencimiento INVÁLIDA:`, c.fechaVencimiento);
          }
          if (c.fechaCobro) {
            const dc = new Date(c.fechaCobro);
            if (isNaN(dc.getTime())) console.warn(`⚠️ [DEBUG] cobro[${i}] id=${c.id} fechaCobro INVÁLIDA:`, c.fechaCobro);
          }
        });
        tab.cobros = cobros;
        tab.dataSource.data = cobros;
        tab.loading = false;
        tab.loaded = true;
        this.assignPaginatorSort(index);
      },
      error: () => {
        this.showMessage(`Error al cargar cobros ${tab.label}`);
        tab.loading = false;
      }
    });
  }

  reloadCurrentTab(): void {
    const tab = this.periodicidadTabs[this.selectedTabIndex];
    tab.loaded = false;
    this.loadTab(this.selectedTabIndex);
  }

  private assignPaginatorSort(index: number): void {
    setTimeout(() => {
      const tab = this.periodicidadTabs[index];
      // With lazy tab rendering only the active tab's DOM exists,
      // so .first always references the current tab's paginator/sort
      if (this.paginators.first) tab.dataSource.paginator = this.paginators.first;
      if (this.sorts.first)     tab.dataSource.sort     = this.sorts.first;
    });
  }

  // ─── Helpers para pipes seguros ─────────────────────────────────────────────
  safeNumber(value: any): number {
    if (value === null || value === undefined) return 0;
    const n = Number(value);
    return isNaN(n) ? 0 : n;
  }

  safeDate(value: any): Date | null {
    if (!value) return null;
    const d = new Date(value);
    return isNaN(d.getTime()) ? null : d;
  }

  loadStats(): void {
    this.cobrosService.getCobroStats().subscribe({
      next: (stats) => {
        console.log('📊 [DEBUG] Stats raw:', JSON.stringify(stats));
        console.log('📊 [DEBUG] montoTotalPendiente ->', typeof stats.montoTotalPendiente, '=', stats.montoTotalPendiente);
        console.log('📊 [DEBUG] montoTotalCobrado  ->', typeof stats.montoTotalCobrado,  '=', stats.montoTotalCobrado);
        console.log('📊 [DEBUG] montoPorVencer     ->', typeof stats.montoPorVencer,     '=', stats.montoPorVencer);
        this.stats = stats;
      },
      error: () => {}
    });
  }

  // ─── Filtros de búsqueda/estado ─────────────────────────────────────────────
  applyFilter(event: Event, tabIndex: number): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.searchValueMap.set(tabIndex, filterValue);
    const tab = this.periodicidadTabs[tabIndex];
    tab.dataSource.filter = filterValue.trim().toLowerCase();
    if (tab.dataSource.paginator) tab.dataSource.paginator.firstPage();
  }

  getSearchValue(tabIndex: number): string {
    return this.searchValueMap.get(tabIndex) ?? '';
  }

  filtrarPorEstado(estado: number | null, tabIndex: number): void {
    this.filtroEstadoMap.set(tabIndex, estado);
    const tab = this.periodicidadTabs[tabIndex];
    const base = tab.cobros;
    tab.dataSource.data = estado === null ? base : base.filter(c => c.estado === estado);
    if (tab.dataSource.paginator) tab.dataSource.paginator.firstPage();
  }

  getFiltroEstado(tabIndex: number): number | null {
    return this.filtroEstadoMap.get(tabIndex) ?? null;
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
          this.cobrosService.createCobro(result).subscribe({
            next: () => {
              this.showMessage('Cobro creado exitosamente');
              this.reloadCurrentTab();
              this.loadStats();
            },
            error: (error) => {
              this.showMessage('Error al crear el cobro: ' + (error.error?.message || error.message));
            }
          });
        }
      });
    });
  }

  registrarCobro(cobro: Cobro): void {
    import('../registrar-cobro-dialog/registrar-cobro-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.RegistrarCobroDialogComponent, {
        width: '500px',
        disableClose: true,
        data: { cobro }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.cobrosService.registrarCobro(result).subscribe({
            next: () => {
              this.showMessage('Cobro registrado exitosamente');
              this.reloadCurrentTab();
              this.loadStats();
            },
            error: (err) => {
              this.showMessage('Error al registrar cobro: ' + (err?.error?.message || err?.message || 'Error desconocido'));
            }
          });
        }
      });
    });
  }

  verDetalle(cobro: Cobro): void {
    this.router.navigate(['/cobros/detalle', cobro.id]);
  }

  enviarEmail(cobro: Cobro): void {
    if (!cobro.correoElectronico) {
      this.showMessage('Este cobro no tiene correo electrónico asociado');
      return;
    }
    this.emailSendingMap.set(cobro.id, true);
    this.cobrosService.enviarEmailCobro(cobro.id).subscribe({
      next: (res) => {
        this.emailSendingMap.delete(cobro.id);
        this.showMessage(res.message || `Email enviado a ${cobro.correoElectronico}`);
      },
      error: (err) => {
        this.emailSendingMap.delete(cobro.id);
        const msg = err?.error?.message || err?.message || 'Error desconocido';
        this.showMessage('Error al enviar email: ' + msg);
      }
    });
  }

  isEmailSending(cobroId: number): boolean {
    return this.emailSendingMap.get(cobroId) ?? false;
  }

  cancelarCobro(cobro: Cobro): void {
    import('../cancelar-cobro-dialog/cancelar-cobro-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.CancelarCobroDialogComponent, {
        width: '450px',
        disableClose: true,
        data: { cobro }
      });

      dialogRef.afterClosed().subscribe((motivo: string | undefined) => {
        if (motivo !== undefined) {
          this.cobrosService.cancelarCobro(cobro.id, motivo).subscribe({
            next: () => {
              this.showMessage('Cobro cancelado exitosamente');
              this.reloadCurrentTab();
              this.loadStats();
            },
            error: (err) => {
              this.showMessage('Error al cancelar cobro: ' + (err?.error?.message || err?.message || 'Error desconocido'));
            }
          });
        }
      });
    });
  }

  getEstadoColor(estado: EstadoCobro): string {
    switch (estado) {
      case EstadoCobro.Pendiente: return 'primary';
      case EstadoCobro.Cobrado:   return 'accent';
      case EstadoCobro.Vencido:   return 'warn';
      default:                    return '';
    }
  }

  getEstadoIcon(estado: EstadoCobro): string {
    switch (estado) {
      case EstadoCobro.Pendiente:  return 'schedule';
      case EstadoCobro.Cobrado:    return 'check_circle';
      case EstadoCobro.Vencido:    return 'warning';
      case EstadoCobro.Cancelado:  return 'cancel';
      default:                     return 'help';
    }
  }

  getEstadoBadgeClass(estado: EstadoCobro): string {
    switch (estado) {
      case EstadoCobro.Pendiente:  return 'badge-pendiente';
      case EstadoCobro.Cobrado:    return 'badge-cobrado';
      case EstadoCobro.Vencido:    return 'badge-vencido';
      case EstadoCobro.Cancelado:  return 'badge-cancelado';
      default:                     return '';
    }
  }

  exportarCobros(): void {
    const tab = this.periodicidadTabs[this.selectedTabIndex];
    const dataToExport = tab.dataSource.filteredData.length > 0
      ? tab.dataSource.filteredData
      : tab.cobros;

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