import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { 
  Reclamo,
  ReclamosStats,
  TipoReclamo,
  EstadoReclamo,
  PrioridadReclamo
} from '../../interfaces/reclamo.interface';
import { ReclamosService } from '../../services/reclamos.service';
import { ExportService, ExportColumn } from '../../../shared/services/export.service';
import { ExportDialogComponent, ExportDialogData, ExportDialogResult } from '../../../shared/components/export-dialog/export-dialog.component';

@Component({
  selector: 'app-reclamos-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatSnackBarModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatMenuModule,
    MatBadgeModule,
    MatDialogModule,
    DatePipe,
    DecimalPipe
  ],
  templateUrl: './reclamos-dashboard.component.html',
  styleUrls: ['./reclamos-dashboard.component.scss']
})
export class ReclamosDashboardComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = [
    'numeroReclamo',
    'numeroPoliza',
    'cliente',
    'tipoReclamo',
    'fechaReclamo',
    'montoReclamado',
    'estado',
    'prioridad',
    'asignadoA',
    'acciones'
  ];

  dataSource = new MatTableDataSource<Reclamo>();
  reclamos: Reclamo[] = [];
  stats: ReclamosStats | null = null;
  loading = true;
  // Cambiar tipo de filtros para usar números en lugar de strings
  filtroEstado: number | null = null;
  filtroTipo: number | null = null;
  filtroPrioridad: number | null = null;

  // Enums para el template
  TipoReclamo = TipoReclamo;
  EstadoReclamo = EstadoReclamo;
  PrioridadReclamo = PrioridadReclamo;

  constructor(
    private readonly reclamosService: ReclamosService,
    private readonly snackBar: MatSnackBar,
    private readonly router: Router,
    private readonly dialog: MatDialog,
    private readonly exportService: ExportService
  ) { }

  ngOnInit(): void {
    this.loadReclamos();
    this.loadStats();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    
    // Configurar sortingDataAccessor personalizado para campos complejos
    this.dataSource.sortingDataAccessor = (data: Reclamo, sortHeaderId: string) => {
      switch (sortHeaderId) {
        case 'cliente':
          return `${data.clienteNombreCompleto}`.toLowerCase();
        case 'asignadoA':
          return data.usuarioAsignadoNombre?.toLowerCase() || 'zzz'; // 'zzz' para que aparezcan al final
        case 'tipoReclamo':
          return data.tipoReclamo; // Usar el valor del enum directamente
        case 'estado':
          return data.estado; // Usar el valor del enum directamente
        case 'prioridad':
          return data.prioridad; // Usar el valor del enum directamente
        case 'fechaReclamo':
          return new Date(data.fechaReclamo).getTime();
        case 'montoReclamado':
          return data.montoReclamado || 0;
        default:
          return (data as unknown as Record<string, string | number>)[sortHeaderId] ?? '';
      }
    };
  }

  loadReclamos(): void {
    this.loading = true;
    
    this.reclamosService.getReclamos().subscribe({
      next: (reclamos: Reclamo[]) => {
        this.reclamos = reclamos;
        this.dataSource.data = this.reclamos;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar reclamos:', error);
        this.showMessage('Error al cargar los reclamos');
        this.loading = false;
      }
    });
  }

  loadStats(): void {
    this.reclamosService.getReclamosStats().subscribe({
      next: (stats) => {
        this.stats = stats;
      },
      error: (error) => {
        console.error('Error al cargar estadísticas:', error);
        this.showMessage('Error al cargar las estadísticas');
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
    this.filtroEstado = estado;
    this.aplicarFiltros();
  }

  filtrarPorTipo(tipo: number | null): void {
    this.filtroTipo = tipo;
    this.aplicarFiltros();
  }

  filtrarPorPrioridad(prioridad: number | null): void {
    this.filtroPrioridad = prioridad;
    this.aplicarFiltros();
  }

  private aplicarFiltros(): void {
    let reclamosFiltrados = [...this.reclamos];

    if (this.filtroEstado !== null) {
      reclamosFiltrados = reclamosFiltrados.filter(r => r.estado === this.filtroEstado);
    }

    if (this.filtroTipo !== null) {
      reclamosFiltrados = reclamosFiltrados.filter(r => r.tipoReclamo === this.filtroTipo);
    }

    if (this.filtroPrioridad !== null) {
      reclamosFiltrados = reclamosFiltrados.filter(r => r.prioridad === this.filtroPrioridad);
    }

    this.dataSource.data = reclamosFiltrados;
  }

  limpiarFiltros(): void {
    this.filtroEstado = null;
    this.filtroTipo = null;
    this.filtroPrioridad = null;
    this.dataSource.data = this.reclamos;
  }

  asignarReclamo(reclamo: Reclamo): void {
    console.log('=== MÉTODO ASIGNAR RECLAMO ACTUALIZADO - TIMESTAMP: 2025-10-27 23:51 ===');
    console.log('Iniciando proceso de asignación para reclamo:', reclamo);
    
    // Importación dinámica del componente del diálogo
    import('../asignar-reclamo-dialog/asignar-reclamo-dialog.component').then(({ AsignarReclamoDialogComponent }) => {
      console.log('Componente del diálogo cargado exitosamente');
      
      const dialogRef = this.dialog.open(AsignarReclamoDialogComponent, {
        width: '500px',
        data: { reclamo }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          console.log('Usuario seleccionado:', result);
          this.loading = true;
          this.reclamosService.asignarReclamo(reclamo.id, result.usuarioId).subscribe({
            next: (response) => {
              console.log('Asignación exitosa:', response);
              this.showMessage(`Reclamo asignado exitosamente a ${result.usuarioNombre}`);
              // Actualizar el reclamo en la lista local
              const index = this.reclamos.findIndex(r => r.id === reclamo.id);
              if (index !== -1) {
                this.reclamos[index] = { 
                  ...this.reclamos[index], 
                  usuarioAsignadoId: result.usuarioId,
                  usuarioAsignadoNombre: result.usuarioNombre 
                };
                this.dataSource.data = [...this.reclamos];
              }
              // Recargar estadísticas
              this.loadStats();
              this.loading = false;
            },
            error: (error) => {
              console.error('Error asignando reclamo:', error);
              this.showMessage('Error al asignar el reclamo');
              this.loading = false;
            }
          });
        } else {
          console.log('Asignación cancelada por el usuario');
        }
      });
    }).catch(error => {
      console.error('Error cargando el diálogo:', error);
      this.showMessage('Error al cargar el diálogo de asignación');
    });
  }

  cambiarEstado(reclamo: Reclamo): void {
    console.log('Cambiar estado:', reclamo);
    
    // Usar import dinámico para el diálogo
    import('../cambiar-estado-dialog').then(module => {
      const dialogRef = this.dialog.open(module.CambiarEstadoDialogComponent, {
        width: '500px',
        data: {
          numeroReclamo: reclamo.numeroReclamo,
          estadoActual: reclamo.estado
        },
        disableClose: false,
        autoFocus: true
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          console.log('🔄 Cambiando estado del reclamo:', {
            reclamoId: reclamo.id,
            numeroReclamo: reclamo.numeroReclamo,
            estadoActual: reclamo.estado,
            nuevoEstado: result.nuevoEstado,
            comentario: result.comentario
          });

          this.reclamosService.cambiarEstado(reclamo.id, result.nuevoEstado, result.comentario || `Estado cambiado a ${this.getEstadoLabel(result.nuevoEstado)}`).subscribe({
            next: (response) => {
              const estadoLabel = this.getEstadoLabel(result.nuevoEstado);
              this.showMessage(`Estado del reclamo ${reclamo.numeroReclamo} cambiado a "${estadoLabel}" exitosamente`);
              this.loadReclamos(); // Recargar la lista
              this.loadStats(); // Recargar estadísticas
            },
            error: (error) => {
              console.error('❌ Error cambiando estado:', error);
              this.showMessage('Error al cambiar el estado del reclamo');
            }
          });
        }
      });
    }).catch(error => {
      console.error('❌ Error cargando diálogo de cambio de estado:', error);
      this.showMessage('Error al cargar el diálogo de cambio de estado');
    });
  }

  private getEstadoLabel(estado: EstadoReclamo): string {
    const estadosLabels = {
      [EstadoReclamo.Abierto]: 'Abierto',
      [EstadoReclamo.EnProceso]: 'En Proceso',
      [EstadoReclamo.Resuelto]: 'Resuelto',
      [EstadoReclamo.Cerrado]: 'Cerrado',
      [EstadoReclamo.Rechazado]: 'Rechazado',
      [EstadoReclamo.Escalado]: 'Escalado'
    };
    return estadosLabels[estado] || 'Desconocido';
  }

  subirDocumento(reclamo: Reclamo): void {
    console.log('Subir documento:', reclamo);
    this.showMessage('Funcionalidad de carga de documentos en desarrollo');
  }

  getEstadoColor(estado: EstadoReclamo): string {
    switch (estado) {
      case EstadoReclamo.Abierto:
        return 'primary';
      case EstadoReclamo.EnProceso:
        return 'accent';
      case EstadoReclamo.Resuelto:
        return 'accent';
      case EstadoReclamo.Cerrado:
        return '';
      case EstadoReclamo.Rechazado:
        return 'warn';
      case EstadoReclamo.Escalado:
        return 'warn';
      default:
        return '';
    }
  }

  getEstadoIcon(estado: EstadoReclamo): string {
    switch (estado) {
      case EstadoReclamo.Abierto:
        return 'schedule';
      case EstadoReclamo.EnProceso:
        return 'rate_review';
      case EstadoReclamo.Resuelto:
        return 'task_alt';
      case EstadoReclamo.Cerrado:
        return 'lock';
      case EstadoReclamo.Rechazado:
        return 'cancel';
      case EstadoReclamo.Escalado:
        return 'priority_high';
      default:
        return 'help';
    }
  }

  getPrioridadColor(prioridad: PrioridadReclamo): string {
    switch (prioridad) {
      case PrioridadReclamo.Baja:
        return '#4CAF50';
      case PrioridadReclamo.Media:
        return '#FF9800';
      case PrioridadReclamo.Alta:
        return '#F44336';
      case PrioridadReclamo.Critica:
        return '#9C27B0';
      default:
        return '#757575';
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

  getTipoReclamoText(tipo: TipoReclamo): string {
    switch (tipo) {
      case TipoReclamo.Siniestro:
        return 'Siniestro';
      case TipoReclamo.Servicio:
        return 'Servicio';
      case TipoReclamo.Facturacion:
        return 'Facturación';
      case TipoReclamo.Cobertura:
        return 'Cobertura';
      case TipoReclamo.Proceso:
        return 'Proceso';
      case TipoReclamo.Otro:
        return 'Otro';
      default:
        return 'Desconocido';
    }
  }

  getEstadoText(estado: EstadoReclamo): string {
    switch (estado) {
      case EstadoReclamo.Abierto:
        return 'Abierto';
      case EstadoReclamo.EnProceso:
        return 'En Proceso';
      case EstadoReclamo.Resuelto:
        return 'Resuelto';
      case EstadoReclamo.Cerrado:
        return 'Cerrado';
      case EstadoReclamo.Rechazado:
        return 'Rechazado';
      case EstadoReclamo.Escalado:
        return 'Escalado';
      default:
        return 'Desconocido';
    }
  }

  getPrioridadText(prioridad: PrioridadReclamo): string {
    switch (prioridad) {
      case PrioridadReclamo.Baja:
        return 'Baja';
      case PrioridadReclamo.Media:
        return 'Media';
      case PrioridadReclamo.Alta:
        return 'Alta';
      case PrioridadReclamo.Critica:
        return 'Crítica';
      default:
        return 'Desconocida';
    }
  }

  exportarReclamos(): void {
    const dataToExport = this.dataSource.filteredData.length > 0
      ? this.dataSource.filteredData
      : this.reclamos;

    const dialogData: ExportDialogData = {
      title: 'Exportar Reclamos',
      totalRecords: dataToExport.length,
      defaultFilename: `reclamos_${this.getCurrentDateString()}`
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

  private performExport(data: Reclamo[], options: ExportDialogResult): void {
    const columns: ExportColumn[] = [
      { key: 'numeroReclamo', header: 'No. Reclamo', type: 'text' },
      { key: 'numeroPoliza', header: 'No. Póliza', type: 'text' },
      { key: 'clienteNombreCompleto', header: 'Cliente', type: 'text' },
      { key: 'tipoReclamo', header: 'Tipo', type: 'text' },
      { key: 'estado', header: 'Estado', type: 'text' },
      { key: 'prioridad', header: 'Prioridad', type: 'text' },
      { key: 'fechaReclamo', header: 'Fecha Reclamo', type: 'date', dateFormat: options.dateFormat },
      { key: 'fechaResolucion', header: 'Fecha Resolución', type: 'date', dateFormat: options.dateFormat },
      { key: 'montoReclamado', header: 'Monto Reclamado', type: 'currency', currencyCode: 'CRC' },
      { key: 'montoAprobado', header: 'Monto Aprobado', type: 'currency', currencyCode: 'CRC' },
      { key: 'usuarioAsignadoNombre', header: 'Asignado A', type: 'text' },
      { key: 'observaciones', header: 'Observaciones', type: 'text' }
    ];

    const transformedData = data.map(r => ({
      ...r,
      tipoReclamo: this.getTipoReclamoText(r.tipoReclamo),
      estado: this.getEstadoText(r.estado),
      prioridad: this.getPrioridadText(r.prioridad),
      fechaReclamo: r.fechaReclamo ? new Date(r.fechaReclamo) : null,
      fechaResolucion: r.fechaResolucion ? new Date(r.fechaResolucion) : null
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
    return `${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}`;
  }

  crearNuevoReclamo(): void {
    this.router.navigate(['/reclamos/crear']);
  }

  verDetalle(reclamo: Reclamo): void {
    this.router.navigate(['/reclamos/detalle', reclamo.id]);
  }

  private showMessage(message: string): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }
}