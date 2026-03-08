import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Cobro } from '../../interfaces/cobro.interface';

export interface CancelarCobroDialogData {
  cobro: Cobro;
}

@Component({
  selector: 'app-cancelar-cobro-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon color="warn" style="vertical-align:middle;margin-right:8px;">warning</mat-icon>
      Cancelar Cobro
    </h2>
    <mat-dialog-content>
      <p>¿Está seguro que desea cancelar el cobro de la póliza <strong>{{ data.cobro.numeroPoliza }}</strong>?</p>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Motivo de Cancelación</mat-label>
          <textarea matInput formControlName="motivo" rows="3" required
                    placeholder="Indique el motivo de cancelación..."></textarea>
          <mat-error *ngIf="form.get('motivo')?.hasError('required')">El motivo es requerido</mat-error>
          <mat-error *ngIf="form.get('motivo')?.hasError('minlength')">Mínimo 10 caracteres</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">No, mantener</button>
      <button mat-raised-button color="warn" (click)="onConfirm()" [disabled]="form.invalid">
        Sí, cancelar cobro
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width { width: 100%; }
  `]
})
export class CancelarCobroDialogComponent {
  form: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: MatDialogRef<CancelarCobroDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CancelarCobroDialogData
  ) {
    this.form = this.fb.group({
      motivo: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.form.invalid) return;
    this.dialogRef.close(this.form.value.motivo as string);
  }
}
