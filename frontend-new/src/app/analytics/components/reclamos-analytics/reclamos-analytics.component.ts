import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { NgxChartsModule, Color, ScaleType, LegendPosition } from '@swimlane/ngx-charts';

import {
  AnalyticsService,
  ReclamosAnalyticsAdvancedDto,
  ReclamosFunnelDto,
  SlaReportDto,
  FunnelEtapaDto,
  RendimientoAgenteDto
} from '../../services/analytics.service';
import { ChartCardComponent } from '../../shared/chart-card/chart-card.component';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-reclamos-analytics',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTooltipModule,
    MatTableModule, MatTabsModule, MatChipsModule, MatFormFieldModule, MatSelectModule,
    NgxChartsModule, ChartCardComponent
  ],
  templateUrl: './reclamos-analytics.component.html',
  styleUrls: ['./reclamos-analytics.component.scss']
})
export class ReclamosAnalyticsComponent implements OnInit {
  funnel: ReclamosFunnelDto | null = null;
  sla: SlaReportDto | null = null;
  advanced: ReclamosAnalyticsAdvancedDto | null = null;

  // Filtros 3.2 y 3.7
  months = 12;
  selectedAgenteId: number | null = null;
  selectedAseguradora = '';

  loading = true;
  error = '';

  funnelChart: any[] = [];
  tipoBarChart: any[] = [];
  prioridadChart: any[] = [];
  lossRatioAsegChart: any[] = [];
  lossRatioTipoChart: any[] = [];
  resolucionTipoChart: any[] = [];
  resolucionPrioridadChart: any[] = [];
  resolucionAgenteChart: any[] = [];
  resolucionAseguradoraChart: any[] = [];
  scatterChart: any[] = [];

  heatmapMeses: string[] = [];
  heatmapTipos: string[] = [];
  heatmapValues: Map<string, number> = new Map();
  heatmapMax = 1;

  readonly rendimientoColumns = ['agente', 'cerrados', 'tiempo', 'sla', 'ratio', 'sparkline'];

  readonly legendBelow = LegendPosition.Below;
  readonly colorSla: Color = {
    name: 'sla', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#10B981', '#EF4444']
  };
  readonly funnelColors: Color = {
    name: 'funnel', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#8B5CF6', '#3B82F6', '#F59E0B', '#10B981', '#EF4444', '#6B7280']
  };
  readonly lossRatioColors: Color = {
    name: 'loss', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#EF4444', '#F59E0B', '#00E5FF', '#10B981', '#8B5CF6']
  };
  readonly resolucionColors: Color = {
    name: 'resolucion', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#8B5CF6', '#00E5FF', '#10B981', '#F59E0B']
  };
  readonly scatterColors: Color = {
    name: 'scatter', selectable: false,
    group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6']
  };

