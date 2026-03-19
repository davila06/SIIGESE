import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Poliza } from '../interfaces/user.interface';
import { formatCurrencyByCode, formatDateCR, parseBackendDate } from '../shared/constants/currency.constants';

export interface PolizaDetailDialogData {
  poliza: Poliza;
}

@Component({
  selector: 'app-poliza-detail-dialog',
  templateUrl: './poliza-detail-dialog.component.html',
  styleUrls: ['./poliza-detail-dialog.component.scss'],
  standalone: false
})
export class PolizaDetailDialogComponent {
  poliza: Poliza;

  constructor(
    public dialogRef: MatDialogRef<PolizaDetailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data: PolizaDetailDialogData
  ) {
    this.poliza = data.poliza;
  }

  get avatarLetter(): string {
    return this.poliza.nombreAsegurado?.charAt(0)?.toUpperCase() ?? '?';
  }

  get hasVehicle(): boolean {
    return !!(this.poliza.placa || this.poliza.marca || this.poliza.modelo || this.poliza.año);
  }

  get hasContact(): boolean {
    return !!(this.poliza.correo || this.poliza.numeroTelefono);
  }

  formatCurrency(amount: number, moneda: string): string {
    return formatCurrencyByCode(amount, moneda);
  }

  formatDate(date: string): string {
    const parsed = parseBackendDate(date);
    return parsed ? formatDateCR(parsed) : date ?? '—';
  }

  close(): void {
    this.dialogRef.close(null);
  }

  edit(): void {
    this.dialogRef.close('edit');
  }

  delete(): void {
    this.dialogRef.close('delete');
  }
}
