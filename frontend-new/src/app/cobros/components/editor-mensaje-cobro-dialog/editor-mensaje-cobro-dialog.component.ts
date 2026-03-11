import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { EmailConfigService } from '../../../configuracion/services/email-config.service';

@Component({
  selector: 'app-editor-mensaje-cobro-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './editor-mensaje-cobro-dialog.component.html',
  styleUrls: ['./editor-mensaje-cobro-dialog.component.scss']
})
export class EditorMensajeCobroDialogComponent implements OnInit {
  form: FormGroup;
  loading = true;
  saving = false;
  defaultSubject = '';
  defaultBody = '';

  readonly variables = [
    { name: '{ClienteNombre}', label: 'Nombre del cliente' },
    { name: '{NumeroPoliza}',  label: 'Número de póliza' },
    { name: '{MontoVencido}',  label: 'Monto vencido' },
    { name: '{FechaVencimiento}', label: 'Fecha de vencimiento' },
    { name: '{DiasMora}',      label: 'Días en mora' },
  ];

  constructor(
    private readonly fb: FormBuilder,
    private readonly dialogRef: MatDialogRef<EditorMensajeCobroDialogComponent>,
    private readonly emailConfigService: EmailConfigService,
    private readonly snackBar: MatSnackBar
  ) {
    this.form = this.fb.group({
      subject: [''],
      body: ['']
    });
  }

  ngOnInit(): void {
    this.emailConfigService.getCobroTemplate().subscribe({
      next: (res) => {
        this.defaultSubject = res.data?.defaultSubject ?? '';
        this.defaultBody = res.data?.defaultBody ?? '';
        this.form.patchValue({
          subject: res.data?.subject ?? '',
          body: res.data?.body ?? ''
        });
        this.loading = false;
      },
      error: () => {
        this.snackBar.open('Error al cargar la plantilla', 'Cerrar', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  insertVariable(variable: string): void {
    const bodyControl = this.form.get('body');
    if (!bodyControl) return;
    const current = bodyControl.value ?? '';
    bodyControl.setValue(current + variable);
  }

  resetToDefault(): void {
    this.form.patchValue({
      subject: '',
      body: ''
    });
    this.snackBar.open('Se usará la plantilla predeterminada al guardar', 'OK', { duration: 3000 });
  }

  save(): void {
    this.saving = true;
    const { subject, body } = this.form.value;
    this.emailConfigService.updateCobroTemplate(
      subject?.trim() || null,
      body?.trim() || null
    ).subscribe({
      next: () => {
        this.snackBar.open('Plantilla guardada exitosamente', 'Cerrar', { duration: 3000 });
        this.saving = false;
        this.dialogRef.close(true);
      },
      error: () => {
        this.snackBar.open('Error al guardar la plantilla', 'Cerrar', { duration: 3000 });
        this.saving = false;
      }
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
