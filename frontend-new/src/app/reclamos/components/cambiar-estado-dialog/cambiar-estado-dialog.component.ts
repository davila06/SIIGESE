import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { EstadoReclamo } from '../../interfaces/reclamo.interface';

export interface CambiarEstadoData {
  numeroReclamo: string;
  estadoActual: EstadoReclamo;
}

export interface CambiarEstadoResult {
  nuevoEstado: EstadoReclamo;
  comentario: string;
}

@Component({
  selector: 'app-cambiar-estado-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule
  ],
  template: `
    <div class="dialog-container">
      <h2 mat-dialog-title>Cambiar Estado del Reclamo</h2>
      
      <mat-dialog-content>
        <div class="form-container">
          <p class="reclamo-info">
            <strong>Reclamo:</strong> {{ data.numeroReclamo }}<br>
            <strong>Estado actual:</strong> {{ getEstadoLabel(data.estadoActual) }}
          </p>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Nuevo Estado</mat-label>
            <mat-select [(value)]="nuevoEstado" required>
              <mat-option 
                *ngFor="let estado of estadosDisponibles" 
                [value]="estado.value"
                [disabled]="estado.value === data.estadoActual">
                {{ estado.label }}
                <span *ngIf="estado.value === data.estadoActual" class="current-state">(actual)</span>
              </mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Comentario (opcional)</mat-label>
            <textarea 
              matInput 
              [(ngModel)]="comentario" 
              rows="3" 
              placeholder="Agregue un comentario sobre el cambio de estado...">
            </textarea>
          </mat-form-field>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">Cancelar</button>
        <button 
          mat-raised-button 
          color="primary" 
          (click)="onConfirm()"
          [disabled]="!nuevoEstado || nuevoEstado === data.estadoActual">
          Cambiar Estado
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .dialog-container {
      min-width: 400px;
      max-width: 500px;
    }

    .form-container {
      padding: 16px 0;
    }

    .reclamo-info {
      background-color: #f5f5f5;
      padding: 12px;
      border-radius: 4px;
      margin-bottom: 16px;
      border-left: 4px solid #2196F3;
    }

    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    .current-state {
      color: #666;
      font-style: italic;
    }

    mat-dialog-actions {
      padding: 8px 0;
      margin: 0;
    }

    mat-dialog-content {
      max-height: 60vh;
      overflow-y: auto;
    }
  `]
})
export class CambiarEstadoDialogComponent {
  nuevoEstado: EstadoReclamo | null = null;
  comentario: string = '';

  estadosDisponibles = [
    { value: EstadoReclamo.Abierto, label: 'Abierto' },
    { value: EstadoReclamo.EnProceso, label: 'En Proceso' },
    { value: EstadoReclamo.Resuelto, label: 'Resuelto' },
    { value: EstadoReclamo.Cerrado, label: 'Cerrado' },
    { value: EstadoReclamo.Rechazado, label: 'Rechazado' },
    { value: EstadoReclamo.Escalado, label: 'Escalado' }
  ];

  constructor(
    public dialogRef: MatDialogRef<CambiarEstadoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CambiarEstadoData
  ) {}

  getEstadoLabel(estado: EstadoReclamo): string {
    const estadoInfo = this.estadosDisponibles.find(e => e.value === estado);
    return estadoInfo?.label || 'Desconocido';
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.nuevoEstado && this.nuevoEstado !== this.data.estadoActual) {
      const result: CambiarEstadoResult = {
        nuevoEstado: this.nuevoEstado,
        comentario: this.comentario.trim()
      };
      this.dialogRef.close(result);
    }
  }
}