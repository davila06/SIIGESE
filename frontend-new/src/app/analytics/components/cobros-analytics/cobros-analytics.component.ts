import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { NgxChartsModule, Color, ScaleType, LegendPosition } from '@swimlane/ngx-charts';

import {
  AnalyticsService,
  CobrosTrendDto,
  AgingReportDto,
  PagoMetodoDistribucionDto,
  CashflowForecastDto,
  TopDeudorDto,
  AgingBucketDto,
  HeatmapCell,
  AgenteCobrosDto
} from '../../services/analytics.service';
import { ChartCardComponent } from '../../shared/chart-card/chart-card.component';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-cobros-analytics',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTabsModule, MatTableModule,
    MatTooltipModule, MatSelectModule, MatFormFieldModule,
    NgxChartsModule, ChartCardComponent
  ],
  templateUrl: './cobros-analytics.component.html',
  styleUrls: ['./cobros-analytics.component.scss']
})
export class CobrosAnalyticsComponent implements OnInit {
  trend: CobrosTrendDto | null = null;
  aging: AgingReportDto | null = null;
  metodos: PagoMetodoDistribucionDto[] = [];
  cashflow: CashflowForecastDto | null = null;
  deudores: TopDeudorDto[] = [];
  agentes: AgenteCobrosDto[] = [];

  loading = true;
  error = '';
  trendMonths = 18;
  cashflowWeeks = 12;
  topN = 10;

  // ngx-charts datasets
  readonly legendBelow = LegendPosition.Below;
  trendAreaChart: any[] = [];
  agingBarChart: any[] = [];
  metodosDonut: any[] = [];
  cashflowBarChart: any[] = [];
  heatmapChart: any[] = [];
  agenteBarChart: any[] = [];

  // Labels for heatmap axes
  readonly DIAS_SEMANA = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];
  readonly HORAS       = Array.from({ length: 24 }, (_, i) => `${i.toString().padStart(2, '0')}h`);

  readonly colorScheme: Color = {
    name: 'cobros', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#F59E0B', '#EF4444', '#10B981']
  };
  readonly agingColors: Color = {
    name: 'aging', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#F59E0B', '#F97316', '#EF4444', '#DC2626', '#991B1B']
  };
  readonly metodosColors: Color = {
    name: 'metodos', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#8B5CF6', '#10B981', '#F59E0B', '#3B82F6']
  };
  readonly heatmapScheme: Color = {
    name: 'heatmap', selectable: false,
    group: ScaleType.Linear,
    domain: ['#0f172a', '#00E5FF']
  };
  readonly agenteScheme: Color = {
    name: 'agente', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#8B5CF6', '#10B981', '#F59E0B', '#3B82F6', '#EF4444']
  };

  displayedColumnsDeudores = ['clienteNombre', 'montoVencidoTotal', 'antiguedadMaxDias', 'numeroPolizas', 'acciones'];

  getAntiguedad(d: any): number { return d.antiguedadMaxDias ?? d.antiguedadMaxDias ?? 0; }

  constructor(private svc: AnalyticsService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      trend:    this.svc.getCobrosTrend(this.trendMonths),
      aging:    this.svc.getCobrosAging(),
      metodos:  this.svc.getCobrosMetodosPago(),
      cashflow: this.svc.getCashflowForecast(this.cashflowWeeks),
      deudores: this.svc.getTopDeudores(this.topN),
      heatmap:  this.svc.getCobrosHeatmap(),
      agente:   this.svc.getCobrosPorAgente()
    }).subscribe({
      next: ({ trend, aging, metodos, cashflow, deudores, heatmap, agente }) => {
        this.trend    = trend;
        this.aging    = aging;
        this.metodos  = metodos;
        this.cashflow = cashflow;
        this.deudores = deudores;
        this.agentes  = agente;
        this.buildCharts(heatmap);
        this.loading = false;
      },
      error: () => {
        this.error = 'Error cargando analítica de cobros.';
        this.loading = false;
      }
    });
  }

  private buildCharts(heatmapCells: HeatmapCell[]): void {
    if (this.trend) {
      this.trendAreaChart = [
        { name: 'Esperado', series: this.trend.mensual.map(m => ({ name: m.mesLabel, value: m.montoEsperado })) },
        { name: 'Cobrado',  series: this.trend.mensual.map(m => ({ name: m.mesLabel, value: m.montoCobrado })) },
        { name: 'Vencido',  series: this.trend.mensual.map(m => ({ name: m.mesLabel, value: m.montoVencido })) }
      ];
    }
    if (this.aging) {
      this.agingBarChart = this.aging.buckets.map(b => ({ name: b.rango, value: b.monto }));
    }
    if (this.metodos.length) {
      this.metodosDonut = this.metodos.map(m => ({ name: m.metodoPago, value: m.cantidad }));
    }
    if (this.cashflow) {
      this.cashflowBarChart = [
        { name: 'Esperado',   series: this.cashflow.semanas.map(s => ({ name: s.semana, value: s.montoEsperado })) },
        { name: 'Proyectado', series: this.cashflow.semanas.map(s => ({ name: s.semana, value: s.montoCobrado })) }
      ];
    }
    this.buildHeatmap(heatmapCells);
    this.agenteBarChart = this.agentes.map(a => ({
      name:  this.shortName(a.nombreAgente),
      value: a.montoCobrado
    }));
  }

  private buildHeatmap(cells: HeatmapCell[]): void {
    const grid: number[][] = Array.from({ length: 7 }, () => new Array(24).fill(0));
    cells.forEach(c => {
      const d = Math.max(0, Math.min(6, c.diaSemana));
      const h = Math.max(0, Math.min(23, c.hora));
      grid[d][h] = c.valor;
    });
    this.heatmapChart = this.DIAS_SEMANA.map((dia, d) => ({
      name:   dia,
      series: this.HORAS.map((hora, h) => ({ name: hora, value: grid[d][h] }))
    }));
  }

  private shortName(name: string): string {
    const parts = name.trim().split(' ');
    if (parts.length >= 2) return `${parts[0]} ${parts[1].charAt(0)}.`;
    return name;
  }

  formatCurrency(v: number): string {
    return new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 }).format(v);
  }

  yAxisCurrency = (v: number) =>
    new Intl.NumberFormat('es-CR', { notation: 'compact', minimumFractionDigits: 0 }).format(v);

  agingClass(bucket: AgingBucketDto): string {
    if (bucket.diasMin >= 91) return 'chip--critico';
    if (bucket.diasMin >= 61) return 'chip--danger';
    if (bucket.diasMin >= 31) return 'chip--warning';
    return 'chip--info';
  }

  tiempoClass(dias: number): string {
    if (dias > 15) return 'chip--critico';
    if (dias > 7)  return 'chip--warning';
    if (dias > 0)  return 'chip--info';
    return 'chip--success';
  }
}
