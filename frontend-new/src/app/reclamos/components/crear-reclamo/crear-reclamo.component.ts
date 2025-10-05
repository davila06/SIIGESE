import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';

import { ReclamosService } from '../../services/reclamos.service';
import { 
  CreateReclamoDto, 
  TipoReclamo, 
  PrioridadReclamo 
} from '../../interfaces/reclamo.interface';

@Component({
  selector: 'app-crear-reclamo',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
    MatIconModule,
    MatStepperModule
  ],
  templateUrl: './crear-reclamo.component.html',
  styleUrls: ['./crear-reclamo.component.scss']
})
export class CrearReclamoComponent implements OnInit {
  reclamoForm: FormGroup;
  loading = false;
  
  // Enums para el template
  TipoReclamo = TipoReclamo;
  PrioridadReclamo = PrioridadReclamo;
  
  tiposReclamo = [
    { value: TipoReclamo.Siniestro, label: 'Siniestro' },
    { value: TipoReclamo.Servicio, label: 'Servicio' },
    { value: TipoReclamo.Facturacion, label: 'Facturación' },
    { value: TipoReclamo.Cobertura, label: 'Cobertura' },
    { value: TipoReclamo.Proceso, label: 'Proceso' },
    { value: TipoReclamo.Otro, label: 'Otro' }
  ];
  
  prioridades = [
    { value: PrioridadReclamo.Baja, label: 'Baja' },
    { value: PrioridadReclamo.Media, label: 'Media' },
    { value: PrioridadReclamo.Alta, label: 'Alta' },
    { value: PrioridadReclamo.Critica, label: 'Crítica' }
  ];

  constructor(
    private fb: FormBuilder,
    private reclamosService: ReclamosService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.reclamoForm = this.fb.group({
      polizaId: [null, [Validators.required]],
      numeroPoliza: ['', [Validators.required]],
      clienteNombre: ['', [Validators.required]],
      clienteApellido: ['', [Validators.required]],
      tipoReclamo: [null, [Validators.required]],
      descripcion: ['', [Validators.required, Validators.minLength(10)]],
      montoReclamado: [null],
      moneda: ['USD', [Validators.required]],
      prioridad: [PrioridadReclamo.Media, [Validators.required]]
    });
  }

  ngOnInit(): void {
    // Inicialización del componente
  }

  onSubmit(): void {
    if (this.reclamoForm.valid) {
      this.loading = true;
      
      const reclamoData: CreateReclamoDto = {
        ...this.reclamoForm.value
      };

      this.reclamosService.createReclamo(reclamoData).subscribe({
        next: (response) => {
          this.loading = false;
          this.showMessage('Reclamo creado exitosamente');
          this.router.navigate(['/reclamos']);
        },
        error: (error) => {
          this.loading = false;
          console.error('Error al crear reclamo:', error);
          this.showMessage('Error al crear el reclamo. Por favor, intente nuevamente.');
        }
      });
    } else {
      this.markFormGroupTouched();
      this.showMessage('Por favor, complete todos los campos requeridos');
    }
  }

  onCancel(): void {
    this.router.navigate(['/reclamos']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.reclamoForm.controls).forEach(key => {
      const control = this.reclamoForm.get(key);
      control?.markAsTouched();
    });
  }

  private showMessage(message: string): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }

  getErrorMessage(fieldName: string): string {
    const control = this.reclamoForm.get(fieldName);
    if (control?.hasError('required')) {
      return `${fieldName} es requerido`;
    }
    if (control?.hasError('minlength')) {
      return `${fieldName} debe tener al menos ${control.errors?.['minlength'].requiredLength} caracteres`;
    }
    return '';
  }
}