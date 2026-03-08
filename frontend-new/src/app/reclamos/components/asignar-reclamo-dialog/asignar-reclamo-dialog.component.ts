import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';

import { ApiService } from '../../../services/api.service';
import { Reclamo } from '../../interfaces/reclamo.interface';

export interface AsignarReclamoDialogData {
  reclamo: Reclamo;
}

export interface AsignarReclamoDialogResult {
  usuarioId: number;
  usuarioNombre: string;
}

interface Usuario {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  roles: { id: number; name: string; }[];
  isActive: boolean;
}

@Component({
  selector: 'app-asignar-reclamo-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatListModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule
  ],
  template: `
    <div class="dialog-container">
      <h2 mat-dialog-title>
        <mat-icon>person_add</mat-icon>
        Asignar Reclamo
      </h2>
      
      <mat-dialog-content>
        <div class="reclamo-info">
          <h3>Reclamo: {{ data.reclamo.numeroReclamo }}</h3>
          <p>{{ data.reclamo.descripcion }}</p>
        </div>
        
        <div class="usuarios-section" *ngIf="!loading">
          <h4>Seleccionar Usuario:</h4>
          <mat-list>
            <mat-list-item 
              *ngFor="let usuario of usuariosDisponibles"
              (click)="selectUsuario(usuario)"
              class="usuario-item"
              [class.selected]="selectedUsuario?.id === usuario.id">
              
              <div matListItemIcon class="usuario-avatar">
                <span>{{ usuario.firstName.charAt(0) }}{{ usuario.lastName.charAt(0) }}</span>
              </div>
              
              <div matListItemTitle>{{ usuario.firstName }} {{ usuario.lastName }}</div>
              <div matListItemLine>{{ usuario.email }}</div>
              
              <mat-icon matListItemMeta *ngIf="selectedUsuario?.id === usuario.id">
                check_circle
              </mat-icon>
            </mat-list-item>
          </mat-list>
        </div>
        
        <div class="loading-container" *ngIf="loading">
          <mat-spinner diameter="40"></mat-spinner>
          <p>Cargando usuarios...</p>
        </div>
        
        <div class="error-container" *ngIf="error">
          <mat-icon color="warn">error</mat-icon>
          <p>{{ error }}</p>
        </div>
      </mat-dialog-content>
      
      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">Cancelar</button>
        <button 
          mat-raised-button 
          color="primary" 
          [disabled]="!selectedUsuario"
          (click)="onConfirm()">
          <mat-icon>assignment_ind</mat-icon>
          Asignar
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .dialog-container {
      min-width: 400px;
      max-width: 600px;
    }
    
    .reclamo-info {
      background: #f5f5f5;
      padding: 16px;
      border-radius: 8px;
      margin-bottom: 20px;
    }
    
    .reclamo-info h3 {
      margin: 0 0 8px 0;
      color: #1976d2;
    }
    
    .reclamo-info p {
      margin: 0;
      color: #666;
    }
    
    .usuarios-section h4 {
      margin: 0 0 16px 0;
      color: #333;
    }
    
    .usuario-item {
      cursor: pointer;
      border-radius: 8px;
      margin-bottom: 8px;
      transition: background-color 0.2s;
    }
    
    .usuario-item:hover {
      background-color: #f0f0f0;
    }
    
    .usuario-item.selected {
      background-color: #e3f2fd;
      border: 2px solid #1976d2;
    }
    
    .usuario-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: #1976d2;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-weight: 600;
      font-size: 14px;
    }
    
    .loading-container,
    .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 32px;
    }
    
    .error-container mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
    }
  `]
})
export class AsignarReclamoDialogComponent implements OnInit {
  usuariosDisponibles: Usuario[] = [];
  selectedUsuario: Usuario | null = null;
  loading = true;
  error: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<AsignarReclamoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AsignarReclamoDialogData,
    private apiService: ApiService
  ) {}

  ngOnInit(): void {
    this.loadUsuarios();
  }

  loadUsuarios(): void {
    this.loading = true;
    this.error = null;
    
    this.apiService.getUsers().subscribe({
      next: (usuarios) => {
        // Filtrar solo usuarios activos
        this.usuariosDisponibles = usuarios.filter(u => u.isActive);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error cargando usuarios:', error);
        this.error = 'Error al cargar los usuarios disponibles';
        this.loading = false;
      }
    });
  }

  selectUsuario(usuario: Usuario): void {
    this.selectedUsuario = usuario;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.selectedUsuario) {
      const result: AsignarReclamoDialogResult = {
        usuarioId: this.selectedUsuario.id,
        usuarioNombre: `${this.selectedUsuario.firstName} ${this.selectedUsuario.lastName}`
      };
      this.dialogRef.close(result);
    }
  }
}