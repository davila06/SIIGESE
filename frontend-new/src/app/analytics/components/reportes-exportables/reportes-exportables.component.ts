import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';

import { AnalyticsService } from '../../services/analytics.service';

@Component({
  selector: 'app-reportes-exportables',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatSelectModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './reportes-exportables.component.html',
  styleUrls: ['./reportes-exportables.component.scss']
})
export class ReportesExportablesComponent {
  aseguradora = '';
  recipientsCsv = '';
  statusMessage = '';
  busy: Record<string, boolean> = {};

  readonly aseguradoras = ['INS', 'SAGICOR', 'ASSA', 'BCR Seguros', 'MAPFRE', 'OTROS'];

  constructor(private analyticsService: AnalyticsService) {}

  downloadCarteraExcel(): void {
    this.executeAction('carteraExcel', this.analyticsService.getCarteraAseguradoraExcel(this.aseguradora || undefined), blob => {
      this.downloadBlob(blob, `cartera_${this.safeSuffix()}.xlsx`);
      this.statusMessage = 'Reporte de cartera en Excel generado correctamente.';
    }, () => {
      this.statusMessage = 'No se pudo generar el Excel de cartera.';
    });
  }

  downloadCarteraPdf(): void {
    this.executeAction('carteraPdf', this.analyticsService.getCarteraAseguradoraPdf(this.aseguradora || undefined), blob => {
      this.downloadBlob(blob, `cartera_${this.safeSuffix()}.pdf`);
      this.statusMessage = 'Reporte de cartera en PDF generado correctamente.';
    }, () => {
      this.statusMessage = 'No se pudo generar el PDF de cartera.';
    });
  }

  downloadMorosidadExcel(): void {
    this.executeAction('morosidadExcel', this.analyticsService.getMorosidadExcel(), blob => {
      this.downloadBlob(blob, `morosidad_${this.todayStamp()}.xlsx`);
      this.statusMessage = 'Reporte de morosidad generado correctamente.';
    }, () => {
      this.statusMessage = 'No se pudo generar el reporte de morosidad.';
    });
  }

  sendMorosidadEmail(): void {
    const recipients = this.parseRecipients();
    this.executeAction('morosidadEmail', this.analyticsService.sendMorosidadByEmail(recipients), result => {
      this.statusMessage = result.message;
    }, () => {
      this.statusMessage = 'Error enviando reporte de morosidad por email.';
    });
  }

  downloadReclamosSlaPdf(): void {
    this.executeAction('reclamosSlaPdf', this.analyticsService.getReclamosSlaPdf(), blob => {
      this.downloadBlob(blob, `reclamos_sla_${this.todayStamp()}.pdf`);
      this.statusMessage = 'Reporte ejecutivo de reclamos (SLA) generado correctamente.';
    }, () => {
      this.statusMessage = 'No se pudo generar el reporte de reclamos SLA.';
    });
  }

  downloadEstadoPortafolioPdf(): void {
    this.executeAction('estadoPortafolioPdf', this.analyticsService.getEstadoPortafolioPdf(), blob => {
      this.downloadBlob(blob, `estado_portafolio_${this.todayStamp()}.pdf`);
      this.statusMessage = 'Estado del portafolio generado correctamente.';
    }, () => {
      this.statusMessage = 'No se pudo generar el estado del portafolio.';
    });
  }

  sendEstadoPortafolioAdmins(): void {
    this.executeAction('estadoPortafolioEmail', this.analyticsService.sendEstadoPortafolioAdmin(), result => {
      this.statusMessage = result.message;
    }, () => {
      this.statusMessage = 'No se pudo enviar el estado del portafolio a administradores.';
    });
  }

  isBusy(key: string): boolean {
    return !!this.busy[key];
  }

  private executeAction<T>(
    key: string,
    request$: Observable<T>,
    onSuccess: (value: T) => void,
    onError: () => void
  ): void {
    this.busy[key] = true;
    request$.pipe(finalize(() => {
      this.busy[key] = false;
    })).subscribe({
      next: onSuccess,
      error: onError
    });
  }

  private parseRecipients(): string[] {
    return this.recipientsCsv
      .split(',')
      .map(r => r.trim())
      .filter(r => !!r);
  }

  private downloadBlob(blob: Blob, fileName: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  private safeSuffix(): string {
    return (this.aseguradora || 'todas').replace(/\s+/g, '_').toLowerCase();
  }

  private todayStamp(): string {
    return new Date().toISOString().split('T')[0].replace(/-/g, '');
  }
}
