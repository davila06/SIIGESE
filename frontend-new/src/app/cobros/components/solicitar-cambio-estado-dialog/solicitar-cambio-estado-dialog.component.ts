import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Cobro } from '../../interfaces/cobro.interface';

export interface SolicitarCambioEstadoDialogData {
  cobro: Cobro | null;
  estadoActualLabel: string;
  estadoSolicitadoLabel: string;
  title?: string;
  description?: string;
  submitLabel?: string;
}

@Component({
  selector: 'app-solicitar-cambio-estado-dialog',
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
    <h2 mat-dialog-title>{{ data.title || 'Solicitar cambio de estado' }}</h2>

    <mat-dialog-content>
      <p *ngIf="!data.description">
        Se enviará una solicitud a un administrador para cambiar el estado
        <strong>{{ data.estadoActualLabel }}</strong> → <strong>{{ data.estadoSolicitadoLabel }}</strong>.
      </p>
      <p *ngIf="data.description">{{ data.description }}</p>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Motivo</mat-label>
        <textarea
          matInput
          rows="5"
          maxlength="1000"
          formControlName="motivo"
          [placeholder]="'Describe el contexto de la solicitud o decisión...'"></textarea>
        <mat-hint align="end">{{ motivoLength }}/1000</mat-hint>
      </mat-form-field>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="cancel()">Cancelar</button>
      <button mat-raised-button color="primary" [disabled]="form.invalid" (click)="submit()">
        <mat-icon>send</mat-icon>
        {{ data.submitLabel || 'Enviar solicitud' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `.full-width { width: 100%; }`
  ]
})
export class SolicitarCambioEstadoDialogComponent {
  form: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: MatDialogRef<SolicitarCambioEstadoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SolicitarCambioEstadoDialogData
  ) {
    this.form = this.fb.group({
      motivo: ['', [Validators.required, Validators.maxLength(1000)]]
    });
  }

  get motivoLength(): number {
    return (this.form.get('motivo')?.value || '').length;
  }

  cancel(): void {
    this.dialogRef.close(undefined);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.dialogRef.close((this.form.get('motivo')?.value || '').trim());
  }
}
