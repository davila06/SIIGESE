import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { 
  Cobro, 
  EstadoCobro, 
  MetodoPago,
  getEstadoCobroLabel,
  getMetodoPagoLabel
} from '../../interfaces/cobro.interface';
import { CURRENCY_CONSTANTS, formatCurrencyByCode } from '../../../shared/constants/currency.constants';

@Component({
  selector: 'app-cobro-detalle-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatIconModule,
    MatChipsModule
  ],
  templateUrl: './cobro-detalle-dialog.component.html',
  styleUrls: ['./cobro-detalle-dialog.component.scss']
})
export class CobroDetalleDialogComponent {
  EstadoCobro = EstadoCobro;
  MetodoPago = MetodoPago;

  constructor(
    public dialogRef: MatDialogRef<CobroDetalleDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public cobro: Cobro
  ) {}

  getEstadoCobroLabel = getEstadoCobroLabel;
  getMetodoPagoLabel = getMetodoPagoLabel;

  formatCurrency(amount: number, currencyCode: string): string {
    return formatCurrencyByCode(amount, currencyCode);
  }

  getEstadoColor(estado: any): string {
    // Convertir el estado a número si es necesario
    const estadoNum = typeof estado === 'string' ? parseInt(estado, 10) : Number(estado);
    
    switch (estadoNum) {
      case 0: // Pendiente
        return 'warn';
      case 1: // Cobrado
        return 'primary';
      case 2: // Vencido
        return 'accent';
      case 3: // Cancelado
        return '';
      default:
        return '';
    }
  }

  onCerrar(): void {
    this.dialogRef.close();
  }
}