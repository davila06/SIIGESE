import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTableModule } from '@angular/material/table';
import { NgxChartsModule, Color, ScaleType, LegendPosition } from '@swimlane/ngx-charts';

import {
  AnalyticsService,
  EmailAnalyticsDto,
  EmailCorrelacionCanalDto
} from '../../services/analytics.service';
import { ChartCardComponent } from '../../shared/chart-card/chart-card.component';

@Component({
  selector: 'app-email-analytics',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTooltipModule,
    MatSelectModule, MatFormFieldModule, MatTableModule,
    NgxChartsModule, ChartCardComponent
  ],
  templateUrl: './email-analytics.component.html',
  styleUrls: ['./email-analytics.component.scss']
})
export class EmailAnalyticsComponent implements OnInit {
  data: EmailAnalyticsDto | null = null;
  loading = true;
  error = '';
  days = 30;

  volumenChart: any[] = [];
  tipoChart: any[] = [];
  coberturaChart: any[] = [];
  correlacionChart: any[] = [];
  roiChart: any[] = [];
  correlacion: EmailCorrelacionCanalDto | null = null;

  // Heatmap grid: 7 days x 24 hours
  heatmapGrid: number[][] = Array.from({ length: 7 }, () => new Array(24).fill(0));
  heatmapMax = 1;
  diasLabels = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];

  readonly colorScheme: Color = {
    name: 'email', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#EF4444', '#10B981']
  };
  readonly legendBelow = LegendPosition.Below;
  readonly coberturaColors: Color = {
    name: 'cobertura', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#10B981', '#F59E0B', '#6B7280']
  };
  readonly correlacionColors: Color = {
    name: 'correlacion', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#10B981', '#3B82F6', '#F59E0B', '#EF4444']
  };
  readonly roiColors: Color = {
    name: 'roi', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#10B981', '#6B7280']
  };

  constructor(private svc: AnalyticsService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    this.svc.getEmailAnalytics(this.days).subscribe({
      next: data => {
        this.data = data;
        this.buildCharts(data);
        this.loading = false;
      },
      error: () => {
        this.error = 'Error cargando analítica de emails.';
        this.loading = false;
      }
    });
  }

  private buildCharts(d: EmailAnalyticsDto): void {
    this.volumenChart = [
      { name: 'Exitosos', series: d.volumenPorDia.map(v => ({ name: v.fechaLabel, value: v.exitosos })) },
      { name: 'Fallidos', series: d.volumenPorDia.map(v => ({ name: v.fechaLabel, value: v.fallidos })) }
    ];
    this.tipoChart = d.porTipo.map(t => ({ name: t.tipo, value: t.enviados }));
    if (d.coberturaCobros) {
      this.coberturaChart = [
        { name: 'Notificados',     value: d.coberturaCobros.conEmailNotificado },
        { name: 'No notificados',  value: d.coberturaCobros.conEmailNoNotificado },
        { name: 'Sin email',       value: d.coberturaCobros.sinEmail }
      ];
    }

    // Build heatmap grid
    this.heatmapGrid = Array.from({ length: 7 }, () => new Array(24).fill(0));
    for (const cell of d.heatmapEnvios) {
      const dia = Math.max(0, Math.min(6, cell.diaSemana));
      const hora = Math.max(0, Math.min(23, cell.hora));
      this.heatmapGrid[dia][hora] += cell.valor;
    }
    this.heatmapMax = Math.max(1, ...this.heatmapGrid.flat());

    // 5.4 Correlación Email → Cobro
    if (d.correlacionCanal) {
      this.correlacion = d.correlacionCanal;
      this.correlacionChart = (d.correlacionCanal.bucketsConEmail ?? []).map(b => ({
        name: b.etiqueta,
        value: b.cantidad
      }));
      this.roiChart = [
        { name: 'Con email', value: d.correlacionCanal.tasaPagoConEmail },
        { name: 'Sin email',  value: d.correlacionCanal.tasaPagoSinEmail }
      ];
    }
  }

  heatmapColor(val: number): string {
    const ratio = val / this.heatmapMax;
    const alpha = 0.1 + ratio * 0.85;
    return `rgba(0, 229, 255, ${alpha.toFixed(2)})`;
  }

  formatPct = (val: number) => `${val}%`;

  heatmapHours = Array.from({ length: 24 }, (_, i) => `${i}h`);
}
