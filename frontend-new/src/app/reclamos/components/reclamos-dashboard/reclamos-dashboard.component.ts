import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
import { 
  Reclamo,
  ReclamosStats,
  TipoReclamo,
  EstadoReclamo,
  PrioridadReclamo,
  FiltroReclamos 
} from '../../interfaces/reclamo.interface';
import { ReclamosService } from '../../services/reclamos.service';

@Component({
  selector: 'app-reclamos-dashboard',
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
  filtroEstado = '';
  filtroTipo = '';
  filtroPrioridad = '';

  // Enums para el template
  TipoReclamo = TipoReclamo;
  EstadoReclamo = EstadoReclamo;
  PrioridadReclamo = PrioridadReclamo;

  constructor(
    private reclamosService: ReclamosService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadReclamos();
    this.loadStats();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadReclamos(): void {
    this.loading = true;
    
    // Por ahora usar datos mock hasta tener el backend
    setTimeout(() => {
      this.reclamos = this.reclamosService.getMockReclamos();
      this.dataSource.data = this.reclamos;
      this.loading = false;
    }, 1000);

    /* Una vez que esté el backend:
    this.reclamosService.getReclamos().subscribe({
      next: (reclamos) => {
        this.reclamos = reclamos;
        this.dataSource.data = reclamos;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar reclamos:', error);
        this.showMessage('Error al cargar los reclamos');
        this.loading = false;
      }
    });
    */
  }

  loadStats(): void {
    // Por ahora usar datos mock
    setTimeout(() => {
      this.stats = this.reclamosService.getMockStats();
    }, 800);

    /* Una vez que esté el backend:
    this.reclamosService.getReclamosStats().subscribe({
      next: (stats) => {
        this.stats = stats;
      },
      error: (error) => {
        console.error('Error al cargar estadísticas:', error);
      }
    });
    */
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  filtrarPorEstado(estado: string): void {
    this.filtroEstado = estado;
    this.aplicarFiltros();
  }

  filtrarPorTipo(tipo: string): void {
    this.filtroTipo = tipo;
    this.aplicarFiltros();
  }

  filtrarPorPrioridad(prioridad: string): void {
    this.filtroPrioridad = prioridad;
    this.aplicarFiltros();
  }

  private aplicarFiltros(): void {
    let reclamosFiltrados = [...this.reclamos];

    if (this.filtroEstado) {
      reclamosFiltrados = reclamosFiltrados.filter(r => r.estado === this.filtroEstado);
    }

    if (this.filtroTipo) {
      reclamosFiltrados = reclamosFiltrados.filter(r => r.tipoReclamo === this.filtroTipo);
    }

    if (this.filtroPrioridad) {
      reclamosFiltrados = reclamosFiltrados.filter(r => r.prioridad === this.filtroPrioridad);
    }

    this.dataSource.data = reclamosFiltrados;
  }

  limpiarFiltros(): void {
    this.filtroEstado = '';
    this.filtroTipo = '';
    this.filtroPrioridad = '';
    this.dataSource.data = this.reclamos;
  }

  verDetalle(reclamo: Reclamo): void {
    console.log('Ver detalle:', reclamo);
    this.showMessage('Funcionalidad de detalles en desarrollo');
  }

  asignarReclamo(reclamo: Reclamo): void {
    console.log('Asignar reclamo:', reclamo);
    this.showMessage('Funcionalidad de asignación en desarrollo');
  }

  cambiarEstado(reclamo: Reclamo): void {
    console.log('Cambiar estado:', reclamo);
    this.showMessage('Funcionalidad de cambio de estado en desarrollo');
  }

  subirDocumento(reclamo: Reclamo): void {
    console.log('Subir documento:', reclamo);
    this.showMessage('Funcionalidad de carga de documentos en desarrollo');
  }

  getEstadoColor(estado: EstadoReclamo): string {
    switch (estado) {
      case EstadoReclamo.PENDIENTE:
        return 'primary';
      case EstadoReclamo.EN_REVISION:
        return 'accent';
      case EstadoReclamo.REQUIERE_DOCUMENTOS:
        return 'warn';
      case EstadoReclamo.APROBADO:
        return 'primary';
      case EstadoReclamo.RECHAZADO:
        return 'warn';
      case EstadoReclamo.RESUELTO:
        return 'accent';
      case EstadoReclamo.CERRADO:
        return '';
      default:
        return '';
    }
  }

  getEstadoIcon(estado: EstadoReclamo): string {
    switch (estado) {
      case EstadoReclamo.PENDIENTE:
        return 'schedule';
      case EstadoReclamo.EN_REVISION:
        return 'rate_review';
      case EstadoReclamo.REQUIERE_DOCUMENTOS:
        return 'description';
      case EstadoReclamo.APROBADO:
        return 'check_circle';
      case EstadoReclamo.RECHAZADO:
        return 'cancel';
      case EstadoReclamo.RESUELTO:
        return 'task_alt';
      case EstadoReclamo.CERRADO:
        return 'lock';
      default:
        return 'help';
    }
  }

  getPrioridadColor(prioridad: PrioridadReclamo): string {
    switch (prioridad) {
      case PrioridadReclamo.BAJA:
        return '#4CAF50';
      case PrioridadReclamo.MEDIA:
        return '#FF9800';
      case PrioridadReclamo.ALTA:
        return '#F44336';
      case PrioridadReclamo.URGENTE:
        return '#9C27B0';
      default:
        return '#757575';
    }
  }

  getTipoIcon(tipo: TipoReclamo): string {
    switch (tipo) {
      case TipoReclamo.SINIESTRO:
        return 'car_crash';
      case TipoReclamo.REEMBOLSO:
        return 'money';
      case TipoReclamo.QUEJA_SERVICIO:
        return 'feedback';
      case TipoReclamo.CANCELACION:
        return 'cancel_presentation';
      case TipoReclamo.CAMBIO_POLIZA:
        return 'edit';
      case TipoReclamo.OTRO:
        return 'help_outline';
      default:
        return 'assignment';
    }
  }

  exportarReclamos(): void {
    this.showMessage('Funcionalidad de exportación en desarrollo');
  }

  crearNuevoReclamo(): void {
    this.showMessage('Funcionalidad de creación de reclamo en desarrollo');
  }

  private showMessage(message: string): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }
}