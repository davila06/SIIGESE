import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { Cobro, MetodoPago, RegistrarCobroRequest } from '../../interfaces/cobro.interface';

export interface RegistrarCobroDialogData {
  cobro: Cobro;
}

@Component({
  selector: 'app-registrar-cobro-dialog',
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
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>Registrar Cobro</h2>
    <mat-dialog-content>
      <p class="cobro-info">
        <strong>Póliza:</strong> {{ data.cobro.numeroPoliza }} &nbsp;|&nbsp;
        <strong>Cliente:</strong> {{ data.cobro.clienteNombreCompleto }}
      </p>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Fecha de Cobro</mat-label>
          <input matInput [matDatepicker]="picker" formControlName="fechaCobro" required>
          <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
          <mat-datepicker #picker></mat-datepicker>
          <mat-error *ngIf="form.get('fechaCobro')?.hasError('required')">La fecha es requerida</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Monto Cobrado</mat-label>
          <input matInput type="number" formControlName="montoCobrado" required min="0.01" step="0.01">
          <mat-error *ngIf="form.get('montoCobrado')?.hasError('required')">El monto es requerido</mat-error>
          <mat-error *ngIf="form.get('montoCobrado')?.hasError('min')">El monto debe ser mayor a 0</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Método de Pago</mat-label>
          <mat-select formControlName="metodoPago" required>
            <mat-option *ngFor="let m of metodosPago" [value]="m.value">{{ m.label }}</mat-option>
          </mat-select>
          <mat-error *ngIf="form.get('metodoPago')?.hasError('required')">Seleccione un método de pago</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Observaciones</mat-label>
          <textarea matInput formControlName="observaciones" rows="3"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancelar</button>
      <button mat-raised-button color="primary" (click)="onConfirm()" [disabled]="form.invalid">
        Registrar Cobro
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width { width: 100%; margin-bottom: 8px; }
    .cobro-info { margin-bottom: 16px; color: rgba(0,0,0,.6); }
  `]
})
export class RegistrarCobroDialogComponent implements OnInit {
  form!: FormGroup;

  metodosPago = [
    { value: MetodoPago.Efectivo, label: 'Efectivo' },
    { value: MetodoPago.Transferencia, label: 'Transferencia' },
    { value: MetodoPago.Cheque, label: 'Cheque' },
    { value: MetodoPago.TarjetaCredito, label: 'Tarjeta de Crédito' },
    { value: MetodoPago.TarjetaDebito, label: 'Tarjeta de Débito' }
  ];

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: MatDialogRef<RegistrarCobroDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: RegistrarCobroDialogData
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      fechaCobro: [new Date(), Validators.required],
      montoCobrado: [this.data.cobro.montoTotal, [Validators.required, Validators.min(0.01)]],
      metodoPago: [null, Validators.required],
      observaciones: ['']
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.form.invalid) return;

    const request: RegistrarCobroRequest = {
      cobroId: this.data.cobro.id,
      fechaCobro: this.form.value.fechaCobro,
      montoCobrado: this.form.value.montoCobrado,
      metodoPago: this.form.value.metodoPago,
      observaciones: this.form.value.observaciones || undefined
    };

    this.dialogRef.close(request);
  }
}
