import { Component, OnInit } from '@angular/core';
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

import { ReclamosService } from '../../services/reclamos.service';
import { 
  Reclamo, 
  TipoReclamo, 
  EstadoReclamo, 
  PrioridadReclamo,
  getTipoReclamoLabel,
  getEstadoReclamoLabel,
  getPrioridadReclamoLabel
} from '../../interfaces/reclamo.interface';

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
    MatListModule
  ],
  templateUrl: './reclamo-detalle.component.html',
  styleUrls: ['./reclamo-detalle.component.scss']
})
export class ReclamoDetalleComponent implements OnInit {
  reclamo: Reclamo | null = null;
  loading = true;
  reclamoId: number = 0;

  // Enums para el template
  TipoReclamo = TipoReclamo;
  EstadoReclamo = EstadoReclamo;
  PrioridadReclamo = PrioridadReclamo;

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
        console.error('Error cargando reclamo:', error);
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