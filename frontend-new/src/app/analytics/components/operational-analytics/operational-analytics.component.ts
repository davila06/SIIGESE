import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTableModule } from '@angular/material/table';
import { NgxChartsModule, Color, ScaleType, LegendPosition } from '@swimlane/ngx-charts';

import {
  AnalyticsService,
  OperacionalStatsDto
} from '../../services/analytics.service';
import { ChartCardComponent } from '../../shared/chart-card/chart-card.component';

@Component({
  selector: 'app-operational-analytics',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTooltipModule, MatTableModule,
    NgxChartsModule, ChartCardComponent
  ],
  templateUrl: './operational-analytics.component.html',
  styleUrls: ['./operational-analytics.component.scss']
})
export class OperationalAnalyticsComponent implements OnInit {
  data: OperacionalStatsDto | null = null;
  loading = true;
  error = '';

  cargaChart: any[] = [];
  slaAgenteChart: any[] = [];
  chatWeeklyChart: any[] = [];
  endpointP90Chart: any[] = [];
  heatmapColumns: string[] = [];

  readonly legendBelow = LegendPosition.Below;
  readonly colorScheme: Color = {
    name: 'operacional', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#10B981', '#8B5CF6', '#F59E0B', '#3B82F6']
  };
  readonly slaColors: Color = {
    name: 'slaOp', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#10B981', '#EF4444']
  };

  constructor(private readonly svc: AnalyticsService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    this.svc.getOperacional().subscribe({
      next: data => {
        this.data = data;
        this.buildCharts(data);
        this.loading = false;
      },
      error: () => {
        this.error = 'Error cargando dashboard operacional.';
        this.loading = false;
      }
    });
  }

  private buildCharts(d: OperacionalStatsDto): void {
    this.cargaChart = [
      { name: 'Activos', series: d.cargaReclamos.map(a => ({ name: a.nombreAgente, value: a.reclamosActivos })) },
      { name: 'Criticos', series: d.cargaReclamos.map(a => ({ name: a.nombreAgente, value: a.reclamosCriticos })) }
    ];

    this.slaAgenteChart = d.cargaReclamos.map(a => ({ name: a.nombreAgente, value: a.porcentajeSla }));

    this.chatWeeklyChart = [
      { name: 'Activas', series: d.chatIA.sesionesPorSemana.map(s => ({ name: s.semana, value: s.activas })) },
      { name: 'Cerradas', series: d.chatIA.sesionesPorSemana.map(s => ({ name: s.semana, value: s.cerradas })) }
    ];

    this.endpointP90Chart = d.sistemaRendimiento.endpoints
      .slice(0, 8)
      .map(e => ({ name: e.endpoint, value: e.p90Ms }));

    this.heatmapColumns = d.actividadHeatmapPorAgente.length > 0
      ? d.actividadHeatmapPorAgente[0].semanas.map(s => s.semana)
      : [];
  }

  slaClass(score: number): string {
    if (score >= 90) return 'chip--success';
    if (score >= 70) return 'chip--warning';
    return 'chip--danger';
  }

  accesoClass(dias: number): string {
    if (dias <= 2) return 'chip--success';
    if (dias <= 7) return 'chip--warning';
    return 'chip--danger';
  }
}
