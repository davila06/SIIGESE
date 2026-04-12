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
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { NgxChartsModule, Color, ScaleType, LegendPosition } from '@swimlane/ngx-charts';

import {
  AnalyticsService,
  PredictivoAnalyticsDto,
  PredictivoForecastPrimaDto,
  PredictivoRenovacionDto,
  PredictivoAnomaliasCobrosDto,
  ChartDataPoint,
  MultiSeriesChart
} from '../../services/analytics.service';
import { ChartCardComponent } from '../../shared/chart-card/chart-card.component';

@Component({
  selector: 'app-predictive-analytics',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTooltipModule,
    MatSelectModule, MatFormFieldModule, MatTableModule,
    MatChipsModule, MatBadgeModule,
    NgxChartsModule, ChartCardComponent
  ],
  templateUrl: './predictive-analytics.component.html',
  styleUrls: ['./predictive-analytics.component.scss']
})
export class PredictiveAnalyticsComponent implements OnInit {

  data: PredictivoAnalyticsDto | null = null;
  loading = true;
  error = '';

  // 7.1 Score Morosidad
  riesgoDonutChart: any[] = [];
  riesgoTableCols = ['poliza', 'nombre', 'tipo', 'score', 'categoria', 'tasaVencido'];

  // 7.2 Reclamos Temporada
  historicoBarChart: any[] = [];
  prediccionTableCols = ['mes', 'esperado', 'anioAnterior', 'cambio'];

  // 7.3 Anomalías
  anomaliasTableCols = ['recibo', 'cliente', 'monto', 'media', 'zscore', 'tipo', 'estado'];

  // 7.4 Renovación / Lead Scoring
  leadDonutChart: any[] = [];
  leadTableCols = ['poliza', 'nombre', 'aseguradora', 'prima', 'dias', 'score', 'factores'];

  // 7.5 Forecast
  forecastChart: any[] = [];
  forecastPrima: PredictivoForecastPrimaDto | null = null;
  renovacion: PredictivoRenovacionDto | null = null;
  anomalias: PredictivoAnomaliasCobrosDto | null = null;

  readonly riesgoColors: Color = {
    name: 'riesgo', selectable: false, group: ScaleType.Ordinal,
    domain: ['#10B981', '#F59E0B', '#EF4444']
  };
  readonly leadColors: Color = {
    name: 'lead', selectable: false, group: ScaleType.Ordinal,
    domain: ['#EF4444', '#F59E0B', '#6B7280']
  };
  readonly forecastColors: Color = {
    name: 'forecast', selectable: false, group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#10B981']
  };
  readonly historicoColor: Color = {
    name: 'historico', selectable: false, group: ScaleType.Ordinal,
    domain: ['#8B5CF6']
  };
  readonly legendBelow = LegendPosition.Below;

  constructor(private svc: AnalyticsService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    this.svc.getPredictivo().subscribe({
      next: data => {
        this.data = data;
        this.buildCharts(data);
        this.loading = false;
      },
      error: () => {
        this.error = 'Error cargando analítica predictiva.';
        this.loading = false;
      }
    });
  }

  private buildCharts(d: PredictivoAnalyticsDto): void {
    // 7.1 — Risk distribution donut
    this.riesgoDonutChart = (d.scoreMorosidad.distribucionRiesgo ?? []).map(p => ({
      name: p.name,
      value: p.value
    }));

    // 7.2 — Historical bar chart
    this.historicoBarChart = (d.reclamosTemporada.historicoSerie ?? []).map((s: MultiSeriesChart) => ({
      name: s.name,
      series: s.series.map((p: ChartDataPoint) => ({ name: p.name, value: p.value }))
    }));

    // 7.4 — Lead score distribution
    this.leadDonutChart = (d.renovacion.distribucionScore ?? []).map(p => ({
      name: p.name,
      value: p.value
    }));
    this.renovacion = d.renovacion;

    // 7.3
    this.anomalias = d.anomaliasCobros;

    // 7.5 — Forecast grouped bar
    this.forecastPrima = d.forecastPrima;
    const proy = d.forecastPrima.proyeccionMensual ?? [];
    this.forecastChart = [
      { name: 'Esperado',   series: proy.map(m => ({ name: m.mesLabel, value: Number(m.montoEsperado) })) },
      { name: 'Proyectado', series: proy.map(m => ({ name: m.mesLabel, value: Number(m.montoProyectado) })) }
    ];
  }

  getRiskClass(categoria: string): string {
    return categoria === 'Rojo' ? 'chip--danger'
      : categoria === 'Amarillo' ? 'chip--warning'
      : 'chip--success';
  }

  getAnomaliaClass(tipo: string): string {
    return tipo === 'Alto' ? 'chip--danger' : 'chip--info';
  }

  formatCRC = (val: number) => `₡${(val / 1000).toFixed(0)}k`;

  formatPct = (val: number) => `${val}%`;
}
