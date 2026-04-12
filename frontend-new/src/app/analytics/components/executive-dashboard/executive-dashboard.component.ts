import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { NgxChartsModule, Color, ScaleType, LegendPosition } from '@swimlane/ngx-charts';

import {
  AnalyticsService,
  ExecutiveDashboardDto,
  AlertaDto,
  ChartDataPoint,
  MultiSeriesChart
} from '../../services/analytics.service';
import { ChartCardComponent } from '../../shared/chart-card/chart-card.component';

@Component({
  selector: 'app-executive-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTooltipModule, MatDividerModule,
    NgxChartsModule, ChartCardComponent
  ],
  templateUrl: './executive-dashboard.component.html',
  styleUrls: ['./executive-dashboard.component.scss']
})
export class ExecutiveDashboardComponent implements OnInit {
  data: ExecutiveDashboardDto | null = null;
  loading = true;
  error = '';

  // ── Color schemes ──────────────────────────────────────────────────────────
  readonly legendBelow = LegendPosition.Below;

  /** Cobrado=cyan / Esperado=amber */
  readonly cobrosLineScheme: Color = {
    name: 'cobrosLine', selectable: false, group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#F59E0B']
  };
  /** Stacked bar: Cobrado / Pendiente / Vencido / Cancelado */
  readonly estadoScheme: Color = {
    name: 'estado', selectable: false, group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#F59E0B', '#EF4444', '#6B7280']
  };
  /** Pie distribución aseguradoras */
  readonly asegColorScheme: Color = {
    name: 'aseg', selectable: false, group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#8B5CF6', '#10B981', '#F59E0B', '#EF4444', '#3B82F6']
  };
  /** Gauge tasa cobro */
  readonly gaugeScheme: Color = {
    name: 'gauge', selectable: false, group: ScaleType.Ordinal,
    domain: ['#00E5FF', '#F59E0B', '#EF4444']
  };
  /** Sparkline neutral */
  readonly sparklineScheme: Color = {
    name: 'spark', selectable: false, group: ScaleType.Ordinal,
    domain: ['#00E5FF']
  };
  readonly sparklineRiesgoScheme: Color = {
    name: 'sparkRiesgo', selectable: false, group: ScaleType.Ordinal,
    domain: ['#EF4444']
  };

  // Sparkline view: compact fixed size [width, height]
  readonly sparklineView: [number, number] = [100, 36];

  constructor(private svc: AnalyticsService, private router: Router) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.error = '';
    this.svc.getDashboard().subscribe({
      next: data => { this.data = data; this.loading = false; },
      error: () => {
        this.error = 'No se pudo cargar el dashboard ejecutivo.';
        this.loading = false;
      }
    });
  }

  // ── Derived data for charts ────────────────────────────────────────────────

  get gaugeData(): { name: string; value: number }[] {
    return this.data ? [{ name: 'Tasa de Cobro', value: Number(this.data.tasaCobro) }] : [];
  }

  get sparklineTasoCobroChart(): MultiSeriesChart[] {
    if (!this.data?.sparklineTasaCobro?.length) return [];
    return [{ name: 'Tasa', series: this.data.sparklineTasaCobro }];
  }

  get sparklineRiesgoChart(): MultiSeriesChart[] {
    if (!this.data?.sparklineMontoRiesgo?.length) return [];
    return [{ name: 'Riesgo', series: this.data.sparklineMontoRiesgo }];
  }

  // ── Helper getters ─────────────────────────────────────────────────────────

  get hasCriticoAlert(): boolean {
    return this.data?.alertas?.some(a => a.tipo === 'CRITICO') ?? false;
  }

  alertaClass(tipo: string): string {
    return ({
      CRITICO: 'alerta--critico',
      ALERTA:  'alerta--alerta',
      AVISO:   'alerta--aviso',
      INFO:    'alerta--info'
    } as Record<string, string>)[tipo] ?? '';
  }

  navigate(ruta: string): void { this.router.navigateByUrl(ruta); }

  formatCurrency(v: number): string {
    return new Intl.NumberFormat('es-CR', {
      style: 'currency', currency: 'CRC', minimumFractionDigits: 0
    }).format(v);
  }

  yAxisFormat = (v: number) =>
    new Intl.NumberFormat('es-CR', {
      style: 'currency', currency: 'CRC', notation: 'compact', minimumFractionDigits: 0
    }).format(v);

  percentFormat = (v: number) => `${v.toFixed(0)}%`;
}
