import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
import { 
  Cobro, 
  CobroStats, 
  EstadoCobro, 
  MetodoPago 
} from '../../interfaces/cobro.interface';
import { CobrosService } from '../../services/cobros.service';

@Component({
  selector: 'app-cobros-dashboard',
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
  filtroEstado = '';

  // Enums para el template
  EstadoCobro = EstadoCobro;
  MetodoPago = MetodoPago;

  constructor(
    private cobrosService: CobrosService,
    private snackBar: MatSnackBar
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
    
    // Por ahora usar datos mock hasta tener el backend
    setTimeout(() => {
      this.cobros = this.cobrosService.getMockCobros();
      this.dataSource.data = this.cobros;
      this.loading = false;
    }, 1000);

    /* Una vez que esté el backend:
    this.cobrosService.getCobros().subscribe({
      next: (cobros) => {
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
    */
  }

  loadStats(): void {
    // Por ahora usar datos mock
    setTimeout(() => {
      this.stats = this.cobrosService.getMockStats();
    }, 800);

    /* Una vez que esté el backend:
    this.cobrosService.getCobroStats().subscribe({
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
    if (estado === '') {
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
    // Aquí se abriría un diálogo con los detalles del cobro
    console.log('Ver detalle:', cobro);
    this.showMessage('Funcionalidad de detalles en desarrollo');
  }

  cancelarCobro(cobro: Cobro): void {
    // Aquí se confirmaría y cancelaría el cobro
    console.log('Cancelar cobro:', cobro);
    this.showMessage('Funcionalidad de cancelación en desarrollo');
  }

  getEstadoColor(estado: EstadoCobro): string {
    switch (estado) {
      case EstadoCobro.PENDIENTE:
        return 'primary';
      case EstadoCobro.COBRADO:
        return 'accent';
      case EstadoCobro.VENCIDO:
        return 'warn';
      case EstadoCobro.CANCELADO:
        return '';
      default:
        return '';
    }
  }

  getEstadoIcon(estado: EstadoCobro): string {
    switch (estado) {
      case EstadoCobro.PENDIENTE:
        return 'schedule';
      case EstadoCobro.COBRADO:
        return 'check_circle';
      case EstadoCobro.VENCIDO:
        return 'warning';
      case EstadoCobro.CANCELADO:
        return 'cancel';
      default:
        return 'help';
    }
  }

  exportarCobros(): void {
    this.showMessage('Funcionalidad de exportación en desarrollo');
  }

  private showMessage(message: string): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }
}