import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';

export interface ExportDialogData {
  title: string;
  totalRecords: number;
  availableFormats?: ExportFormat[];
  defaultFormat?: string;
  defaultFilename?: string;
}

export interface ExportFormat {
  value: string;
  label: string;
  icon: string;
  description: string;
}

export interface ExportDialogResult {
  format: string;
  filename: string;
  includeHeaders: boolean;
  dateFormat: string;
}

@Component({
  selector: 'app-export-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatIconModule,
    MatDividerModule
  ],
  templateUrl: './export-dialog.component.html',
  styleUrls: ['./export-dialog.component.scss']
})
export class ExportDialogComponent {
  exportForm: FormGroup;
  
  defaultFormats: ExportFormat[] = [
    {
      value: 'csv',
      label: 'CSV',
      icon: 'table_view',
      description: 'Archivo de valores separados por comas, compatible con Excel'
    },
    {
      value: 'excel',
      label: 'Excel',
      icon: 'grid_on',
      description: 'Archivo de Excel (.xlsx) con formato mejorado'
    },
    {
      value: 'pdf',
      label: 'PDF (Texto)',
      icon: 'picture_as_pdf',
      description: 'Documento PDF con formato de texto plano'
    }
  ];

  dateFormats = [
    { value: 'dd/MM/yyyy', label: 'DD/MM/AAAA' },
    { value: 'MM/dd/yyyy', label: 'MM/DD/AAAA' },
    { value: 'yyyy-MM-dd', label: 'AAAA-MM-DD' },
    { value: 'dd/MM/yyyy HH:mm', label: 'DD/MM/AAAA HH:MM' }
  ];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ExportDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ExportDialogData
  ) {
    this.exportForm = this.fb.group({
      format: [data.defaultFormat || 'csv', [Validators.required]],
      filename: [data.defaultFilename || this.generateDefaultFilename(), [Validators.required]],
      includeHeaders: [true],
      dateFormat: ['dd/MM/yyyy', [Validators.required]]
    });

    // Update filename when format changes
    this.exportForm.get('format')?.valueChanges.subscribe(format => {
      const currentFilename = this.exportForm.get('filename')?.value || '';
      const nameWithoutExtension = currentFilename.replace(/\.[^/.]+$/, '');
      const extension = this.getFileExtension(format);
      this.exportForm.patchValue({
        filename: `${nameWithoutExtension}.${extension}`
      });
    });
  }

  get availableFormats(): ExportFormat[] {
    return this.data.availableFormats || this.defaultFormats;
  }

  private generateDefaultFilename(): string {
    const now = new Date();
    const dateString = now.toISOString().split('T')[0].replace(/-/g, '');
    const timeString = now.toTimeString().split(' ')[0].replace(/:/g, '');
    return `export_${dateString}_${timeString}`;
  }

  private getFileExtension(format: string): string {
    switch (format) {
      case 'csv': return 'csv';
      case 'excel': return 'xlsx';
      case 'pdf': return 'txt';
      default: return 'csv';
    }
  }

  getSelectedFormat(): ExportFormat | undefined {
    const selectedValue = this.exportForm.get('format')?.value;
    return this.availableFormats.find(f => f.value === selectedValue);
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onExport(): void {
    if (this.exportForm.valid) {
      const result: ExportDialogResult = this.exportForm.value;
      this.dialogRef.close(result);
    }
  }
}