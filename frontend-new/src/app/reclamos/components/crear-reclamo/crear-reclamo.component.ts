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
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { Observable, of } from 'rxjs';
import { map, startWith, debounceTime, switchMap, catchError } from 'rxjs/operators';

import { ReclamosService } from '../../services/reclamos.service';
import { PolizasService } from '../../../polizas/services/polizas.service';
import { 
  CreateReclamoDto, 
  TipoReclamo, 
  PrioridadReclamo 
} from '../../interfaces/reclamo.interface';

interface PolizaOption {
  id: number;
  numeroPoliza: string;
  nombreAsegurado: string;
  prima: number;
  moneda: string;
}

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
    MatStepperModule,
    MatAutocompleteModule
  ],
  templateUrl: './crear-reclamo.component.html',
  styleUrls: ['./crear-reclamo.component.scss']
})
export class CrearReclamoComponent implements OnInit {
  reclamoForm: FormGroup;
  loading = false;
  
  polizasFiltradas$!: Observable<PolizaOption[]>;
  polizaSeleccionada: PolizaOption | null = null;
  
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
    private readonly fb: FormBuilder,
    private readonly reclamosService: ReclamosService,
    private readonly polizasService: PolizasService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar
  ) {
    this.reclamoForm = this.fb.group({
      polizaBusqueda: [''],
      polizaId: [null, [Validators.required]],
      numeroPoliza: ['', [Validators.required]],
      clienteNombreCompleto: ['', [Validators.required]],
      tipoReclamo: [null, [Validators.required]],
      descripcion: ['', [Validators.required, Validators.minLength(10)]],
      montoReclamado: [null],
      moneda: ['USD', [Validators.required]],
      prioridad: [PrioridadReclamo.Media, [Validators.required]]
    });
  }

  ngOnInit(): void {
    // Configurar autocomplete de pólizas
    this.polizasFiltradas$ = this.reclamoForm.get('polizaBusqueda')!.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      switchMap(value => {
        const searchTerm = typeof value === 'string' ? value : value?.numeroPoliza || '';
        if (!searchTerm || searchTerm.length < 2) {
          return of([]);
        }
        return this.buscarPolizas(searchTerm);
      })
    );
    
    // Asegurar que el formulario esté siempre limpio al iniciar
    this.resetForm();
  }
  
  buscarPolizas(termino: string): Observable<PolizaOption[]> {
    return this.polizasService.buscarPolizas(termino).pipe(
      map(polizas => polizas.map(p => ({
        id: p.id,
        numeroPoliza: p.numeroPoliza,
        nombreAsegurado: p.nombreAsegurado,
        prima: p.prima,
        moneda: p.moneda
      }))),
      catchError(error => {
        console.error('Error buscando pólizas:', error);
        return of([]);
      })
    );
  }
  
  displayPoliza(poliza: PolizaOption | null): string {
    return poliza ? `${poliza.numeroPoliza}` : '';
  }
  
  onPolizaSeleccionada(poliza: PolizaOption): void {
    this.polizaSeleccionada = poliza;
    this.reclamoForm.patchValue({
      polizaId: poliza.id,
      numeroPoliza: poliza.numeroPoliza,
      clienteNombreCompleto: poliza.nombreAsegurado,
      moneda: poliza.moneda
    });
  }
  
  resetForm(): void {
    this.reclamoForm.reset();
    this.polizaSeleccionada = null;
    
    // Restablecer valores por defecto
    this.reclamoForm.patchValue({
      moneda: 'USD',
      prioridad: PrioridadReclamo.Media
    });
    
    // Limpiar completamente los estados de validación
    Object.keys(this.reclamoForm.controls).forEach(key => {
      const control = this.reclamoForm.get(key);
      if (control) {
        control.markAsUntouched();
        control.markAsPristine();
        control.setErrors(null);
        control.updateValueAndValidity();
      }
    });
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