  constructor(private svc: AnalyticsService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      funnel: this.svc.getReclamosFunnel(),
      sla: this.svc.getReclamosSla(),
      advanced: this.svc.getReclamosAdvanced(
        this.months,
        this.selectedAgenteId ?? undefined,
        this.selectedAseguradora || undefined
      )
    }).subscribe({
      next: ({ funnel, sla, advanced }) => {
        this.funnel = funnel;
        this.sla = sla;
        this.advanced = advanced;
        this.buildCharts();
        this.loading = false;
      },
      error: () => {
        this.error = 'Error cargando analítica de reclamos.';
        this.loading = false;
      }
    });
  }

  private buildCharts(): void {
    if (this.funnel) {
      this.funnelChart = this.funnel.etapas.map(e => ({ name: e.nombre, value: e.cantidad }));
    }

    if (this.sla) {
      this.tipoBarChart = [
        { name: 'Dentro SLA',  series: this.sla.porTipo.map(t => ({ name: t.nombre, value: t.dentroSla })) },
        { name: 'Fuera SLA',   series: this.sla.porTipo.map(t => ({ name: t.nombre, value: t.fueraSla })) }
      ];
      this.prioridadChart = this.sla.porPrioridad.map(p => ({
        name: p.nombre,
        value: p.porcentajeDentroSla
      }));
    }

    if (this.advanced) {
      this.lossRatioAsegChart = this.advanced.lossRatioPorAseguradora.map(x => ({
        name: x.segmento,
        value: x.lossRatio
      }));

      this.lossRatioTipoChart = this.advanced.lossRatioPorTipo.map(x => ({
        name: x.segmento,
        value: x.lossRatio
      }));

      this.resolucionTipoChart = this.advanced.resolucionPorTipo.map(x => ({ name: x.grupo, value: x.promedioDias }));
      this.resolucionPrioridadChart = this.advanced.resolucionPorPrioridad.map(x => ({ name: x.grupo, value: x.promedioDias }));
      this.resolucionAgenteChart = this.advanced.resolucionPorAgente.map(x => ({ name: x.grupo, value: x.promedioDias }));
      this.resolucionAseguradoraChart = this.advanced.resolucionPorAseguradora.map(x => ({ name: x.grupo, value: x.promedioDias }));

      const scatterByTipo = this.advanced.montoReclamadoVsAprobado
        .reduce((acc, point) => {
          const found = acc.find(s => s.name === point.tipoReclamo);
          const node = {
            name: point.numeroReclamo,
            x: point.montoReclamado,
            y: point.montoAprobado,
            r: Math.max(2, Math.min(20, point.duracionDias || 1))
          };
          if (found) {
            found.series.push(node);
          } else {
            acc.push({ name: point.tipoReclamo, series: [node] });
          }
          return acc;
        }, [] as any[]);
      this.scatterChart = scatterByTipo;

      this.buildHeatmap();
    }
  }

  private buildHeatmap(): void {
    if (!this.advanced) {
      return;
    }

    const cells = this.advanced.heatmapMesTipo;
    const monthSet = [...new Set(cells.map(c => c.mes))].sort();
    const tipoSet = [...new Set(cells.map(c => c.tipoReclamo))].sort();

    this.heatmapMeses = monthSet;
    this.heatmapTipos = tipoSet;
    this.heatmapValues = new Map<string, number>();

    let maxValue = 1;
    for (const cell of cells) {
      const key = `${cell.tipoReclamo}|${cell.mes}`;
      this.heatmapValues.set(key, cell.cantidad);
      maxValue = Math.max(maxValue, cell.cantidad);
    }
    this.heatmapMax = maxValue;
  }

  getHeatmapValue(tipo: string, mes: string): number {
    return this.heatmapValues.get(`${tipo}|${mes}`) ?? 0;
  }

  getHeatmapColor(tipo: string, mes: string): string {
    const value = this.getHeatmapValue(tipo, mes);
    const ratio = this.heatmapMax > 0 ? value / this.heatmapMax : 0;
    const alpha = 0.08 + ratio * 0.92;
    return `rgba(0, 229, 255, ${alpha.toFixed(2)})`;
  }

  onAgenteChange(value: string | number | null): void {
    if (value === null || value === '' || value === undefined) {
      this.selectedAgenteId = null;
      return;
    }
    this.selectedAgenteId = Number(value);
  }

  performanceClass(value: number): string {
    if (value >= 90) return 'chip--success';
    if (value >= 70) return 'chip--warning';
    return 'chip--danger';
  }

  ratioClass(value: number): string {
    if (value >= 70) return 'chip--success';
    if (value >= 50) return 'chip--warning';
    return 'chip--danger';
  }

  totalReclamosAnalizados(): number {
    if (!this.advanced?.heatmapMesTipo?.length) {
      return 0;
    }
    return this.advanced.heatmapMesTipo.reduce((acc, curr) => acc + curr.cantidad, 0);
  }

  monthLabel(monthToken: string): string {
    if (!monthToken?.includes('-')) return monthToken;
    const [year, month] = monthToken.split('-').map(Number);
    const date = new Date(year, month - 1, 1);
    return date.toLocaleDateString('es-CR', { month: 'short', year: '2-digit' });
  }

  yAxisCurrency = (value: number) => new Intl.NumberFormat('es-CR', {
    notation: 'compact',
    maximumFractionDigits: 1
  }).format(value);

  xAxisDays = (value: number) => `${value.toFixed(1)}d`;

  gaugeValue(): { name: string; value: number }[] {
    if (!this.sla) return [];
    return [{ name: 'SLA', value: this.sla.porcentajeGlobalDentroSLA }];
  }

  sparklineSeries(row: RendimientoAgenteDto): any[] {
    return [{
      name: row.agente,
      series: row.sparklineCierresMensuales.map(p => ({ name: p.name, value: p.value }))
    }];
  }

  etapaWidth(etapa: FunnelEtapaDto): string {
    return `${Math.max(etapa.porcentajeDel100, 5)}%`;
  }

  slaClass(pct: number): string {
    if (pct >= 90) return 'chip--success';
    if (pct >= 70) return 'chip--warning';
    return 'chip--danger';
  }

  formatCurrency(v: number): string {
    return new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 }).format(v);
  }
}
