import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { Observable, of } from 'rxjs';
import { map, startWith, debounceTime, switchMap, catchError } from 'rxjs/operators';
import { PolizasService } from '../../../polizas/services/polizas.service';
import { CobroRequest, MetodoPago } from '../../interfaces/cobro.interface';
import { MONEDAS_SISTEMA } from '../../../shared/constants/currency.constants';

interface PolizaOption {
  id: number;
  numeroPoliza: string;
  nombreAsegurado: string;
  prima: number;
  moneda: string;
}

@Component({
  selector: 'app-agregar-cobro-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule,
    MatAutocompleteModule
  ],
  templateUrl: './agregar-cobro-dialog.component.html',
  styleUrls: ['./agregar-cobro-dialog.component.scss']
})
export class AgregarCobroDialogComponent implements OnInit {
  cobroForm: FormGroup;
  monedas = MONEDAS_SISTEMA;
  metodosPago = [
    { value: MetodoPago.Efectivo, label: 'Efectivo' },
    { value: MetodoPago.Transferencia, label: 'Transferencia' },
    { value: MetodoPago.Cheque, label: 'Cheque' },
    { value: MetodoPago.TarjetaCredito, label: 'Tarjeta de Crédito' },
    { value: MetodoPago.TarjetaDebito, label: 'Tarjeta de Débito' }
  ];

  polizasFiltradas$!: Observable<PolizaOption[]>;
  polizaSeleccionada: PolizaOption | null = null;
  
  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AgregarCobroDialogComponent>,
    private polizasService: PolizasService
  ) {
    this.cobroForm = this.fb.group({
      polizaBusqueda: ['', Validators.required],
      polizaId: [null, Validators.required],
      numeroPoliza: ['', Validators.required],
      clienteNombreCompleto: ['', Validators.required],
      correoElectronico: ['', [Validators.email]],
      fechaVencimiento: [null, Validators.required],
      montoTotal: [null, [Validators.required, Validators.min(0)]],
      moneda: ['CRC', Validators.required],
      observaciones: ['']
    });
  }

  ngOnInit(): void {
    // Configurar autocomplete para buscar pólizas
    this.polizasFiltradas$ = this.cobroForm.get('polizaBusqueda')!.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      switchMap(value => {
        const searchTerm = typeof value === 'string' ? value : value?.numeroPoliza || '';
        if (searchTerm.length < 2) {
          return of([]);
        }
        return this.buscarPolizas(searchTerm);
      })
    );
  }

  buscarPolizas(termino: string): Observable<PolizaOption[]> {
    return this.polizasService.getAll().pipe(
      map(polizas => polizas
        .filter(p => 
          p.numeroPoliza.toLowerCase().includes(termino.toLowerCase()) ||
          p.nombreAsegurado.toLowerCase().includes(termino.toLowerCase())
        )
        .slice(0, 10)
        .map(p => ({
          id: p.id,
          numeroPoliza: p.numeroPoliza,
          nombreAsegurado: p.nombreAsegurado,
          prima: p.prima,
          moneda: p.moneda
        }))
      ),
      catchError(() => of([]))
    );
  }

  displayPoliza(poliza: PolizaOption | null): string {
    return poliza ? `${poliza.numeroPoliza} - ${poliza.nombreAsegurado}` : '';
  }

  onPolizaSeleccionada(poliza: PolizaOption): void {
    this.polizaSeleccionada = poliza;
    this.cobroForm.patchValue({
      polizaId: poliza.id,
      numeroPoliza: poliza.numeroPoliza,
      clienteNombreCompleto: poliza.nombreAsegurado,
      montoTotal: poliza.prima,
      moneda: poliza.moneda || 'CRC'
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.cobroForm.valid) {
      const formValue = this.cobroForm.value;
      
      // Crear objeto CobroRequest con los campos correctos de la BD
      const cobroRequest: CobroRequest = {
        polizaId: formValue.polizaId,
        fechaVencimiento: formValue.fechaVencimiento,
        montoTotal: formValue.montoTotal,
        moneda: formValue.moneda,
        observaciones: formValue.observaciones || ''
      };

      this.dialogRef.close(cobroRequest);
    } else {
      // Marcar todos los campos como touched para mostrar errores
      Object.keys(this.cobroForm.controls).forEach(key => {
        this.cobroForm.get(key)?.markAsTouched();
      });
    }
  }

  // Helpers para mostrar errores
  getErrorMessage(fieldName: string): string {
    const control = this.cobroForm.get(fieldName);
    if (control?.hasError('required')) {
      return 'Este campo es requerido';
    }
    if (control?.hasError('min')) {
      return 'El valor debe ser mayor o igual a 0';
    }
    return '';
  }
}
